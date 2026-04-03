using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageEntryUI : MonoBehaviour
{
    [SerializeField] private Image bubbleImage;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI senderNameText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [SerializeField] private Color playerBubbleColor = new Color(0.2f, 0.6f, 0.9f);
    [SerializeField] private Color otherBubbleColor = new Color(0.9f, 0.9f, 0.9f);
    
    private RectTransform rectTransform;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void SetMessage(MessageEntry message, bool isGroupChat, bool isLeftSide)
    {
        contentText.text = message.content;
        timeText.text = TimeFormatter.FormatMessageTime(message.timeStamp);
        
        SetAlignment(isLeftSide);
        SetBubbleColor(message.isFromPlayer, isGroupChat);
        
        if (isGroupChat && !message.isFromPlayer)
        {
            senderNameText.gameObject.SetActive(true);
            senderNameText.text = message.senderName;
        }
        else
        {
            senderNameText.gameObject.SetActive(false);
        }
    }
    
    private void SetAlignment(bool isLeftSide)
    {
        if (isLeftSide)
        {
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchoredPosition = new Vector2(10, 0);
        }
        else
        {
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-10, 0);
        }
    }
    
    private void SetBubbleColor(bool isFromPlayer, bool isGroupChat)
    {
        if (isFromPlayer)
        {
            bubbleImage.color = playerBubbleColor;
            contentText.color = Color.white;
        }
        else
        {
            bubbleImage.color = otherBubbleColor;
            contentText.color = Color.black;
        }
    }
}