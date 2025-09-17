using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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

    // scan 명령어 이벤트
    public delegate void ScanCommandHandler(string fileName);
    public event ScanCommandHandler OnScanCommandEntered;

    // 메시지 큐
    private Queue<string> messageQueue = new Queue<string>();
    private bool isTyping = false;

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

            // 이전 로그 먼저 출력
            for (int i = start; i < currentLine - 1; i++)
                sb.AppendLine(lines[i % maxLines]);

            logText.text = sb.ToString();

            // 새 메시지 한 글자씩 출력
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
        if (string.IsNullOrWhiteSpace(command)) return;

        Log("명령어 입력: " + command);

        command = command.Trim().ToLower();

        // 명령어 처리
        if (command.StartsWith("scan "))
        {
            string fileName = command.Substring(5).Trim();
            OnScanCommandEntered?.Invoke(fileName);
        }
        else if (command == "help")
        {
            Log("사용 가능한 명령어: scan [파일명], help, clear");
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

}
