using UnityEngine;
using TMPro;
using System;

public class OSTimeManager : MonoBehaviour
{
    [Header("UI 표시용 Text")]
    public TextMeshProUGUI timeText;

    [Header("시작 날짜/시간")]
    public int year = 25;
    public int month = 1;
    public int day = 1;
    public int hour = 8;
    public int minute = 0;

    [Header("1초당 흐르는 분 단위 (예: 1초 = 1분)")]
    public float timeSpeed = 1f;

    private float timer = 0f;
    private bool isAM = true;

    void Update()
    {
        timer += Time.deltaTime * timeSpeed;
        if (timer >= 1f)
        {
            minute += 1;
            timer = 0f;

            if (minute >= 60)
            {
                minute = 0;
                hour++;
            }

            if (hour >= 24)
            {
                hour = 0;
                day++;
                // 간단히 31일 기준 (원하면 각 달별 일수 처리 가능)
                if (day > 31)
                {
                    day = 1;
                    month++;
                    if (month > 12)
                    {
                        month = 1;
                        year++;
                    }
                }
            }

            UpdateDisplay();
        }
    }

    public void UpdateDisplay()
    {
        // 12시간제 변환
        int displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12;
        isAM = hour < 12;

        string ampm = isAM ? "am" : "pm";
        string timeString = $"{year}/{month}/{day}  {displayHour:00}:{minute:00} {ampm}";
        if (timeText != null)
            timeText.text = timeString;
    }
    public GameDateTime GetCurrentGameTime()
    {
        return new GameDateTime(year, month, day, hour, minute);
    }
}
