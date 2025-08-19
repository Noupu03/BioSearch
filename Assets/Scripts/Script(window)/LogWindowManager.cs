using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class LogWindowManager : MonoBehaviour
{
    public static LogWindowManager Instance;

    [Header("UI References")]
    public TMP_Text logText;           // ScrollRect Content�� �ٷ� ����
    public TMP_InputField inputField;
    public ScrollRect scrollRect;

    [Header("Settings")]
    public int maxLines = 50;

    private string[] lines;
    private int currentLine = 0;
    private StringBuilder sb;

    private bool autoScroll = false;
    private bool userScrolling = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        lines = new string[maxLines];
        sb = new StringBuilder();

        logText.text = "";
        inputField.onSubmit.AddListener(OnCommandEntered);
        inputField.ActivateInputField();

        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();

        // Elastic Scroll
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.inertia = true;

        // TMP_Text�� ScrollRect Content�� ����
        scrollRect.content = logText.rectTransform;

        // ��ũ�� �� �÷���
        scrollRect.onValueChanged.AddListener(_ => userScrolling = true);

        // TMP_Text ����: �Ʒ��� ����
        logText.rectTransform.pivot = new Vector2(0, 0);          // Pivot �Ʒ���
        logText.rectTransform.anchorMin = new Vector2(0, 0);       // Bottom Left
        logText.rectTransform.anchorMax = new Vector2(1, 0);       // Bottom Right Stretch
    }

    private void LateUpdate()
    {
        // TMP_Text ���� �ݿ�
        float contentHeight = logText.preferredHeight;
        Vector2 size = logText.rectTransform.sizeDelta;
        logText.rectTransform.sizeDelta = new Vector2(size.x, contentHeight);

        // �� �α� �Է� �� �ڵ� ��ũ��
        if (autoScroll && !userScrolling)
        {
            if (contentHeight > scrollRect.viewport.rect.height)
                scrollRect.verticalNormalizedPosition = 0f; // �� �Ʒ�
            else
                scrollRect.verticalNormalizedPosition = 0f; // content�� ª�Ƶ� �Ʒ��� ǥ��

            autoScroll = false;
        }

        userScrolling = false;
    }

    public void Log(string message)
    {
        lines[currentLine % maxLines] = "> " + message;
        currentLine++;

        sb.Clear();
        int start = Mathf.Max(0, currentLine - maxLines);

        // �Ʒ����� ���� ���̵��� ���� ����
        for (int i = start; i < currentLine; i++)
        {
            sb.AppendLine(lines[i % maxLines]);
        }

        logText.text = sb.ToString();

        // �� �α� �߻� �� �ڵ� ��ũ��
        autoScroll = true;
    }

    private void OnCommandEntered(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;

        Log("��ɾ� �Է�: " + command);

        switch (command.ToLower())
        {
            case "help":
                Log("��� ������ ��ɾ�: help, hello, clear");
                break;
            case "hello":
                Log("Hello, Commander!");
                break;
            case "clear":
                ClearLog();
                break;
            default:
                Log("�� �� ���� ��ɾ�: " + command);
                break;
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
