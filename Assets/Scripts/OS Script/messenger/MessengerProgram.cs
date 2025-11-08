using UnityEngine;
using UnityEngine.UI; // ScrollRect, Button
using TMPro;
using System;
using System.Collections.Generic;

public class MessengerProgram : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text chatContent;       // ScrollView 안의 대화 표시 텍스트
    public TMP_Text currentTargetText; // 현재 대화 상대 이름 표시
    public ScrollRect scrollRect;

    [Header("대화 전환 버튼")]
    public Button bossButton;
    public Button predecessorButton;

    [Header("시간 매니저 연결")]
    public OSTimeManager timeManager;  // 시뮬레이션 시간 참조

    [Header("설정")]
    public string currentTarget = "상사"; // 현재 대화 상대 ("상사" 또는 "전임자")

    private Dictionary<string, List<MessageData>> conversations = new Dictionary<string, List<MessageData>>();
    private List<MessageData> scheduledMessages = new List<MessageData>();

    void Start()
    {
        // 초기 대화 상대 등록
        conversations["상사"] = new List<MessageData>();
        conversations["전임자"] = new List<MessageData>();

        // 버튼 연결
        if (bossButton != null) bossButton.onClick.AddListener(() => ShowConversation("상사"));
        if (predecessorButton != null) predecessorButton.onClick.AddListener(() => ShowConversation("전임자"));

        // 테스트용 메시지 추가
        AddMessage(new GameDateTime(25, 1, 1, 8, 5), "상사", "회의 준비됐지?");
        AddMessage(new GameDateTime(25, 1, 1, 8, 30), "전임자", "신입, 점심 같이 가자!");
    }

    void Update()
    {
        if (timeManager == null) return;

        GameDateTime now = timeManager.GetCurrentGameTime();

        for (int i = scheduledMessages.Count - 1; i >= 0; i--)
        {
            var msg = scheduledMessages[i];
            if (msg.dateTime <= now)
            {
                DisplayMessage(msg);
                scheduledMessages.RemoveAt(i);
            }
        }
    }

    // 특정 시간에 메시지 예약
    public void AddMessage(GameDateTime time, string sender, string content)
    {
        MessageData msg = new MessageData(time, sender, content);
        scheduledMessages.Add(msg);
    }

    // 대화 상대 전환
    public void ShowConversation(string target)
    {
        if (!conversations.ContainsKey(target)) return;
        currentTarget = target;
        currentTargetText.text = target;

        chatContent.text = "";
        foreach (var msg in conversations[target])
        {
            AppendMessageToUI(msg);
        }

        ScrollToBottom();
    }

    // 실제 메시지 표시
    void DisplayMessage(MessageData msg)
    {
        if (!conversations.ContainsKey(msg.sender))
            conversations[msg.sender] = new List<MessageData>();

        // 상대 메시지 추가
        conversations[msg.sender].Add(msg);
        if (msg.sender == currentTarget)
            AppendMessageToUI(msg);

        // 내 자동 응답 ("네")
        MessageData myReply = new MessageData(timeManager.GetCurrentGameTime(), "나", "네");
        conversations[msg.sender].Add(myReply);
        if (msg.sender == currentTarget)
            AppendMessageToUI(myReply);

        ScrollToBottom();
    }

    // UI 출력
    void AppendMessageToUI(MessageData msg)
    {
        string color = msg.sender == "나" ? "green" : "blue";
        chatContent.text += $"<color={color}>[{msg.sender}] {msg.content}</color>\n";
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // 메시지 데이터 구조
    public struct MessageData
    {
        public GameDateTime dateTime;
        public string sender;
        public string content;

        public MessageData(GameDateTime dt, string sender, string content)
        {
            this.dateTime = dt;
            this.sender = sender;
            this.content = content;
        }
    }
}
[System.Serializable]
public struct GameDateTime : IComparable<GameDateTime>
{
    public int year, month, day, hour, minute;

    public GameDateTime(int y, int m, int d, int h, int min)
    {
        year = y;
        month = m;
        day = d;
        hour = h;
        minute = min;
    }

    // 비교 연산
    public int CompareTo(GameDateTime other)
    {
        if (year != other.year) return year.CompareTo(other.year);
        if (month != other.month) return month.CompareTo(other.month);
        if (day != other.day) return day.CompareTo(other.day);
        if (hour != other.hour) return hour.CompareTo(other.hour);
        return minute.CompareTo(other.minute);
    }

    public static bool operator <=(GameDateTime a, GameDateTime b) => a.CompareTo(b) <= 0;
    public static bool operator >=(GameDateTime a, GameDateTime b) => a.CompareTo(b) >= 0;
    public static bool operator <(GameDateTime a, GameDateTime b) => a.CompareTo(b) < 0;
    public static bool operator >(GameDateTime a, GameDateTime b) => a.CompareTo(b) > 0;

    public override string ToString()
    {
        return $"{year}/{month}/{day} {hour:00}:{minute:00}";
    }
}
