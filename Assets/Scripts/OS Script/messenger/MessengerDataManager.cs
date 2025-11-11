using System.Collections.Generic;
using UnityEngine;

public class MessengerDataManager : MonoBehaviour
{
    public static MessengerDataManager Instance;

    public List<MessengerChatUI.MessageData> scheduledMessages = new List<MessengerChatUI.MessageData>();
    public Dictionary<string, List<MessengerChatUI.MessageData>> conversations = new Dictionary<string, List<MessengerChatUI.MessageData>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddMessage(MessengerChatUI.MessageData msg)
    {
        if (!conversations.ContainsKey(msg.sender))
            conversations[msg.sender] = new List<MessengerChatUI.MessageData>();

        conversations[msg.sender].Add(msg);
    }

    public List<MessengerChatUI.MessageData> GetConversation(string sender)
    {
        if (conversations.ContainsKey(sender))
            return conversations[sender];
        return new List<MessengerChatUI.MessageData>();
    }
}
