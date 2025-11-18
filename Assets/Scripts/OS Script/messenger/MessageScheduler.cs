using UnityEngine;
using System;
using System.Collections.Generic;

public class MessageScheduler : MonoBehaviour
{
    public OSTimeManager timeManager;
    public MessengerNotifier notifier;

    // 예약 메시지 저장
    private List<MessengerChatUI.MessageData> scheduledMessages = new List<MessengerChatUI.MessageData>();

    // 시간 기반 콜백 예약
    private class ScheduledAction
    {
        public GameDateTime time;
        public Action action;
    }
    private List<ScheduledAction> scheduledActions = new List<ScheduledAction>();

    // ===============================
    // 메시지 예약
    // ===============================
    public void ScheduleMessage(MessengerChatUI.MessageData msg)
    {
            scheduledMessages.Add(msg);
    }

    // ===============================
    // 시간 기반 액션 예약
    // ===============================
    public void ScheduleAction(GameDateTime time, Action callback)
    {
        if (callback == null) return;
        scheduledActions.Add(new ScheduledAction { time = time, action = callback });
    }

    // ===============================
    // Update: 예약 메시지 & 액션 처리
    // ===============================
    private void Update()
    {
        if (timeManager == null) return;

        var now = timeManager.GetCurrentGameTime();

        // ------------------------
        // 예약 메시지 처리
        // ------------------------
        var messagesToProcess = new List<MessengerChatUI.MessageData>();
        foreach (var msg in scheduledMessages)
        {
            if (msg.dateTime <= now)
                messagesToProcess.Add(msg);
        }

        foreach (var msg in messagesToProcess)
        {
            string conversationKey = msg.sender;
            MessengerDataManager.Instance.AddMessage(msg, conversationKey);

            if (notifier != null)
                notifier.HandleMessageArrival(msg);

            scheduledMessages.Remove(msg);
        }

        // ------------------------
        // 예약 액션 처리
        // ------------------------
        var actionsToExecute = new List<ScheduledAction>();
        foreach (var sa in scheduledActions)
        {
            if (sa.time <= now)
                actionsToExecute.Add(sa);
        }

        foreach (var sa in actionsToExecute)
        {
            sa.action?.Invoke();
            scheduledActions.Remove(sa);
        }
    }
}
