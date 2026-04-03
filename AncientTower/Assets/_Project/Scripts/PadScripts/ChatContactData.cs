using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChatContact", menuName = "Game/ChatContact")]
public class ChatContactData : ScriptableObject
{
    public ChatContactType contactType;
    public string displayName;
    public Sprite avatar;
    public List<MessageEntry> messages = new List<MessageEntry>();
    public string latestMessagePreview;
    public bool hasNewMessage;
}