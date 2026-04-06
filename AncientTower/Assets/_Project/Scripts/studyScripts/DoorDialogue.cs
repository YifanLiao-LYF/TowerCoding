using UnityEngine;

public class ConditionalDialogue : ClickableDialogue
{
    public enum ConditionType
    {
        HasUnlockedManuscript,  // 是否已获得手札（书房门用）
        HasItem,                // 是否拥有某物品（可扩展）
        BoolGlobalFlag,         // 全局布尔标志（楼梯用）
    }

    [Header("条件设置")]
    public ConditionType condition;
    public string conditionParam = "";

    [Header("分支对话ID")]
    public int trueDialogueId = 29;
    public int falseDialogueId = 30;

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

        Debug.Log($"条件检测: {condition}, 结果={conditionMet}, 选中ID={dialogueId}");
        DialogueTreeManager.Instance.StartDialogue(dialogueId);
    }

    private bool EvaluateCondition()
    {
        switch (condition)
        {
            case ConditionType.HasUnlockedManuscript:
                return boxController != null && boxController.HasUnlockedManuscript;

            case ConditionType.HasItem:
                Debug.LogWarning("HasItem 条件需要你自己实现物品系统");
                return false;

            case ConditionType.BoolGlobalFlag:
                if (conditionParam == "MemoryTriggered")
                    return DialogueTreeManager.HasTriggeredMemory;
                Debug.LogWarning($"未处理的全局标志: {conditionParam}");
                return false;

            default:
                return false;
        }
    }
}