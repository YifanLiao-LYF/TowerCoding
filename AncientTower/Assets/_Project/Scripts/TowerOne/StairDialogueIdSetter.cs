using UnityEngine;

public class StairDialogueIdSetter : MonoBehaviour
{
    public int beforeMemoryId = 18;   // 记忆触发前的对话ID
    public int afterMemoryId = 36;    // 记忆触发后的对话ID

    private ClickableDialogue clickable;

    void Start()
    {
        clickable = GetComponent<ClickableDialogue>();
        if (clickable == null) return;

        // 根据全局标志设置对话ID
        if (DialogueTreeManager.HasTriggeredMemory)
            clickable.startDialogueId = afterMemoryId;
        else
            clickable.startDialogueId = beforeMemoryId;
    }
}