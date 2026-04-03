using UnityEngine;
using UnityEngine.UI;

public class WoodenBoxController : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject boxViewPanel;
    public Camera boxCameraUI;
    public RawImage boxDisplay;          // 用于显示相机画面的 RawImage，同时作为点击触发器
    public GameObject manuscript;        // 手札物体

    [Header("拖拽设置")]
    public float dragDistance = 0.5f;

    [Header("手札相机")]
    public float manuscriptViewDistance = 1.5f; // 查看手札时的相机距离

    private bool isInBoxView = false;
    private bool isManuscriptView = false; // 是否处于手札查看模式
    private int currentStep = 0;
    private readonly string[] stepOrder = { "001", "002", "003", "004", "005", "006" };
    private Quaternion originalManuscriptRotation;
    private Vector3 originalManuscriptScale;

    // 保存盒身原有的渲染器和碰撞器状态（用于恢复）
    private Renderer[] boxRenderers;
    private Collider[] boxColliders;

    void Start()
    {
        if (boxViewPanel != null) boxViewPanel.SetActive(false);
        if (boxCameraUI != null) boxCameraUI.gameObject.SetActive(false);
        if (manuscript != null) manuscript.SetActive(false);

        // 记录盒身的所有渲染器和碰撞器（包括所有子物体）
        boxRenderers = GetComponentsInChildren<Renderer>();
        boxColliders = GetComponentsInChildren<Collider>();

        // 为 boxDisplay 添加按钮监听，确保点击 RawImage 时调用 OnBoxClick
        if (boxDisplay != null)
        {
            var btn = boxDisplay.gameObject.GetComponent<Button>();
            if (btn == null) btn = boxDisplay.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners(); // 避免重复添加
            btn.onClick.AddListener(OnBoxClick);
        }

        // 确保手札上有 ClickableDialogue 组件（用户手动添加）
        if (manuscript != null)
        {
            var clickable = manuscript.GetComponent<ClickableDialogue>();
            if (clickable == null)
                Debug.LogWarning("手札物体缺少 ClickableDialogue 组件，请手动添加并设置起始对话ID。");
        }
    }

    public void EnterBoxView()
    {
        if (isInBoxView) return;

        // 强制结束任何正在进行的对话树
        if (DialogueTreeManager.Instance != null)
            DialogueTreeManager.Instance.EndDialogue();
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.EndDialogue();

        isInBoxView = true;
        currentStep = 0;
        isManuscriptView = false;

        boxViewPanel.SetActive(true);
        boxCameraUI.gameObject.SetActive(true);

        // 对话部分假设 DialogueManager 在主Canvas下，直接调用
        DialogueManager.Instance.panel.SetActive(true);
        DialogueManager.Instance.StartDialogue("系统", new string[] { "请按照顺序抽出木条：001 → 002 → 003 → 004 → 005 → 006（顶板）" });

        var router = FindObjectOfType<ClickRouter2D>();
        if (router != null) router.cam = boxCameraUI;
    }

    public void ExitBoxView()
    {
        if (!isInBoxView) return;
        isInBoxView = false;

        // 如果处于手札查看模式，先恢复盒身显示
        if (isManuscriptView)
            RestoreBoxView();

        boxViewPanel.SetActive(false);
        boxCameraUI.gameObject.SetActive(false);

        DialogueManager.Instance.panel.SetActive(false);
        if (manuscript != null) manuscript.SetActive(false);

        var router = FindObjectOfType<ClickRouter2D>();
        if (router != null) router.cam = Camera.main; // 恢复为主相机

    }

    // 注意：此方法必须是 public，以便按钮调用
    public void OnBoxClick()
    {
        Debug.Log("OnBoxClick triggered");
        if (!isInBoxView) return;

        // 解谜完成后，不再处理零件拖拽，让手札等物体通过 ClickRouter2D 处理
        if (currentStep >= stepOrder.Length)
        {
            Debug.Log("解谜已完成，点击由其他组件处理");
            return;
        }

        int layerMask = LayerMask.GetMask("Draggable");
        Ray ray = boxCameraUI.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

        if (hits.Length == 0)
        {
            Debug.Log("未命中任何零件");
            return;
        }

        string expectedPart = stepOrder[currentStep];

        foreach (var hit in hits)
        {
            var dragHandler = hit.collider.GetComponent<DragHandler>();
            if (dragHandler != null && dragHandler.partType == expectedPart)
            {
                dragHandler.StartDrag(boxCameraUI);
                return;
            }
        }
        Debug.Log($"当前需要拖拽 {expectedPart}，请点击正确的零件");
    }

    public bool CanPull(string partType)
    {
        return partType == stepOrder[currentStep];
    }

    public void OnPartPulled(string partType)
    {
        if (partType != stepOrder[currentStep]) return;

        Transform part = transform.Find(partType);
        if (part != null) part.gameObject.SetActive(false);
        else Debug.LogWarning($"未找到部件 {partType}");

        currentStep++;
        //DialogueManager.Instance.StartDialogue("系统", new string[] { $"已取出 {partType}，请继续。" });

        if (currentStep >= stepOrder.Length)
        {
            OpenBox();
        }
    }

    void OpenBox()
    {
        if (manuscript != null)
        {
            manuscript.SetActive(true);

            var clickable = manuscript.GetComponent<ClickableDialogue>();
            if (clickable != null)
            {
                clickable.startDialogueId = 15;
                Debug.Log($"手札已激活，对话起始ID设置为 {clickable.startDialogueId}");
            }
        }

        if (boxDisplay != null)
        {
            Button btn = boxDisplay.GetComponent<Button>();
            if (btn != null)
            {
                btn.enabled = false;   // 禁用按钮，不再响应点击
                                       // 也可移除监听：btn.onClick.RemoveListener(OnBoxClick);
            }
        }
    }

    /// <summary>
    /// 显示手札正面视图，隐藏盒身
    /// </summary>
    public void ShowManuscriptView()
    {
        if (manuscript == null) return;
        if (isManuscriptView) return;

        isManuscriptView = true;

        originalManuscriptRotation = manuscript.transform.localRotation;
        originalManuscriptScale = manuscript.transform.localScale;

        // 调整手札：旋转90度（绕Z轴，使横向显示）并缩小到原来的0.7倍
        manuscript.transform.localRotation = Quaternion.Euler(0, 90, 0);
        manuscript.transform.localScale = originalManuscriptScale * 0.05f;

        // 隐藏盒身的所有渲染器和碰撞器（排除手札自身及其子物体）
        foreach (var rend in boxRenderers)
        {
            if (rend.gameObject != manuscript && !rend.transform.IsChildOf(manuscript.transform))
                rend.enabled = false;
        }
        foreach (var col in boxColliders)
        {
            if (col.gameObject != manuscript && !col.transform.IsChildOf(manuscript.transform))
                col.enabled = false;
        }

        // 调整相机，使手札正面显示
        PositionCameraForManuscript();

        // 更改手札上的 ClickableDialogue 的起始 ID，指向手札内容对话（例如 102）
        var clickable = manuscript.GetComponent<ClickableDialogue>();
        if (clickable != null)
        {
            clickable.startDialogueId = 15; // 请根据实际对话树设置内容节点ID
        }
    }

    void PositionCameraForManuscript()
    {
        if (boxCameraUI == null || manuscript == null) return;

        // 计算手札的中心和正面方向（假设手札的 forward 是正面）
        Vector3 targetPos = manuscript.transform.position;
        Vector3 forwardDir = manuscript.transform.forward;
        // 相机放置在手札正面，距离 manuscriptViewDistance 处
        Vector3 cameraPos = targetPos - forwardDir * manuscriptViewDistance;
        // 可选：调整高度和水平位置
        cameraPos.y = targetPos.y; // 保持同一高度

        boxCameraUI.transform.position = cameraPos;
        boxCameraUI.transform.LookAt(targetPos);
    }

    void RestoreBoxView()
    {
        // 恢复盒身的渲染器和碰撞器
        foreach (var rend in boxRenderers)
        {
            if (rend.gameObject != manuscript && !rend.transform.IsChildOf(manuscript.transform))
                rend.enabled = true;
        }
        foreach (var col in boxColliders)
        {
            if (col.gameObject != manuscript && !col.transform.IsChildOf(manuscript.transform))
                col.enabled = true;
        }

        // 恢复手札的旋转和缩放
        if (manuscript != null)
        {
            manuscript.transform.localRotation = originalManuscriptRotation;
            manuscript.transform.localScale = originalManuscriptScale;
        }

        isManuscriptView = false;
    }
}