using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceDialogueManager : MonoBehaviour
{
    public static ChoiceDialogueManager Instance;

    public GameObject choicePanel;
    public Button optionAButton;
    public Button optionBButton;
    public GameObject choiceBlocker; // 全屏透明遮挡物（需在 Inspector 中拖入）

    private System.Action<int> onChoiceSelected;

    void Awake()
    {
        Instance = this;
        choicePanel.SetActive(false);
        if (choiceBlocker != null) choiceBlocker.SetActive(false);
        optionAButton.onClick.AddListener(() => OnOptionSelected(0));
        optionBButton.onClick.AddListener(() => OnOptionSelected(1));
    }

    public void ShowChoice(string title, string[] options, System.Action<int> callback)
    {
        // 显示遮挡物，拦截所有外部点击
        if (choiceBlocker != null) choiceBlocker.SetActive(true);

        optionAButton.GetComponentInChildren<TMP_Text>().text = options[0];
        optionBButton.GetComponentInChildren<TMP_Text>().text = options[1];
        onChoiceSelected = callback;
        choicePanel.SetActive(true);
    }

    void OnOptionSelected(int index)
    {
        // 关闭遮挡物和选择面板
        if (choiceBlocker != null) choiceBlocker.SetActive(false);
        choicePanel.SetActive(false);
        onChoiceSelected?.Invoke(index);
        onChoiceSelected = null;
    }
}