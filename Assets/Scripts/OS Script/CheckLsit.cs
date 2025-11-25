using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// CheckList: 이제 메시지 기반 체크리스트를 관리합니다.
/// - AddCheckFromMessage(messageKey, text, linkedFileName)
/// - MarkCompleteByFileName(fileName)
/// </summary>
public class CheckList : MonoBehaviour
{
    public TMP_Text checkListText;
    private List<CheckItem> items = new List<CheckItem>();

    public static CheckList Instance;

    private void Awake()
    {
        Instance = this;
    }

    // 내부 매핑: messageKey -> index in items
    private Dictionary<string, int> messageKeyToIndex = new Dictionary<string, int>();

    // ============================================
    // 메시지 기반으로 체크리스트 추가
    // (openChat에서 호출되어 체크리스트를 UI에 표시)
    // ============================================
    public void AddCheckFromMessage(string messageKey, string itemText, string linkedFileName)
    {
        if (string.IsNullOrEmpty(itemText)) return;
        if (messageKeyToIndex.ContainsKey(messageKey))
        {
            // 이미 추가됨
            Debug.Log($"[CheckList] Already added check for message {messageKey}");
            return;
        }

        CheckItem ci = new CheckItem
        {
            messageKey = messageKey,
            text = itemText,
            linkedFileName = linkedFileName,
            completed = false
        };

        items.Add(ci);
        messageKeyToIndex[messageKey] = items.Count - 1;
        UpdateText();

        Debug.Log($"[CheckList] Added check: {itemText} (linked: {linkedFileName})");
    }

    // ============================================
    // 파일을 열었을 때 해당 파일에 매핑된 체크리스트를 완료 처리
    // ============================================
    public void MarkCompleteByFileName(string fileName)
    {
        bool anyChanged = false;
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (!it.completed && !string.IsNullOrEmpty(it.linkedFileName))
            {
                if (string.Equals(it.linkedFileName, fileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    items[i].completed = true;
                    anyChanged = true;
                    Debug.Log($"[CheckList] Marked completed: {it.text} for file {fileName}");
                }
            }
        }

        if (anyChanged)
        {
            UpdateText();
        }

        // 또한 MessengerDataManager에도 완료 표시를 남김
        if (MessengerDataManager.Instance != null)
            MessengerDataManager.Instance.MarkMappingCompletedByFile(fileName);
    }

    // ============================================
    // 단일 항목에 취소선 적용(외부 호출용)
    // ============================================
    public void StrikeCheckList(string text)
    {
        // 기존 동작을 유지하려면 텍스트 매칭으로 처리
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].text == text)
            {
                items[i].completed = true;
            }
        }
        UpdateText();
    }

    private void UpdateText()
    {
        if (checkListText == null) return;

        checkListText.richText = true;
        List<string> lines = new List<string>();
        foreach (var it in items)
        {
            if (it.completed)
                lines.Add($"<s>{it.text}</s>");
            else
                lines.Add(it.text);
        }

        checkListText.text = string.Join("\n", lines);
        checkListText.ForceMeshUpdate();
    }

    class CheckItem
    {
        public string messageKey;
        public string text;
        public string linkedFileName;
        public bool completed;
    }

    // ============================================
    // ★ 체크리스트 전체 초기화
    // MessageScheduler → InitializeAllStates()에서 호출됨
    // ============================================
    public void ResetCheckListAndText()
    {
        // 모든 체크 항목 초기화
        items.Clear();
        messageKeyToIndex.Clear();

        // UI 텍스트 초기화
        if (checkListText != null)
        {
            checkListText.text = "";
            checkListText.ForceMeshUpdate();
        }

        Debug.Log("[CheckList] 체크리스트 전체 초기화 완료");
    }

}
