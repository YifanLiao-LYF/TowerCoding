using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueTreeManager : MonoBehaviour
{
    public static DialogueTreeManager Instance;

    [Header("CSV文件")]
    public TextAsset csvFile;

    [Header("长按 UI")]
    public LongPressQHandler longPressHandler;

    private Dictionary<int, DialogueNode> nodes = new Dictionary<int, DialogueNode>();
    private int currentNodeId;
    private bool isInDialogue = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        ParseCSV();
    }

    void ParseCSV()
    {
        if (csvFile == null) return;
        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] fields = line.Split(',');
            if (fields.Length < 8) continue;

            string flag = fields[0];
            int id = SafeParseInt(fields[1]);
            string speaker = fields[2];
            string position = fields[3];
            string content = fields[4];
            int nextId = SafeParseInt(fields[5]);
            string effect = fields[6];
            string target = fields[7];

            DialogueNode node = new DialogueNode
            {
                id = id,
                type = flag,
                speaker = speaker,
                position = position,
                content = content,
                nextId = nextId,
                effect = effect,
                target = target
            };
            nodes[id] = node;
        }
    }

    int SafeParseInt(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return -1;
        if (int.TryParse(s, out int result)) return result;
        return -1;
    }

    public void StartDialogue(int startId)
    {
        if (isInDialogue) return;
        isInDialogue = true;
        currentNodeId = startId;
        ProcessCurrentNode();
    }

    void ProcessCurrentNode()
    {
        if (!nodes.ContainsKey(currentNodeId))
        {
            Debug.LogError($"节点 {currentNodeId} 不存在，结束对话");
            EndDialogue();
            DialogueManager.Instance.EndDialogue();
            return;
        }

        DialogueNode node = nodes[currentNodeId];

        switch (node.type)
        {
            case "#":
                // 先处理 effect（除 LongPressQ 外立即执行）
                if (!string.IsNullOrEmpty(node.effect) && node.effect != "LongPressQ")
                {
                    ApplyEffect(node.effect, node.target);  // 传递 target
                }

                if (node.effect == "LongPressQ")
                {
                    DialogueManager.Instance.StartDialogue(node.speaker, new string[] { node.content });
                    StartLongPressQ(node.nextId);
                }
                else
                {
                    DialogueManager.Instance.StartDialogue(node.speaker, new string[] { node.content });
                    DialogueManager.Instance.onNextClicked += OnDialogueNext;
                }
                break;
            case "&":
                var choiceNodes = CollectChoiceNodes(currentNodeId);
                if (choiceNodes.Count == 0)
                {
                    Debug.LogWarning("选择节点没有选项，直接跳转下一节点");
                    currentNodeId = node.nextId;
                    ProcessCurrentNode();
                    return;
                }

                List<string> options = new List<string>();
                List<int> targets = new List<int>();
                List<string> effects = new List<string>();
                List<string> effectTargets = new List<string>();
                foreach (var cn in choiceNodes)
                {
                    options.Add(cn.content);
                    targets.Add(cn.nextId);
                    effects.Add(cn.effect);
                    effectTargets.Add(cn.target);
                }

                if (ChoiceDialogueManager.Instance == null)
                {
                    Debug.LogError("ChoiceDialogueManager.Instance 为空，无法显示选项");
                    return;
                }

                ChoiceDialogueManager.Instance.ShowChoice("", options.ToArray(), (index) =>
                {
                    if (!string.IsNullOrEmpty(effects[index]))
                        ApplyEffect(effects[index], effectTargets[index]);  // 传递 target
                    currentNodeId = targets[index];
                    ProcessCurrentNode();
                });
                break;
            case "END":
                DialogueManager.Instance.EndDialogue();
                EndDialogue();
                break;
            default:
                EndDialogue();
                break;
        }
    }

    List<DialogueNode> CollectChoiceNodes(int startId)
    {
        List<DialogueNode> list = new List<DialogueNode>();
        int id = startId;
        while (nodes.ContainsKey(id) && nodes[id].type == "&")
        {
            list.Add(nodes[id]);
            id++;
        }
        return list;
    }

    void OnDialogueNext()
    {
        DialogueManager.Instance.onNextClicked -= OnDialogueNext;

        if (!nodes.ContainsKey(currentNodeId))
        {
            Debug.LogError($"节点 {currentNodeId} 不存在，结束对话");
            DialogueManager.Instance.EndDialogue();
            return;
        }

        int nextId = nodes[currentNodeId].nextId;
        currentNodeId = nextId;
        ProcessCurrentNode();
    }

    void StartLongPressQ(int nextId)
    {
        if (DialogueManager.Instance.nextButton != null)
            DialogueManager.Instance.nextButton.SetActive(false);

        if (longPressHandler == null)
        {
            Debug.LogError("未在 Inspector 中指定 LongPressQHandler");
            currentNodeId = nextId;
            ProcessCurrentNode();
            return;
        }

        longPressHandler.StartLongPress(() =>
        {
            if (DialogueManager.Instance.nextButton != null)
                DialogueManager.Instance.nextButton.SetActive(true);
            currentNodeId = nextId;
            ProcessCurrentNode();
        });
    }

    /// <summary>
    /// 执行效果（支持 target 参数）
    /// </summary>
    void ApplyEffect(string effect, string target = "")
    {
        switch (effect)
        {
            case "EnterBoxView":
                WoodenBoxController box = FindObjectOfType<WoodenBoxController>();
                if (box != null) box.EnterBoxView();
                break;
           
            case "ExitBoxView":
                WoodenBoxController boxCtrl2 = FindObjectOfType<WoodenBoxController>();
                if (boxCtrl2 != null) boxCtrl2.ExitBoxView();
                break;
            case "ExitAndShowManuscript":   // 新增组合效果
                WoodenBoxController box3 = FindObjectOfType<WoodenBoxController>();
                if (box3 != null)
                {
                    box3.ExitBoxView();        // 先退出盒子视图
                    box3.ShowManuscriptPanel(); // 再显示手札面板
                }
                break;
            case "ShowManuscriptPanel":
                WoodenBoxController box2 = FindObjectOfType<WoodenBoxController>();
                if (box2 != null) box2.ShowManuscriptPanel();
                break;
            case "FadeToNewBackground":
                if (BackgroundManager.Instance != null)
                    BackgroundManager.Instance.FadeToNewBackground();
                break;
            case "LoadScene":
                if (BackgroundManager.Instance != null && !string.IsNullOrEmpty(target))
                    BackgroundManager.Instance.FadeAndLoadScene(target);
                else
                    Debug.LogError("LoadScene 效果缺少场景名称参数");
                break;
            case "LoadScene2":
                // 临时用索引2来加载ZoomingInTower1场景
                BackgroundManager.Instance.FadeAndLoadScene2(target);
                break;
            case "CloseManuscriptPanel":
                box = FindObjectOfType<WoodenBoxController>();
                if (box != null) box.CloseManuscriptPanel();
                break;
        }
    }

    public void EndDialogue()
    {
        isInDialogue = false;
        if (ChoiceDialogueManager.Instance != null)
            ChoiceDialogueManager.Instance.choicePanel.SetActive(false);
    }
}