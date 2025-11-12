using UnityEngine;
using System.Collections.Generic;

public class MessageScheduler : MonoBehaviour
{
    public OSTimeManager timeManager;
    public MessengerNotifier notifier; // Notifier 연결

    // 예약 메시지 저장 (시간 순서대로 관리)
    private List<MessengerChatUI.MessageData> scheduledMessages = new List<MessengerChatUI.MessageData>();

    // 메시지 예약
    public void ScheduleMessage(MessengerChatUI.MessageData msg)
    {
        scheduledMessages.Add(msg);
    }

    private void Update()
    {
        if (timeManager == null) return;

        var now = timeManager.GetCurrentGameTime();

        // 예약 메시지 체크 (역순 삭제)
        for (int i = scheduledMessages.Count - 1; i >= 0; i--)
        {
            var msg = scheduledMessages[i];
            if (msg.dateTime <= now)
            {
                // 대화 상대 기준으로 DataManager에 저장
                string conversationKey = msg.sender; // sender 기준
                MessengerDataManager.Instance.AddMessage(msg, conversationKey);

                // 알림 처리
                if (notifier != null)
                    notifier.HandleMessageArrival(msg);

                // 예약 리스트에서 제거
                scheduledMessages.RemoveAt(i);
            }
        }
    }
}
