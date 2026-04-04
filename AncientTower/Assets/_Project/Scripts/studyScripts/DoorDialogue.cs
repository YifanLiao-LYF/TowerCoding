using UnityEngine;

public class ConditionalDialogue : ClickableDialogue
{
    public enum ConditionType
    {
        HasUnlockedManuscript,  // 是否已获得手札（解谜完成）
        HasItem,                // 是否拥有某物品（可扩展）
        BoolGlobalFlag,         // 自定义全局布尔标志（需要你自己实现全局标志管理）
    }

    [Header("条件设置")]
    public ConditionType condition;
    public string conditionParam = "";   // 用于 HasItem 时填写物品名，或 BoolGlobalFlag 的标志名

    [Header("分支对话ID")]
    public int trueDialogueId = 29;      // 条件满足时触发的对话ID
    public int falseDialogueId = 30;     // 条件不满足时触发的对话ID

    private WoodenBoxController boxController;

    void Awake()
    {
        boxController = FindObjectOfType<WoodenBoxController>();
    }

    public override void TriggerDialogue()
    {
        if (DialogueTreeManager.Instance == null) return;

        bool conditionMet = EvaluateCondition();
        int dialogueId = conditionMet ? trueDialogueId : falseDialogueId;
        DialogueTreeManager.Instance.StartDialogue(dialogueId);
    }

    private bool EvaluateCondition()
    {
        switch (condition)
        {
            case ConditionType.HasUnlockedManuscript:
                return boxController != null && boxController.HasUnlockedManuscript;

            case ConditionType.HasItem:
                // 示例：假设你有一个 Inventory 系统
                // return InventoryManager.Instance.HasItem(conditionParam);
                Debug.LogWarning("HasItem 条件需要你自己实现物品系统");
                return false;

            case ConditionType.BoolGlobalFlag:
                // 示例：假设你有一个 GlobalFlags 管理器
                // return GlobalFlags.GetFlag(conditionParam);
                Debug.LogWarning("BoolGlobalFlag 条件需要你自己实现全局标志系统");
                return false;

            default:
                return false;
        }
    }
}