using UnityEngine;

public class MessageSetup : MonoBehaviour
{
    public MessageScheduler scheduler;
    public CheckList checkList;

    private void Start()
    {
        if (scheduler == null) return;

        // 예: 시간, sender, text
        GameDateTime t1 = new GameDateTime(25, 1, 1, 8, 10);
        string sender1 = "상사";
        string text1 = "아마 슬슬 발전기에 녹이 슬었을거다. 해결 해라.";

        // 예약 메세지 생성
        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(t1, sender1, text1));
        // 매핑 등록 (이 메시지를 읽으면 체크리스트가 추가되게 할 경우)
        MessengerDataManager.Instance.RegisterMapping(t1.ToString(), sender1, text1,
            "발전기 의 녹을 제거할 방법 찾기.", // 체크리스트 텍스트
            "녹슬었을때" // 매핑된 파일 이름 (File.name 기준)
        );

        // 두번째 메시지
        GameDateTime t2 = new GameDateTime(25, 1, 1, 8, 12);
        string sender2 = "상사";
        string text2 = "체크리스트 작성 꼭 하고.";
        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(t2, sender2, text2));

        // 전임자 메시지 (예)
        GameDateTime t3 = new GameDateTime(25, 1, 1, 8, 30);
        string sender3 = "전임자";
        string text3 = "신입, 점심 같이 가자!";
        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(t3, sender3, text3));
        // 이 메시지는 체크리스트와 관련 없음 → 등록 안 해도 됨
    }

    // ScheduleCheckList 함수는 더 이상 사용하지 않음 (메시지 기반으로 변경)
}
