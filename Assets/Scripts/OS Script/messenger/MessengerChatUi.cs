using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MessengerChatUI : MonoBehaviour
{
    public TMP_Text currentTargetText;
    public TMP_Text chatContent;

    [HideInInspector]
    public string target;

    private List<MessageData> conversation = new List<MessageData>();

    public struct MessageData
    {
        public GameDateTime dateTime;
        public string sender;
        public string content;

        public MessageData(GameDateTime dt, string sender, string content)
        {
            dateTime = dt;
            this.sender = sender;
            this.content = content;
        }
    }

    // 대화 상대 지정
    public void SetConversation(List<MessageData> msgs)
    {
        conversation = msgs;
        target = (msgs.Count > 0) ? msgs[0].sender : "";
    }

    // UI 갱신
    public void RefreshChat()
    {
        if (currentTargetText != null)
            currentTargetText.text = target;

        chatContent.text = "";
        foreach (var msg in conversation)
        {
            if (msg.sender == "나")
                chatContent.text += $"[{msg.sender}] : {msg.content}\n";
            else
                chatContent.text += $"[{msg.sender}] : {msg.content} ({msg.dateTime})\n";
        }
    }
}
