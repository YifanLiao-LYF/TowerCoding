using System.Collections.Generic;
using UnityEngine;

public class ChatListController : MonoBehaviour
{
    [SerializeField] private Transform messageListContent;
    [SerializeField] private ChatEntryUI chatEntryPrefab;
    [SerializeField] private GameObject chatDetailPanel;
    
    private Dictionary<ChatContactType, ChatEntryUI> chatEntries = new Dictionary<ChatContactType, ChatEntryUI>();
    private Dictionary<ChatContactType, ChatContactData> contactData = new Dictionary<ChatContactType, ChatContactData>();
    
    void Start()
    {
        InitializeContacts();
        CreateChatEntries();
    }
    
    private void InitializeContacts()
    {
        // 创建4个联系人数据
        AddContact(ChatContactType.Father, "父亲");
        AddContact(ChatContactType.ManagerChen, "陈经理");
        AddContact(ChatContactType.ProjectGroup, "古塔开发项目组");
        AddContact(ChatContactType.ProfessorZhou, "周教授");
        
        // 加载初始聊天记录
        LoadInitialMessages();
    }
    
    private void AddContact(ChatContactType type, string name)
    {
        var contact = ScriptableObject.CreateInstance<ChatContactData>();
        contact.contactType = type;
        contact.displayName = name;
        contact.messages = new List<MessageEntry>();
        contact.hasNewMessage = false;
        contactData[type] = contact;
    }
    
    private void LoadInitialMessages()
    {
        // 这里加载你提供的所有聊天记录
        LoadFatherMessages();
        LoadManagerMessages();
        LoadGroupMessages();
        LoadProfessorMessages();
        
        // 更新每个联系人的最新消息预览
        foreach (var contact in contactData.Values)
        {
            if (contact.messages.Count > 0)
            {
                var lastMsg = contact.messages[contact.messages.Count - 1];
                contact.latestMessagePreview = lastMsg.content;
            }
        }
    }
    
    private void CreateChatEntries()
    {
        foreach (var kvp in contactData)
        {
            var entry = Instantiate(chatEntryPrefab, messageListContent);
            entry.Initialize(kvp.Value, OnChatEntryClicked);
            chatEntries[kvp.Key] = entry;
        }
    }
    
    private void OnChatEntryClicked(ChatContactData contact)
    {
        // 打开聊天详情
        chatDetailPanel.SetActive(true);
        var detailController = chatDetailPanel.GetComponent<ChatDetailController>();
        detailController.LoadMessages(contact);
        
        // 清除未读标记
        if (contact.hasNewMessage)
        {
            contact.hasNewMessage = false;
            chatEntries[contact.contactType].UpdateUI();
        }
    }
    
    // 以下是你的4个聊天记录初始化方法
    private void LoadFatherMessages()
    {
        var contact = contactData[ChatContactType.Father];
        float dayStart = GetTodayTime(15, 23);
        
        contact.messages.Add(new MessageEntry
        {
            messageId = "father_001",
            content = "听说你接了那个开发项目？",
            isFromPlayer = false,
            senderName = "父亲",
            timeStamp = GetTodayTime(15, 23)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "father_002",
            content = "只是去做评估，还没定。",
            isFromPlayer = true,
            senderName = "沈凌",
            timeStamp = GetTodayTime(15, 25)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "father_003",
            content = "咱们家的规矩你忘了？",
            isFromPlayer = false,
            senderName = "父亲",
            timeStamp = GetTodayTime(15, 27)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "father_004",
            content = "可不动它也会塌。现在有新技术，不一样了。",
            isFromPlayer = true,
            senderName = "沈凌",
            timeStamp = GetTodayTime(15, 30)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "father_005",
            content = "回来一趟，当面说。",
            isFromPlayer = false,
            senderName = "父亲",
            timeStamp = GetTodayTime(15, 41)
        });
    }
    
    private void LoadManagerMessages()
    {
        var contact = contactData[ChatContactType.ManagerChen];
        
        contact.messages.Add(new MessageEntry
        {
            messageId = "manager_001",
            content = "沈老师，明天需要我派人陪你吗？",
            isFromPlayer = false,
            senderName = "陈经理",
            timeStamp = GetTodayTime(10, 20)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "manager_002",
            content = "不用，我自己去。",
            isFromPlayer = true,
            senderName = "沈凌",
            timeStamp = GetTodayTime(10, 22)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "manager_003",
            content = "好。下周能出初步报告吗？投资方催得紧。",
            isFromPlayer = false,
            senderName = "陈经理",
            timeStamp = GetTodayTime(10, 25)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "manager_004",
            content = "我先看看再说。",
            isFromPlayer = true,
            senderName = "沈凌",
            timeStamp = GetTodayTime(10, 26)
        });
    }
    
    private void LoadGroupMessages()
    {
        var contact = contactData[ChatContactType.ProjectGroup];
        
        contact.messages.Add(new MessageEntry
        {
            messageId = "group_001",
            content = "@沈凌 沈老师明天去现场，大家有什么要提醒的？",
            isFromPlayer = false,
            senderName = "陈经理",
            timeStamp = GetTodayTime(11, 05)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "group_002",
            content = "沈老师，开发后有什么亮点可以包装？",
            isFromPlayer = false,
            senderName = "小王（市场）",
            timeStamp = GetTodayTime(11, 08)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "group_003",
            content = "等我先看了再说。",
            isFromPlayer = true,
            senderName = "沈凌",
            timeStamp = GetTodayTime(11, 09)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "group_004",
            content = "底层中心柱据说有沉降，您重点看看。",
            isFromPlayer = false,
            senderName = "老赵（工程）",
            timeStamp = GetTodayTime(11, 13)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "group_005",
            content = "好的。",
            isFromPlayer = true,
            senderName = "沈凌",
            timeStamp = GetTodayTime(11, 14)
        });
    }
    
    private void LoadProfessorMessages()
    {
        var contact = contactData[ChatContactType.ProfessorZhou];
        
        contact.messages.Add(new MessageEntry
        {
            messageId = "prof_001",
            content = "听说你接了城郊那座塔的评估项目？",
            isFromPlayer = false,
            senderName = "周教授",
            timeStamp = GetTodayTime(14, 50)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "prof_002",
            content = "嗯，明天去看看。",
            isFromPlayer = true,
            senderName = "沈凌",
            timeStamp = GetTodayTime(14, 52)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "prof_003",
            content = "好。用专业说话，别被家里那套困住了。",
            isFromPlayer = false,
            senderName = "周教授",
            timeStamp = GetTodayTime(14, 57)
        });
        contact.messages.Add(new MessageEntry
        {
            messageId = "prof_004",
            content = "我明白。谢谢周老师。",
            isFromPlayer = true,
            senderName = "沈凌",
            timeStamp = GetTodayTime(15, 01)
        });
    }
    
    private float GetTodayTime(int hour, int minute)
    {
        System.DateTime today = System.DateTime.Today;
        System.DateTime time = new System.DateTime(today.Year, today.Month, today.Day, hour, minute, 0);
        return (float)(time - new System.DateTime(1970, 1, 1)).TotalSeconds;
    }
}