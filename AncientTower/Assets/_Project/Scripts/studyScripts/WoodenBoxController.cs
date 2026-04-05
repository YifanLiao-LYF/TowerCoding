using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WoodenBoxController : MonoBehaviour
{
    [Header("UI���")]
    public GameObject boxViewPanel;          // ������ͼ��������壨���� RawImage �ȣ�
    public Camera boxCameraUI;
    public RawImage boxDisplay;

    [Header("����ȫ���������")]
    public GameObject manuscriptPanel;       // ȫ����͸����ɫ��壨�� Button���ں�����ͼƬ�����֣�

    [Header("�������ã�ȫ��Ĭ��ֵ��")]
    public float pullDuration = 0.3f;
    public Vector3 pullDirection = new Vector3(0, 1, 0);
    public float pullDistance = 1.5f;

    [Header("�����Ի�����")]
    public string[] manuscriptDialogueLines = new string[] { "�������������ݡ�" };

    private bool isInBoxView = false;
    private int currentStep = 0;
    private bool isPuzzleCompleted = false;
    private readonly string[] stepOrder = { "001", "002", "003", "004", "005", "006" };
    public bool HasUnlockedManuscript { get; private set; } = false;

    void Start()
    {
        // ��ʼ�����������
        if (boxViewPanel != null) boxViewPanel.SetActive(false);
        if (boxCameraUI != null) boxCameraUI.gameObject.SetActive(false);
        if (manuscriptPanel != null) manuscriptPanel.SetActive(false);

        // Ϊ RawImage ���ӵ���������������ģʽ�����ڵ���������ʾ������
        if (boxDisplay != null)
        {
            var btn = boxDisplay.gameObject.GetComponent<Button>();
            if (btn == null) btn = boxDisplay.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnBoxClick);
        }

        // Ϊ�������� Button ���Ӽ������������λ�ô����Ի���
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
                Debug.LogWarning("ManuscriptPanel ȱ�� Button ������޷���Ӧ���");
            }
        }
    }

    public void EnterBoxView()
    {
        if (isInBoxView) return;

        // �������ܴ��ڵĶԻ�
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
        DialogueManager.Instance.StartDialogue("ϵͳ", new string[] { "�밴��˳����ľ����001 �� 002 �� 003 �� 004 �� 005 �� 006" });

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

    // RawImage（即箱子图片）点击回调
    public void OnBoxClick()
    {
        if (!isInBoxView) return;

        // ������ɺ���ʾ������壨���ٴ�����������
        if (isPuzzleCompleted)
        {
            if (DialogueTreeManager.Instance != null)
            {
                DialogueTreeManager.Instance.StartDialogue(15);
            }
            else
            {
                Debug.LogError("DialogueTreeManager �����ڣ��޷����������Ի�");
            }
            return;
        }

        // δ���ʱ����������
        if (currentStep >= stepOrder.Length) return;

        // ����Ļ���ת��Ϊ RawImage �� UV ���꣬���������߼�����
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
            Debug.Log($"˳�������Ҫ {stepOrder[currentStep]}������� {partType}");
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

    /// <summary>
    /// 某个部件被拉出后的处理逻辑
    /// </summary>
    public void OnPartPulled(string partType)
    {
        if (partType != stepOrder[currentStep]) return;
        currentStep++;
        if (currentStep >= stepOrder.Length)
        {
            isPuzzleCompleted = true;
            HasUnlockedManuscript = true;
            Debug.Log("拼图完成，可以查看手稿");
        }
    }

    // 显示手稿面板（供 DialogueTreeManager 调用）
    public void ShowManuscriptPanel()
    {
        if (manuscriptPanel == null)
        {
            Debug.LogError("�������δָ��");
            return;
        }
        manuscriptPanel.SetActive(true);
        Debug.Log("�����������ʾ���������λ�ô����Ի�");
    }

    // 手稿面板点击时的对话
    private void OnManuscriptPanelClicked()
    {
        Debug.Log("���������壬�����Ի�");
        // �������ܲ����ĶԻ�
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.NextOrClose();
       // if (DialogueTreeManager.Instance != null)
           // DialogueTreeManager.Instance.EndDialogue();

        // ���������Ի���ʹ�����õĶԻ����ݣ�
        if (DialogueManager.Instance != null && manuscriptDialogueLines != null && manuscriptDialogueLines.Length > 0)
        {
            DialogueManager.Instance.StartDialogue("����", manuscriptDialogueLines);
        }
        else if (DialogueTreeManager.Instance != null)
        {
            // �����ʹ�� DialogueTreeManager ����Ҫ ID�����Ը�Ϊ ID ��ʽ
            // DialogueTreeManager.Instance.StartDialogue(manuscriptDialogueId);
            Debug.LogWarning("未设置 manuscriptDialogueLines，建议使用 ID 方式对话");
        }

        // ��ѡ�������ر���壨����Ҫ��ȡ������ע�ͣ�
        // if (manuscriptPanel != null) manuscriptPanel.SetActive(false);
    }

    // 调整摄像机以适应所有部件（原逻辑基本不变）
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

    // 关闭手稿面板（供 DialogueTreeManager 调用）
    public void CloseManuscriptPanel()
    {
        if (manuscriptPanel != null && manuscriptPanel.activeSelf)
        {
            manuscriptPanel.SetActive(false);
            Debug.Log("��������ѹر�");
        }
    }
    public bool CanPull(string partType)
    {
        return partType == stepOrder[currentStep];
    }
}