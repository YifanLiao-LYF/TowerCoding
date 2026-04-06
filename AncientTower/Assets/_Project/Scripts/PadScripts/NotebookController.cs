using UnityEngine;

public class NotebookController : MonoBehaviour
{
    public GameObject[] notebookPages; // °´Ë³Ðò£ºImage, Image (1), Image (2), Image (3)

    void OnEnable()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        int unlocked = DialogueTreeManager.UnlockedNotebookPageCount;
        for (int i = 0; i < notebookPages.Length; i++)
        {
            if (notebookPages[i] != null)
                notebookPages[i].SetActive(i < unlocked);
        }
    }
}