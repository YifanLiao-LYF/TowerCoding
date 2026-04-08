using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text contentText;
    public GameObject nextButton;

    public System.Action onNextClicked;

    private string currentSpeaker;
    private string[] currentLines;
    private int currentIndex;
    private bool isTalking;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            panel.SetActive(false);
        }
    }

    public bool IsTalking() => isTalking;

    private bool IsDialogueContentValid(string[] lines)
    {
        if (lines == null || lines.Length == 0) return false;
        foreach (string line in lines)
            if (!string.IsNullOrWhiteSpace(line)) return true;
        return false;
    }

    public void StartDialogue(string speaker, string[] lines)
    {
        if (!IsDialogueContentValid(lines))
        {
            // 仅透明，不改其他状态
            if (canvasGroup != null) canvasGroup.alpha = 0;
            return;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1;
        currentSpeaker = speaker;
        currentLines = lines;
        currentIndex = 0;
        isTalking = true;

        panel.SetActive(true);
        nameText.text = currentSpeaker;
        contentText.text = currentLines[currentIndex];
    }
    /// <summary>
    /// 每次点击时调用：显示下一句，若已到最后则关闭对话框
    /// </summary>
    public void NextOrClose()
    {
       // if (ChoiceDialogueManager.IsChoosing) return;
        if (!isTalking) return;

        // 如果有外部订阅者（如对话树），则将推进工作交给它们
        if (onNextClicked != null)
        {
            //Debug.Log("NextOrClose: 触发事件，跳过内部翻页");
            onNextClicked?.Invoke();
            return;
        }

        // 否则执行原有的简单对话翻页逻辑
        //Debug.Log("NextOrClose: 执行内部翻页");
        currentIndex++;
        if (currentIndex < currentLines.Length)
        {
            contentText.text = currentLines[currentIndex];
        }
        else
        {
            EndDialogue();
        }
    }

    // 改为 public，以便外部调用（例如在弹出选项前结束对话）
    public void EndDialogue()
    {
        //Debug.Log("EndDialogue 被调用");
        isTalking = false;
        panel.SetActive(false);
        currentLines = null;
        currentIndex = 0;

        // 确保选项面板关闭，避免 UI 遮挡
        if (ChoiceDialogueManager.Instance != null)
            ChoiceDialogueManager.Instance.choicePanel.SetActive(false);
    }
}