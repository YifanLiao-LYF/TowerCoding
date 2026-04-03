using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatEntryUI : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI previewText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject newBadge;
    [SerializeField] private Button clickButton;
    
    private ChatContactData contactData;
    private System.Action<ChatContactData> onClickCallback;
    
    public void Initialize(ChatContactData data, System.Action<ChatContactData> callback)
    {
        contactData = data;
        onClickCallback = callback;
        
        avatarImage.sprite = data.avatar;
        nameText.text = data.displayName;
        previewText.text = string.IsNullOrEmpty(data.latestMessagePreview) ? "暂无消息" : data.latestMessagePreview;
        newBadge.SetActive(data.hasNewMessage);
        
        // 显示最新消息的时间
        if (data.messages.Count > 0)
        {
            var lastMessage = data.messages[data.messages.Count - 1];
            timeText.text = TimeFormatter.FormatMessageTime(lastMessage.timeStamp);
        }
        
        clickButton.onClick.RemoveAllListeners();
        clickButton.onClick.AddListener(OnClick);
    }
    
    private void OnClick()
    {
        onClickCallback?.Invoke(contactData);
    }
    
    public void UpdateUI()
    {
        previewText.text = contactData.latestMessagePreview;
        newBadge.SetActive(contactData.hasNewMessage);
        
        if (contactData.messages.Count > 0)
        {
            var lastMessage = contactData.messages[contactData.messages.Count - 1];
            timeText.text = TimeFormatter.FormatMessageTime(lastMessage.timeStamp);
        }
    }
}