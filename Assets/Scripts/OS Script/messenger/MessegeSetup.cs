using UnityEngine;

public class MessageSetup : MonoBehaviour
{
    public MessageScheduler scheduler;

    private void Start()
    {
        if (scheduler == null) return;

        // 기존 예시 메시지 예약
        scheduler.ScheduleMessage(
            new MessengerProgram.MessageData(
                new GameDateTime(25, 1, 1, 8, 5),
                "상사",
                "회의 준비됐지?"
            )
        );

        scheduler.ScheduleMessage(
            new MessengerProgram.MessageData(
                new GameDateTime(25, 1, 1, 8, 30),
                "전임자",
                "신입, 점심 같이 가자!"
            )
        );
    }
}
