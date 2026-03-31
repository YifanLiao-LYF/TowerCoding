using UnityEngine;

public class ClickableDialogue : MonoBehaviour
{
    [Header("Priority: 数值越大越优先")]
    public int priority = 0;

    [Header("对话树模式")]
    public int startDialogueId = -1;   // 设置为 >=0 时使用对话树

    // 供点击路由器调用
    public void TriggerDialogue()
    {
        if (DialogueManager.Instance == null) return;

        // 如果配置了对话树起始ID，则使用对话树
        if (DialogueTreeManager.Instance != null && startDialogueId >= 0)
        {
            DialogueTreeManager.Instance.StartDialogue(startDialogueId);
            return;
        }

        // 如果没有配置对话树，可以添加旧逻辑，但本例中假设所有物品都已迁移
        Debug.LogWarning("物体 " + gameObject.name + " 未设置对话树起始ID，无法触发对话。");
    }
}