using UnityEngine;
using System.Collections.Generic;

public class MessageSetup : MonoBehaviour
{
    public MessageScheduler scheduler;
    public CheckList checkList;
    public OSTimeManager timeManager; // 인스펙터에서 할당

    //────────────────────────────────────────────
    // ① 랜덤 메시지 템플릿 구조
    //────────────────────────────────────────────
    [System.Serializable]
    public class MessageTemplate
    {
        public string sender;
        public string message;
        public string checklistText;
        public string linkedFileName;

        public MessageTemplate(string sender, string message, string checklistText, string linkedFileName)
        {
            this.sender = sender;
            this.message = message;
            this.checklistText = checklistText;
            this.linkedFileName = linkedFileName;
        }
    }

    //────────────────────────────────────────────
    //  5종류의 랜덤 메시지 템플릿
    //────────────────────────────────────────────
    private List<MessageTemplate> randomTemplates = new List<MessageTemplate>
    {
        new MessageTemplate(
            "상사",
            "아마 슬슬 발전기에 녹이 슬었을거다. 해결 해라.",
            "발전기의 녹을 제거할 방법 찾기.",
            "녹슬었을때"
        ),
        new MessageTemplate(
            "상사",
            "발전기 배터리를 교체할 때가 된거 같다. 교체할 방법을 찾아라.",
            "발전기 배터리를 교체할 방법 찾기.",
            "배터리교체방법"
        ),
        new MessageTemplate(
            "상사",
            "발전기 내부 점검표 체크해라.",
            "발전기 내부 점검 보고서 확인하기.",
            "점검보고서"
        ),
        new MessageTemplate(
            "상사",
            "엔진에서 이상 소리가 난다. 원인을 조사해라.",
            "엔진 이상 소음의 원인 찾기.",
            "엔진이상소음"
        ),
        new MessageTemplate(
            "상사",
            "안전 매뉴얼을 다시 읽어봐라. 실수하지 말고.",
            "안전 매뉴얼 다시 읽기.",
            "안전매뉴얼"
        )
    };

    private void Start()
    {
        if (scheduler == null) return;

        // 기존 메시지 예약들
        presetMessages();
    }

    private void presetMessages()/////실제로 메시지를 예약 거는 코드 부분. 중요!!
    {
        /* 여기 원래 있던 메시지 등록 부분 그대로…
        GameDateTime t1 = new GameDateTime(25, 1, 1, 8, 10);
        string sender1 = "상사";
        string text1 = "아마 슬슬 발전기에 녹이 슬었을거다. 해결 해라.";

        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(t1, sender1, text1));
        MessengerDataManager.Instance.RegisterMapping(t1.ToString(), sender1, text1,
            "발전기의 녹을 제거할 방법 찾기.",
            "녹슬었을때");

        GameDateTime t2 = new GameDateTime(25, 1, 1, 8, 12);
        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(t2, "상사", "체크리스트 작성 꼭 하고."));

        GameDateTime t3 = new GameDateTime(25, 1, 1, 8, 30);
        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(t3, "전임자", "신입, 점심 같이 가자!"));
        기존 예시들..*/

        GameDateTime t1 = new GameDateTime(25, 1, 1, 8, 10);
        SpawnRandomScheduledMessage(t1);

        GameDateTime t2 = new GameDateTime(25, 1, 1, 8, 12);
        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(t2, "상사", "체크리스트 작성 꼭 하고."));

        scheduler.ScheduleRandomErrors(t1);

        GameDateTime t3 = new GameDateTime(25, 1, 1, 8, 30);
        GameDateTime t4 = new GameDateTime(25, 1, 1, 21, 0);
        scheduler.ScheduleInitializeAllStates(t4);


    }

    //────────────────────────────────────────────
    //  랜덤 메시지 예약 함수
    //────────────────────────────────────────────
    public void SpawnRandomScheduledMessage(GameDateTime time)
    {
        if (scheduler == null) return;

        // 1) 랜덤 메시지 선택
        int index = Random.Range(0, randomTemplates.Count);
        MessageTemplate chosen = randomTemplates[index];

        // 2) 메시지 생성 및 예약
        MessengerChatUI.MessageData msg = new MessengerChatUI.MessageData(
            time,
            chosen.sender,
            chosen.message
        );
        scheduler.ScheduleMessage(msg);

        var now = timeManager.GetCurrentGameTime();

        if (time.year > now.year || time.month > now.month || time.day >= now.day)
        {
            MessengerDataManager.Instance.RegisterMapping(
                msg.dateTime,        // GameDateTime 그대로
                chosen.sender,
                chosen.message,
                chosen.checklistText,
                chosen.linkedFileName
            );
        }
        else
        {
            Debug.Log($"[SpawnRandomScheduledMessage] 하루가 지난 메시지이므로 체크리스트 등록 무시: {chosen.message}"); 
        }
        

        //────────────────────────────────────────────
        // 4) ★ 파일 중요 표시 (isImportant = true)
        //────────────────────────────────────────────
        // 파일 탐색 (프로젝트 파일 구조에 맞춰 함수명 수정)
        File file = FileWindow.Instance.FindFileByName(chosen.linkedFileName);

        if (file != null)
        {
            file.isImportant = true;

#if UNITY_EDITOR
            Debug.Log($"[File Important] '{chosen.linkedFileName}' 파일의 isImportant = true");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[File Important] 파일 '{chosen.linkedFileName}' 을 찾을 수 없습니다!");
#endif
        }

#if UNITY_EDITOR
        Debug.Log($"[Random MSG] '{chosen.sender}' : '{chosen.message}' 예약됨 → 중요파일 {chosen.linkedFileName}");
#endif
    }


}
