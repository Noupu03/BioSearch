using UnityEngine;
using System.Collections;

public class SkipDayOnClick : MonoBehaviour
{
    public OSTimeManager timeManager;
    public int targetHour = 8;
    public int targetMinute = 0;
    public int minutesPerFrame = 60; // 스킵 시 한 프레임당 흐르는 분

    private void OnMouseDown()
    {
        if (timeManager == null)
        {
            Debug.LogWarning("TimeManager 미설정!");
            return;
        }

        StartCoroutine(FastForwardToNextMorning());
    }

    private IEnumerator FastForwardToNextMorning()
    {
        GameDateTime now = timeManager.GetCurrentGameTime();

        int nextDay = now.day + 1;
        int nextMonth = now.month;
        int nextYear = now.year;

        if (nextDay > 31)
        {
            nextDay = 1;
            nextMonth++;
            if (nextMonth > 12)
            {
                nextMonth = 1;
                nextYear++;
            }
        }

        GameDateTime target = new GameDateTime(nextYear, nextMonth, nextDay, targetHour, targetMinute);

        while (now < target)
        {
            now.minute += minutesPerFrame;
            now.NormalizeTime();

            timeManager.year = now.year;
            timeManager.month = now.month;
            timeManager.day = now.day;
            timeManager.hour = now.hour;
            timeManager.minute = now.minute;
            timeManager.UpdateDisplay();

            // InputManager S키 강제 트리거
            InputManager.Instance?.TriggerS(); // S키 강제 입력
            yield return null;
        }

        // 최종 시간 세팅
        timeManager.year = target.year;
        timeManager.month = target.month;
        timeManager.day = target.day;
        timeManager.hour = target.hour;
        timeManager.minute = target.minute;
        timeManager.UpdateDisplay();

        // 마지막에도 S키 이벤트 호출
        InputManager.Instance?.TriggerS(); // S키 강제 입력

        Debug.Log($"[Skip Completed] 현재 시간: {target}");
    }
}
