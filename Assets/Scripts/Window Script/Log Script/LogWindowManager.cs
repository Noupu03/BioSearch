using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 로그 출력 및 명령어 입력 관리
/// - 메시지 큐 기반 출력 (타이핑 효과)
/// - 최대 라인 수 관리
/// - 입력 필드 비활성화/활성화 지원
/// </summary>
public class LogWindowManager : MonoBehaviour
{
    public static LogWindowManager Instance;

    [Header("UI References")]
    public TMP_Text logText;
    public TMP_InputField inputField;
    public ScrollRect scrollRect;

    [Header("Settings")]
    public int maxLines = 50;
    public float charDelay = 0.01f;

    private string[] lines;
    private int currentLine = 0;
    private StringBuilder sb;

    private bool autoScroll = false;
    private bool userScrolling = false;

    private readonly Queue<string> messageQueue = new Queue<string>();
    private bool isTyping = false;

    public delegate void ScanCommandHandler(string folderName);
    public event ScanCommandHandler OnScanCommandEntered;

    public delegate void ExtenseCommandHandler(string args);
    public event ExtenseCommandHandler OnExtenseCommandEntered;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        lines = new string[maxLines];
        sb = new StringBuilder();

        logText.text = "";
        inputField.onSubmit.AddListener(OnInputSubmitted);
        inputField.ActivateInputField();

        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;
        scrollRect.content = logText.rectTransform;
        scrollRect.onValueChanged.AddListener(_ => userScrolling = true);

        logText.rectTransform.pivot = new Vector2(0, 0);
        logText.rectTransform.anchorMin = new Vector2(0, 0);
        logText.rectTransform.anchorMax = new Vector2(1, 0);

        int stage = ScoreCount.stageCount;
        Log($"{stage}번 피검사자 검사실에 배치..");
        Log(".......complete");
        Log("해당 피검사자 BioSearch system 연결..");
        Log(".......complete");
        Log("BioSearch에서 사용자의 정신 연결..");
        Log(".......complete");

    }

    private void LateUpdate()
    {
        float contentHeight = logText.preferredHeight;
        Vector2 size = logText.rectTransform.sizeDelta;
        logText.rectTransform.sizeDelta = new Vector2(size.x, contentHeight);

        if (autoScroll && !userScrolling)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            autoScroll = false;
        }

        userScrolling = false;
    }

    /// <summary>
    /// 외부에서 로그 추가
    /// </summary>
    public void Log(string message)
    {
        messageQueue.Enqueue(message);
        if (!isTyping)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isTyping = true;

        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            lines[currentLine % maxLines] = "> " + message;
            currentLine++;

            sb.Clear();
            int start = Mathf.Max(0, currentLine - maxLines);
            for (int i = start; i < currentLine - 1; i++)
                sb.AppendLine(lines[i % maxLines]);

            logText.text = sb.ToString();

            string newLine = lines[(currentLine - 1) % maxLines];
            for (int i = 0; i < newLine.Length; i++)
            {
                logText.text = sb.ToString() + newLine.Substring(0, i + 1);
                yield return new WaitForSeconds(charDelay);
            }

            autoScroll = true;
        }

        isTyping = false;
    }

    private void OnInputSubmitted(string command)
    {
        if (!inputField.interactable || string.IsNullOrWhiteSpace(command)) return;

        Log("명령어 입력: " + command);

        command = command.Trim().ToLower();

        if (command.StartsWith("scan "))
        {
            string folderName = command.Substring(5).Trim();
            OnScanCommandEntered?.Invoke(folderName);
        }
        else if (command.StartsWith("extense "))
        {
            string args = command.Substring(8).Trim();
            OnExtenseCommandEntered?.Invoke(args);
        }
        else if (command == "help")
        {
            Log("사용 가능한 명령어: scan [폴더명], extense [파일명] [새 확장자], help, clear");
        }
        else if (command == "clear")
        {
            ClearLog();
        }
        else
        {
            Log("잘못된 명령어 입력됨.");
        }

        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void ClearLog()
    {
        currentLine = 0;
        sb.Clear();
        logText.text = "";
        autoScroll = true;
    }

    /// <summary>
    /// 마지막 로그 한 줄 교체
    /// </summary>
    public void ReplaceLastScanLog(string message)
    {
        if (lines == null || lines.Length == 0) return;

        // 역순으로 탐색해서 "이상 스캔중"으로 시작하는 마지막 로그 찾기
        for (int i = currentLine - 1; i >= 0; i--)
        {
            int index = (i + maxLines) % maxLines;
            if (lines[index].StartsWith("> 이상 스캔중"))
            {
                lines[index] = "> " + message; // 갱신
                break;
            }
        }

        // 전체 텍스트 갱신
        sb.Clear();
        int start = Mathf.Max(0, currentLine - maxLines);
        for (int i = start; i < currentLine; i++)
            sb.AppendLine(lines[i % maxLines]);

        logText.text = sb.ToString();
    }


    /// <summary>
    /// 입력 필드 비활성화
    /// </summary>
    public void DisableInput()
    {
        inputField.interactable = false;
        inputField.readOnly = true;
    }

    /// <summary>
    /// 입력 필드 활성화
    /// </summary>
    public void EnableInput()
    {
        inputField.interactable = true;
        inputField.readOnly = false;
        inputField.ActivateInputField();
    }
}
