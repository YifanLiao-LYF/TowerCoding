using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WoodenBoxController : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject boxViewPanel;          // 盒子视图的整体面板（包含 RawImage 等）
    public Camera boxCameraUI;
    public RawImage boxDisplay;

    [Header("手札全屏遮罩面板")]
    public GameObject manuscriptPanel;       // 全屏半透明黑色面板（带 Button，内含手札图片和文字）

    [Header("动画设置（全局默认值）")]
    public float pullDuration = 0.3f;
    public Vector3 pullDirection = new Vector3(0, 1, 0);
    public float pullDistance = 1.5f;

    [Header("手札对话内容")]
    public string[] manuscriptDialogueLines = new string[] { "这是手札的内容。" };

    private bool isInBoxView = false;
    private int currentStep = 0;
    private bool isPuzzleCompleted = false;
    private readonly string[] stepOrder = { "001", "002", "003", "004", "005", "006" };
    public bool HasUnlockedManuscript { get; private set; } = false;

    void Start()
    {
        // 初始隐藏所有面板
        if (boxViewPanel != null) boxViewPanel.SetActive(false);
        if (boxCameraUI != null) boxCameraUI.gameObject.SetActive(false);
        if (manuscriptPanel != null) manuscriptPanel.SetActive(false);

        // 为 RawImage 添加点击监听（进入盒子模式后用于点击零件或显示手札）
        if (boxDisplay != null)
        {
            var btn = boxDisplay.gameObject.GetComponent<Button>();
            if (btn == null) btn = boxDisplay.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnBoxClick);
        }

        // 为手札面板的 Button 添加监听（点击任意位置触发对话）
        if (manuscriptPanel != null)
        {
            Button panelBtn = manuscriptPanel.GetComponent<Button>();
            if (panelBtn != null)
            {
                panelBtn.onClick.RemoveAllListeners();
                panelBtn.onClick.AddListener(OnManuscriptPanelClicked);
            }
            else
            {
                Debug.LogWarning("ManuscriptPanel 缺少 Button 组件，无法响应点击");
            }
        }
    }

    public void EnterBoxView()
    {
        if (isInBoxView) return;

        // 结束可能存在的对话
        if (DialogueTreeManager.Instance != null)
            DialogueTreeManager.Instance.EndDialogue();
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.EndDialogue();

        AdjustCameraToParts();
        boxDisplay.texture = boxCameraUI.targetTexture;

        isInBoxView = true;
        currentStep = 0;
        isPuzzleCompleted = false;

        boxViewPanel.SetActive(true);
        boxCameraUI.gameObject.SetActive(true);

        DialogueManager.Instance.panel.SetActive(true);
        DialogueManager.Instance.StartDialogue("系统", new string[] { "请按照顺序点击木条：001 → 002 → 003 → 004 → 005 → 006" });

        var router = FindObjectOfType<ClickRouter2D>();
        if (router != null) router.cam = boxCameraUI;
    }

    public void ExitBoxView()
    {
        if (!isInBoxView) return;

        boxViewPanel.SetActive(false);
        boxCameraUI.gameObject.SetActive(false);
        DialogueManager.Instance.panel.SetActive(false);
        //if (manuscriptPanel != null) manuscriptPanel.SetActive(false);

        var router = FindObjectOfType<ClickRouter2D>();
        if (router != null) router.cam = Camera.main;

        isInBoxView = false;
    }

    // 点击 RawImage（盒子视图区域）的回调
    public void OnBoxClick()
    {
        if (!isInBoxView) return;

        // 解谜完成后，显示手札面板（不再处理零件点击）
        if (isPuzzleCompleted)
        {
            if (DialogueTreeManager.Instance != null)
            {
                DialogueTreeManager.Instance.StartDialogue(15);
            }
            else
            {
                Debug.LogError("DialogueTreeManager 不存在，无法启动手札对话");
            }
            return;
        }

        // 未完成时处理零件点击
        if (currentStep >= stepOrder.Length) return;

        // 将屏幕点击转换为 RawImage 的 UV 坐标，并发射射线检测零件
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boxDisplay.rectTransform,
            Input.mousePosition,
            null,
            out Vector2 localPoint
        );
        Rect rect = boxDisplay.rectTransform.rect;
        Vector2 uv = new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height
        );
        Ray ray = boxCameraUI.ViewportPointToRay(uv);

        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        if (hits.Length == 0) return;

        string required = stepOrder[currentStep];
        PartClickHandler targetPart = null;
        foreach (var hit in hits)
        {
            PartClickHandler part = hit.collider.GetComponent<PartClickHandler>();
            if (part != null && part.partType == required)
            {
                targetPart = part;
                break;
            }
        }
        if (targetPart != null)
            OnPartClicked(targetPart.partType);
    }

    public void OnPartClicked(string partType)
    {
        if (!isInBoxView || currentStep >= stepOrder.Length) return;
        if (partType == stepOrder[currentStep])
            StartCoroutine(PullPartAnimation(partType));
        else
            Debug.Log($"顺序错误：需要 {stepOrder[currentStep]}，点击了 {partType}");
    }

    private IEnumerator PullPartAnimation(string partType)
    {
        Transform part = transform.Find(partType);
        if (part == null) yield break;

        PartClickHandler handler = part.GetComponent<PartClickHandler>();
        Vector3 direction = (handler != null && handler.pullDirection != Vector3.zero)
                            ? handler.pullDirection
                            : pullDirection;
        float distance = (handler != null && handler.pullDistance > 0)
                         ? handler.pullDistance
                         : pullDistance;

        Vector3 startPos = part.localPosition;
        Vector3 endPos = startPos + direction * distance;
        float elapsed = 0f;
        while (elapsed < pullDuration)
        {
            float t = elapsed / pullDuration;
            part.localPosition = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        part.localPosition = endPos;
        part.gameObject.SetActive(false);
        OnPartPulled(partType);
    }

    private void OnPartPulled(string partType)
    {
        if (partType != stepOrder[currentStep]) return;
        currentStep++;
        if (currentStep >= stepOrder.Length)
        {
            isPuzzleCompleted = true;
            HasUnlockedManuscript = true;
            Debug.Log("解谜完成，点击盒子以查看手札");
        }
    }

    // 显示全屏手札面板（改为 public，供 DialogueTreeManager 调用）
    public void ShowManuscriptPanel()
    {
        if (manuscriptPanel == null)
        {
            Debug.LogError("手札面板未指定");
            return;
        }
        manuscriptPanel.SetActive(true);
        Debug.Log("手札面板已显示，点击任意位置触发对话");
    }

    // 点击手札面板时触发的对话
    private void OnManuscriptPanelClicked()
    {
        Debug.Log("点击手札面板，启动对话");
        // 结束可能残留的对话
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.NextOrClose();
       // if (DialogueTreeManager.Instance != null)
           // DialogueTreeManager.Instance.EndDialogue();

        // 启动手札对话（使用配置的对话内容）
        if (DialogueManager.Instance != null && manuscriptDialogueLines != null && manuscriptDialogueLines.Length > 0)
        {
            DialogueManager.Instance.StartDialogue("手札", manuscriptDialogueLines);
        }
        else if (DialogueTreeManager.Instance != null)
        {
            // 如果你使用 DialogueTreeManager 且需要 ID，可以改为 ID 方式
            // DialogueTreeManager.Instance.StartDialogue(manuscriptDialogueId);
            Debug.LogWarning("请配置 manuscriptDialogueLines 或使用 ID 启动对话");
        }

        // 可选：点击后关闭面板（若需要，取消下面注释）
        // if (manuscriptPanel != null) manuscriptPanel.SetActive(false);
    }

    // 调整相机视角以看到所有零件（原有逻辑，保持不变）
    private void AdjustCameraToParts()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        bool hasPart = false;
        foreach (string partName in stepOrder)
        {
            Transform part = transform.Find(partName);
            if (part != null && part.gameObject.activeSelf)
            {
                hasPart = true;
                Renderer rend = part.GetComponent<Renderer>();
                if (rend != null)
                    bounds.Encapsulate(rend.bounds);
                else
                    bounds.Encapsulate(part.position);
            }
        }
        if (hasPart)
        {
            Vector3 center = bounds.center;
            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            float distance = maxExtent * 2f;
            boxCameraUI.transform.position = center + new Vector3(0, 0, -distance);
            boxCameraUI.transform.LookAt(center);
            boxCameraUI.orthographic = true;
            boxCameraUI.orthographicSize = maxExtent * 1.2f;
        }
        else
        {
            Vector3 boxCenter = transform.position;
            boxCameraUI.transform.position = boxCenter + new Vector3(0, 0, -5);
            boxCameraUI.transform.LookAt(boxCenter);
            boxCameraUI.orthographicSize = 5;
        }
    }

    // 关闭全屏手札面板（供 DialogueTreeManager 调用）
    public void CloseManuscriptPanel()
    {
        if (manuscriptPanel != null && manuscriptPanel.activeSelf)
        {
            manuscriptPanel.SetActive(false);
            Debug.Log("手札面板已关闭");
        }
    }
    public bool CanPull(string partType)
    {
        return partType == stepOrder[currentStep];
    }
}