using System.Collections;
using UnityEngine;

public class MessageScheduler : MonoBehaviour
{
    public OSTimeManager timeManager;        // 게임 시간 참조
    public MessengerProgram messengerProgram; // 메시지 수신용

    private void Update()
    {
        if (timeManager == null || messengerProgram == null) return;

        var now = timeManager.GetCurrentGameTime();
        for (int i = MessengerDataManager.Instance.scheduledMessages.Count - 1; i >= 0; i--)
        {
            var msg = MessengerDataManager.Instance.scheduledMessages[i];
            if (msg.dateTime <= now)
            {
                messengerProgram.ReceiveExternalMessage(msg);
                MessengerDataManager.Instance.scheduledMessages.RemoveAt(i);
            }
        }
    }

    // 외부에서 메시지 예약
    public void ScheduleMessage(MessengerProgram.MessageData msg)
    {
        MessengerDataManager.Instance.scheduledMessages.Add(msg);
    }
}
