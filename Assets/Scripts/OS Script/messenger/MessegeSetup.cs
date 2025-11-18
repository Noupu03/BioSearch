using UnityEngine;

public class MessageSetup : MonoBehaviour
{
    public MessageScheduler scheduler;
    public CheckList checkList;

    private void Start()
    {
        if (scheduler == null) return;

        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(
            new GameDateTime(25, 1, 1, 8, 10), "상사", "아마 슬슬 발전기에 녹이 슬었을거다. 해결 해라."
        ));

        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(
            new GameDateTime(25, 1, 1, 8, 12), "상사", "체크리스트 작성 꼭 하고."
        ));

        CheckList.Instance.RegisterStrikeMapping(
            "녹슬었을때",
            "발전기 의 녹을 제거할 방법 찾기."
        );

        ScheduleCheckList(new GameDateTime(25, 1, 1, 8, 12),
                          "발전기 의 녹을 제거할 방법 찾기.");

        scheduler.ScheduleMessage(new MessengerChatUI.MessageData(
            new GameDateTime(25, 1, 1, 8, 30), "전임자", "신입, 점심 같이 가자!"
        ));
    }

    /// <summary>
    /// 지정된 시간에 체크리스트 항목 추가
    /// </summary>
    void ScheduleCheckList(GameDateTime time, string itemText)
    {
        if (checkList == null) return;

        scheduler.ScheduleAction(time, () =>
        {
            checkList.AddCheckList(itemText);
            Debug.Log($"[CheckList] Added at {time}: {itemText}");
        });
    }
}
