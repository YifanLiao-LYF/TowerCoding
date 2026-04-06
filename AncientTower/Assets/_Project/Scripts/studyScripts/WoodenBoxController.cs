using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WoodenBoxController : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject boxViewPanel;
    public Camera boxCameraUI;
    public RawImage boxDisplay;

    [Header("手札全屏遮罩面板")]
    public GameObject manuscriptPanel;

    [Header("手札页面（多个Image物体）")]
    public GameObject[] manuscriptPages;

    [Header("动画设置")]
    public float pullDuration = 0.3f;
    public Vector3 pullDirection = new Vector3(0, 1, 0);
    public float pullDistance = 1.5f;

    [Header("手札对话内容（已废弃）")]
    public string[] manuscriptDialogueLines = new string[] { "这是手札的内容。" };

    private bool isInBoxView = false;
    private int currentStep = 0;               // 仍然保留，用于顺序提示，但不用于完成判断
    private bool isPuzzleCompleted = false;
    private readonly string[] stepOrder = { "001", "002", "003", "004", "005", "006" };
    public bool HasUnlockedManuscript { get; private set; } = false;

    private HashSet<string> pulledParts = new HashSet<string>();   // 防止同一零件重复拉取
    private bool isPulling = false;
    private bool hasCompleted = false;
    private List<string> completedParts = new List<string>();       // 记录已正确完成的零件（按顺序）

    void Start()
    {
        if (boxViewPanel != null) boxViewPanel.SetActive(false);
        if (boxCameraUI != null) boxCameraUI.gameObject.SetActive(false);
        if (manuscriptPanel != null) manuscriptPanel.SetActive(false);

        if (boxDisplay != null)
        {
            var btn = boxDisplay.gameObject.GetComponent<Button>();
            if (btn == null) btn = boxDisplay.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnBoxClick);
        }

        if (manuscriptPanel != null)
        {
            Button panelBtn = manuscriptPanel.GetComponent<Button>();
            if (panelBtn == null) panelBtn = manuscriptPanel.AddComponent<Button>();
            panelBtn.onClick.RemoveAllListeners();
            panelBtn.onClick.AddListener(OnManuscriptPanelClicked);
        }

        ResetManuscriptPages();
    }

    public void ResetManuscriptPages()
    {
        if (manuscriptPages == null || manuscriptPages.Length == 0) return;
        for (int i = 0; i < manuscriptPages.Length; i++)
            if (manuscriptPages[i] != null)
                manuscriptPages[i].SetActive(i == 0);
    }

    public void SetManuscriptPage(int page)
    {
        if (manuscriptPages == null || manuscriptPages.Length == 0) return;
        if (page < 1 || page >= manuscriptPages.Length)
        {
            Debug.LogWarning($"页码 {page} 超出范围（1~{manuscriptPages.Length - 1}）");
            return;
        }
        for (int i = 0; i < manuscriptPages.Length; i++)
            if (manuscriptPages[i] != null)
                manuscriptPages[i].SetActive(i == page);
    }

    private void OnManuscriptPanelClicked()
    {
        Debug.Log("点击手札面板，推进对话");
        DialogueManager.Instance?.NextOrClose();
    }

    public void EnterBoxView()
    {
        if (isInBoxView) return;

        DialogueTreeManager.Instance?.EndDialogue();
        DialogueManager.Instance?.EndDialogue();

        AdjustCameraToParts();
        boxDisplay.texture = boxCameraUI.targetTexture;

        isInBoxView = true;
        currentStep = 0;
        isPuzzleCompleted = false;
        hasCompleted = false;
        pulledParts.Clear();
        completedParts.Clear();          // 清空完成记录
        isPulling = false;

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

        var router = FindObjectOfType<ClickRouter2D>();
        if (router != null) router.cam = Camera.main;

        isInBoxView = false;
    }

    public void OnBoxClick()
    {
        if (!isInBoxView) return;

        if (isPuzzleCompleted)
        {
            DialogueTreeManager.Instance?.StartDialogue(15);
            return;
        }

        if (currentStep >= stepOrder.Length) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boxDisplay.rectTransform,
            Input.mousePosition,
            null,
            out Vector2 localPoint))
        {
            return;
        }

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
        if (pulledParts.Contains(partType))
        {
            Debug.Log($"零件 {partType} 已被拉取过，忽略");
            return;
        }
        if (isPulling)
        {
            Debug.Log($"正在播放动画，忽略点击 {partType}");
            return;
        }

        if (partType == stepOrder[currentStep])
        {
            Debug.Log($"正确点击 {partType}，当前步骤 {currentStep}，启动动画");
            pulledParts.Add(partType);
            StartCoroutine(PullPartAnimation(partType));
        }
        else
        {
            Debug.Log($"顺序错误：需要 {stepOrder[currentStep]}，点击了 {partType}");
        }
    }

    private IEnumerator PullPartAnimation(string partType)
    {
        isPulling = true;
        Debug.Log($"开始拉取 {partType}");

        Transform part = transform.Find(partType);
        if (part == null)
        {
            Debug.LogError($"未找到零件 {partType}");
            isPulling = false;
            yield break;
        }

        PartClickHandler handler = part.GetComponent<PartClickHandler>();
        Vector3 direction = (handler != null && handler.pullDirection != Vector3.zero) ? handler.pullDirection : pullDirection;
        float distance = (handler != null && handler.pullDistance > 0) ? handler.pullDistance : pullDistance;

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
        Debug.Log($"零件 {partType} 动画完成，调用 OnPartPulled");
        OnPartPulled(partType);
        isPulling = false;
    }

    private void OnPartPulled(string partType)
    {
        Debug.Log($"OnPartPulled 收到零件: {partType}, 当前 completedParts 数量: {completedParts.Count}, 期望零件: {stepOrder[completedParts.Count]}");

        // 检查顺序：必须等于下一个应该完成的零件
        if (partType != stepOrder[completedParts.Count])
        {
            Debug.LogWarning($"顺序错误: 收到 {partType}, 期望 {stepOrder[completedParts.Count]}, 忽略");
            return;
        }

        completedParts.Add(partType);
        Debug.Log($"完成零件 {partType}，已完成数量 {completedParts.Count}");

        // 同步更新 currentStep（用于后续顺序提示，但不影响完成判断）
        currentStep = completedParts.Count;

        // 打印当前 completedParts 列表内容
        string listStr = "";
        foreach (var p in completedParts) listStr += p + " ";
        Debug.Log($"当前已完成零件列表: {listStr}");

        if (completedParts.Count == stepOrder.Length && !hasCompleted)
        {
            hasCompleted = true;
            isPuzzleCompleted = true;
            HasUnlockedManuscript = true;
            Debug.Log("解谜完成，点击盒子以查看手札");
        }
        else
        {
            Debug.Log($"尚未完成，还需要 {stepOrder.Length - completedParts.Count} 个零件");
        }
    }

    public void ShowManuscriptPanel()
    {
        if (manuscriptPanel == null) return;
        ResetManuscriptPages();
        manuscriptPanel.SetActive(true);
        Debug.Log("手札面板已显示");
    }

    public void CloseManuscriptPanel()
    {
        if (manuscriptPanel != null && manuscriptPanel.activeSelf)
            manuscriptPanel.SetActive(false);
    }

    public bool CanPull(string partType) => partType == stepOrder[currentStep];

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
}