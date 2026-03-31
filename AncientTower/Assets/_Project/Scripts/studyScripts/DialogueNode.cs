// 对话节点数据类
public class DialogueNode
{
    public int id;          // 节点ID
    public string type;     // "#" 对话, "&" 选择, "END" 结束
    public string speaker;  // 说话者
    public string position; // 位置（左/右，可用于UI显示）
    public string content;  // 对话内容或选项文本
    public int nextId;      // 跳转目标ID（对话节点使用）
    public string effect;   // 效果（如获得物品）
    public string target;   // 额外目标（如跳转场景）
}