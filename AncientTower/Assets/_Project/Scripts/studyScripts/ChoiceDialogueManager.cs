using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceDialogueManager : MonoBehaviour
{
    public static ChoiceDialogueManager Instance;

    public GameObject choicePanel;
    public Button optionAButton;
    public Button optionBButton;

    private System.Action<int> onChoiceSelected;

    void Awake()
    {
        Instance = this;
        choicePanel.SetActive(false);
        optionAButton.onClick.AddListener(() => OnOptionSelected(0));
        optionBButton.onClick.AddListener(() => OnOptionSelected(1));
    }

    public void ShowChoice(string title, string[] options, System.Action<int> callback)
    {
        // 可以设置标题（title）显示，这里简化
        optionAButton.GetComponentInChildren<TMP_Text>().text = options[0];
        optionBButton.GetComponentInChildren<TMP_Text>().text = options[1];
        onChoiceSelected = callback;
        choicePanel.SetActive(true);
    }

    void OnOptionSelected(int index)
    {
        choicePanel.SetActive(false);
        onChoiceSelected?.Invoke(index);
        onChoiceSelected = null;
    }
}