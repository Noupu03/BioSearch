using UnityEngine;
using System;
using System.Collections.Generic;

public class MessageScheduler : MonoBehaviour
{
    public OSTimeManager timeManager;
    public MessengerNotifier notifier;
    SubmissionChecker submissionChecker;

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
    //===========================================================
    // ★ MessageSetup에서 사용: 초기화 스케줄 예약 전용 함수
    //===========================================================
    public void ScheduleInitializeAllStates(GameDateTime time)
    {
        Debug.Log($"[Scheduler] InitializeAllStates 예약됨 → {time}");

        ScheduleAction(time, () =>
        {
            Debug.Log("[Scheduler] InitializeAllStates 실행됨!");
            InitializeAllStates();
        });
    }

    //===========================================================
    // 전체 초기화 함수  여기서 모든 초기화 실행
    //===========================================================
    private void InitializeAllStates()
    {
        
        // 1. 체크리스트 및 메시지 텍스트 초기화
        CheckList.Instance?.ResetCheckListAndText();

        // 2. Body Parts Viewer 초기화
        MachinePartViewer.Instance?.InitializeViewerState();

        // 3. File 전체 초기화
        FileWindow.Instance?.InitializeAllFileState();

        Debug.Log("[Scheduler] 모든 상태 초기화 완료");
    }

    // =====================================
    // ★ 지정 시간에 MachinePartViewer 랜덤 에러 적용
    // =====================================
    public void ScheduleRandomErrors(GameDateTime time, float errorChance = 0.1f)
    {
        ScheduleAction(time, () =>
        {
            if (MachinePartViewer.Instance != null)
            {
                MachinePartViewer.Instance.RandomlySetErrors(errorChance);
#if UNITY_EDITOR
                Debug.Log($"[Scheduler] {time}에 랜덤 에러 적용 완료 (확률: {errorChance * 100}%)");
#endif
            }
        });
    }

    /// <summary>
    /// 지정 시간에 파일 및 부품 체크 상태를 점검하고 결과 로그 출력
    /// </summary>
    public void ScheduleSubmissionCheck(GameDateTime time)
    {
        ScheduleAction(time, () =>
        {
            Debug.Log("[Submission Check] =====");

            // 1) 파일 체크 상태 출력
            string fileStatus = SubmissionChecker.CheckFilesStatus();
            Debug.Log(fileStatus);

            // 2) 부품 체크 상태 출력
            string partStatus = SubmissionChecker.CheckPartsStatus();
            Debug.Log(partStatus);

            Debug.Log("[Submission Check] ===== 완료");
        });
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
