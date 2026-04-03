using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChatDetailController : MonoBehaviour
{
    [SerializeField] private Transform messageContent;
    [SerializeField] private MessageEntryUI messagePrefab;
    [SerializeField] private TextMeshProUGUI contactNameText;
    [SerializeField] private Image contactAvatarImage;
    [SerializeField] private Button backButton;
    [SerializeField] private ScrollRect messageScrollRect;
    
    private ChatContactData currentContact;
    private List<MessageEntryUI> currentMessages = new List<MessageEntryUI>();
    
    void Start()
    {
        backButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
    
    public void LoadMessages(ChatContactData contact)
    {
        currentContact = contact;
        
        contactNameText.text = contact.displayName;
        contactAvatarImage.sprite = contact.avatar;
        
        ClearMessages();
        
        bool isGroupChat = (contact.contactType == ChatContactType.ProjectGroup);
        
        foreach (var message in contact.messages)
        {
            AddMessage(message, isGroupChat);
        }
        
        ScrollToBottom();
    }
    
    private void AddMessage(MessageEntry message, bool isGroupChat)
    {
        var msgObj = Instantiate(messagePrefab, messageContent);
        bool isLeftSide = !message.isFromPlayer;
        msgObj.SetMessage(message, isGroupChat, isLeftSide);
        currentMessages.Add(msgObj);
    }
    
    private void ClearMessages()
    {
        foreach (var msg in currentMessages)
        {
            Destroy(msg.gameObject);
        }
        currentMessages.Clear();
    }
    
    private void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }
    
    private System.Collections.IEnumerator ScrollToBottomCoroutine()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        messageScrollRect.verticalNormalizedPosition = 0f;
    }
}