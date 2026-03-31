using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NextButtonHandler : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.NextOrClose();
            }
        });
    }
}