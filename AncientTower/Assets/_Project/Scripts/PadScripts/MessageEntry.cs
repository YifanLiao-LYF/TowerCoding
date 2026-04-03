using System;

[Serializable]
public class MessageEntry
{
    public string messageId;
    public string content;
    public bool isFromPlayer;  // true:玩家, false:对方
    public string senderName;
    public float timeStamp;
}