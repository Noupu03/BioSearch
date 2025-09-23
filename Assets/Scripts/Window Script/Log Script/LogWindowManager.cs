using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// �α� ��� �� ��ɾ� �Է��� �����ϴ� Ŭ����
/// - �Է� �ʵ忡�� ��ɾ� ó��
/// - �޽��� ť ��� ��� (�� ���ھ� Ÿ���� ȿ��)
/// - ��ũ�� �ڵ� ����
/// </summary>
public class LogWindowManager : MonoBehaviour
{
    public static LogWindowManager Instance;

    [Header("UI References")]
    public TMP_Text logText;                  // �αװ� ǥ�õ� �ؽ�Ʈ
    public TMP_InputField inputField;         // ��ɾ� �Է� �ʵ�
    public ScrollRect scrollRect;             // ��ũ�� �����

    [Header("Settings")]
    public int maxLines = 50;                 // �ִ� ���� �α� ���� ��
    public float charDelay = 0.01f;           // ���� ��� ���� (Ÿ���� ȿ��)

    private string[] lines;                   // �α� ���� ���� ����
    private int currentLine = 0;              // ���� ���� �ε���
    private StringBuilder sb;                 // �α� ���ڿ� ���ձ�

    private bool autoScroll = false;          // �� �α� ��� �� �ڵ� ��ũ�� ����
    private bool userScrolling = false;       // ����ڰ� ��ũ���� ���������� ����

    // scan ��ɾ� �̺�Ʈ
    public delegate void ScanCommandHandler(string fileName);
    public event ScanCommandHandler OnScanCommandEntered;

    // �޽��� ť (Ÿ���� ȿ���� ���� ���)
    private readonly Queue<string> messageQueue = new Queue<string>();
    private bool isTyping = false;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        lines = new string[maxLines];
        sb = new StringBuilder();

        // �ʱ� ����
        logText.text = "";
        inputField.onSubmit.AddListener(OnInputSubmitted);
        inputField.ActivateInputField();

        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        // ��ũ�� ����
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;
        scrollRect.content = logText.rectTransform;
        scrollRect.onValueChanged.AddListener(_ => userScrolling = true);

        // �ؽ�Ʈ ��ġ ���� (�Ʒ��� ����)
        logText.rectTransform.pivot = new Vector2(0, 0);
        logText.rectTransform.anchorMin = new Vector2(0, 0);
        logText.rectTransform.anchorMax = new Vector2(1, 0);
    }

    private void LateUpdate()
    {
        // �ؽ�Ʈ ���̸� ������ ũ�⿡ ����
        float contentHeight = logText.preferredHeight;
        Vector2 size = logText.rectTransform.sizeDelta;
        logText.rectTransform.sizeDelta = new Vector2(size.x, contentHeight);

        // �ڵ� ��ũ��
        if (autoScroll && !userScrolling)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            autoScroll = false;
        }

        userScrolling = false;
    }

    /// <summary>
    /// �ܺο��� �α׸� �߰��� �� ȣ��
    /// </summary>
    public void Log(string message)
    {
        messageQueue.Enqueue(message);
        if (!isTyping)
            StartCoroutine(ProcessQueue());
    }

    /// <summary>
    /// �޽��� ť�� ���������� ��� (Ÿ���� ȿ�� ����)
    /// </summary>
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

            // ���� �α� ���
            for (int i = start; i < currentLine - 1; i++)
                sb.AppendLine(lines[i % maxLines]);

            logText.text = sb.ToString();

            // �� �޽����� �� ���ھ� ���
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

    /// <summary>
    /// ��ɾ� �Է� ó��
    /// </summary>
    private void OnInputSubmitted(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;

        Log("��ɾ� �Է�: " + command);

        command = command.Trim().ToLower();

        // ��ɾ� ó��
        if (command.StartsWith("scan "))
        {
            string fileName = command.Substring(5).Trim();
            OnScanCommandEntered?.Invoke(fileName);
        }
        else if (command == "help")
        {
            Log("��� ������ ��ɾ�: scan [���ϸ�], help, clear");
        }
        else if (command == "clear")
        {
            ClearLog();
        }
        else
        {
            Log("�߸��� ��ɾ� �Էµ�.");
        }

        inputField.text = "";
        inputField.ActivateInputField();
    }

    /// <summary>
    /// �α� â ��ü �ʱ�ȭ
    /// </summary>
    public void ClearLog()
    {
        currentLine = 0;
        sb.Clear();
        logText.text = "";
        autoScroll = true;
    }
}
