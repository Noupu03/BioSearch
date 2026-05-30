using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

/// <summary>
/// 로그 창 및 명령어 입력 관리.
/// - 메시지 큐 처리 (타이핑 효과)
/// - 최대 줄 수 제한
/// - 입력 필드 활성화/비활성화
/// </summary>
public class LogWindowManager : MonoBehaviour
{
    public static LogWindowManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text       logText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private ScrollRect     scrollRect;

    [Header("설정")]
    [SerializeField] private int   maxLines  = GameConfig.LogMaxLines;
    [SerializeField] private float charDelay = 0.01f;

    private string[]      lines;
    private int           currentLine;
    private StringBuilder sb;
    private bool          needsScroll;
    private bool          isTyping;

    private readonly System.Collections.Generic.Queue<string> messageQueue =
        new System.Collections.Generic.Queue<string>();

    public event System.Action<string> OnScanCommandEntered;
    public event System.Action<string> OnExtenseCommandEntered;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        lines = new string[maxLines];
        sb    = new StringBuilder();

        logText.text = "";

        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.inertia      = true;
        scrollRect.content      = logText.rectTransform;

        logText.rectTransform.pivot     = new Vector2(0, 0);
        logText.rectTransform.anchorMin = new Vector2(0, 0);
        logText.rectTransform.anchorMax = new Vector2(1, 0);

        inputField.onSubmit.AddListener(OnInputSubmitted);
        // 텍스트가 바뀔 때만 높이 재계산 (LateUpdate 폴링 제거)
        logText.RegisterDirtyLayoutCallback(OnLogTextDirty);

        inputField.ActivateInputField();

        Log($"{ScoreCount.StageCount}번째 검사자 발견..");
        Log(".......complete");
        Log("BioSearch system 접속..");
        Log(".......complete");
        Log("BioSearch에서 대기중입니다..");
        Log(".......complete");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (inputField != null) inputField.onSubmit.RemoveListener(OnInputSubmitted);
        if (logText != null)    logText.UnregisterDirtyLayoutCallback(OnLogTextDirty);
    }

    // ── 텍스트 변경 콜백 (LateUpdate 대체) ─────────
    private void OnLogTextDirty()
    {
        // 텍스트가 dirty 상태일 때 다음 프레임에 높이 업데이트 (Canvas.ForceUpdateCanvases 이후)
        needsScroll = true;
    }

    void LateUpdate()
    {
        if (!needsScroll) return;
        needsScroll = false;

        float h = logText.preferredHeight;
        logText.rectTransform.sizeDelta = new Vector2(logText.rectTransform.sizeDelta.x, h);
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // ── 로그 API ─────────────────────────────────
    public void Log(string message)
    {
        messageQueue.Enqueue(message);
        if (!isTyping) StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isTyping = true;

        while (messageQueue.Count > 0)
        {
            string msg = messageQueue.Dequeue();
            lines[currentLine % maxLines] = "> " + msg;
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

            needsScroll = true;
        }

        isTyping = false;
    }

    public void ReplaceLastScanLog(string message)
    {
        if (lines == null) return;

        for (int i = currentLine - 1; i >= 0; i--)
        {
            int idx = (i + maxLines) % maxLines;
            if (lines[idx].StartsWith("> 이상 스캔 중"))
            {
                lines[idx] = "> " + message;
                break;
            }
        }

        sb.Clear();
        int s2 = Mathf.Max(0, currentLine - maxLines);
        for (int i = s2; i < currentLine; i++) sb.AppendLine(lines[i % maxLines]);
        logText.text = sb.ToString();
    }

    public void ClearLog()
    {
        currentLine = 0;
        sb.Clear();
        logText.text = "";
        needsScroll  = true;
    }

    public void DisableInput()
    {
        inputField.interactable = false;
        inputField.readOnly     = true;
    }

    public void EnableInput()
    {
        inputField.interactable = true;
        inputField.readOnly     = false;
        inputField.ActivateInputField();
    }

    // ── 명령어 처리 ──────────────────────────────
    private void OnInputSubmitted(string command)
    {
        if (!inputField.interactable || string.IsNullOrWhiteSpace(command)) return;

        Log("명령어 입력: " + command);
        command = command.Trim().ToLower();

        if      (command.StartsWith("scan "))    OnScanCommandEntered?.Invoke(command.Substring(5).Trim());
        else if (command.StartsWith("extense ")) OnExtenseCommandEntered?.Invoke(command.Substring(8).Trim());
        else if (command == "help")  Log("사용 가능 명령어: scan [폴더명], extense [파일명] [새 확장자], help, clear");
        else if (command == "clear") ClearLog();
        else                         Log("알 수 없는 명령어입니다.");

        inputField.text = "";
        inputField.ActivateInputField();
    }
}
