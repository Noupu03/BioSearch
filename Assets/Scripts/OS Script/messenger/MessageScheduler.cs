using UnityEngine;

public class MessageScheduler : MonoBehaviour
{
    public OSTimeManager timeManager;
    public MessengerNotifier notifier; // Notifier 연결

    // 예약 메시지 저장 (MessageData)
    public void ScheduleMessage(MessengerChatUI.MessageData msg)
    {
        MessengerDataManager.Instance.scheduledMessages.Add(msg);
    }

    private void Update()
    {
        if (timeManager == null) return;

        var now = timeManager.GetCurrentGameTime();
        for (int i = MessengerDataManager.Instance.scheduledMessages.Count - 1; i >= 0; i--)
        {
            var msg = MessengerDataManager.Instance.scheduledMessages[i];
            if (msg.dateTime <= now)
            {
                MessengerDataManager.Instance.AddMessage(msg);

                if (notifier != null)
                    notifier.HandleMessageArrival(msg);

                MessengerDataManager.Instance.scheduledMessages.RemoveAt(i);
            }
        }
    }
}
