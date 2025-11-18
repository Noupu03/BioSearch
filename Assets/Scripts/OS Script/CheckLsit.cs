using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CheckList : MonoBehaviour
{
    public TMP_Text checkListText;
    private List<string> items = new List<string>();

    public static CheckList Instance;

    // 파일명 → 항목 리스트 매핑 저장
    private Dictionary<string, List<string>> strikeMapping
        = new Dictionary<string, List<string>>();

    private void Awake()
    {
        Instance = this;
    }

    // ============================================
    // 체크리스트 항목 추가
    // ============================================
    public void AddCheckList(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        items.Add(text);
        UpdateText();
    }

    // ============================================
    // 체크리스트 항목 취소선 적용
    // ============================================
    public void StrikeCheckList(string text)
    {
        int index = items.IndexOf(text);
        if (index >= 0)
        {
            items[index] = $"<s>{text}</s>";
            UpdateText();
        }
    }

    // ============================================
    // [중요] 파일명 + 항목 매핑 등록
    // 예: RegisterStrikeMapping("발전기녹제거", "발전기 의 녹을 제거할 방법 찾기");
    // ============================================
    public void RegisterStrikeMapping(string fileNameKey, string itemName)
    {
        if (!strikeMapping.ContainsKey(fileNameKey))
            strikeMapping[fileNameKey] = new List<string>();

        if (!strikeMapping[fileNameKey].Contains(itemName))
            strikeMapping[fileNameKey].Add(itemName);

        Debug.Log($"[CheckList] Mapping registered: {fileNameKey} → {itemName}");
    }

    // ============================================
    // 파일이 열렸을 때 해당 파일의 매핑된 항목들 취소선 처리
    // ============================================
    public void ApplyStrikeForFile(string fileNameKey)
    {
        if (!strikeMapping.ContainsKey(fileNameKey))
        {
            Debug.Log($"[CheckList] No mapping found for '{fileNameKey}'");
            return;
        }

        foreach (var item in strikeMapping[fileNameKey])
        {
            StrikeCheckList(item);
            Debug.Log($"[CheckList] Struck due to file open: {item}");
        }
    }

    private void UpdateText()
    {
        if (checkListText == null) return;

        checkListText.richText = true;
        checkListText.text = string.Join("\n", items);
        checkListText.ForceMeshUpdate();
    }
}
