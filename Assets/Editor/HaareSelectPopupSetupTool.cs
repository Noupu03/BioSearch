using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

/// <summary>
/// SelectPopupSample 1/2.prefab(같은 SelectPopup 컴포넌트, 질문 텍스트만 다름)을
/// 데이터 기반(SelectPopupData)의 단일 Addressable 프리팹(SelectPopup.prefab)으로 정리한다.
/// git으로 추적되므로, 결과가 마음에 안 들면 커밋 전에 되돌릴 수 있다.
/// </summary>
public static class HaareSelectPopupSetupTool
{
    private const string SourcePrefabPath   = "Assets/Prefabs/UIObject/SelectPopupSample 1.prefab";
    private const string TargetPrefabPath   = "Assets/Prefabs/UIObject/SelectPopup.prefab";
    private const string ObsoletePrefabPath = "Assets/Prefabs/UIObject/SelectPopupSample 2.prefab";
    private const string AddressableGroupName = "BioSearch";
    private const string AddressableAddress   = "SelectPopup";

    [MenuItem("Tools/Haare SelectPopup 셋업", priority = 11)]
    public static void SetupSelectPopup()
    {
        var log = new StringBuilder("=== Haare SelectPopup 셋업 ===\n\n");

        if (AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath) == null)
        {
            if (!MigratePrefab(log))
            {
                Debug.Log("[Haare SelectPopup 셋업]\n" + log);
                EditorUtility.DisplayDialog("Haare SelectPopup 셋업", log.ToString(), "확인");
                return;
            }
        }
        else
        {
            log.AppendLine($"✓ {TargetPrefabPath}: 기존 유지");
        }

        RegisterAddressable(log);

        Debug.Log("[Haare SelectPopup 셋업]\n" + log);
        EditorUtility.DisplayDialog("Haare SelectPopup 셋업", log.ToString(), "확인");
    }

    private static bool MigratePrefab(StringBuilder log)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath) == null)
        {
            log.AppendLine($"⚠ 원본 프리팹을 찾을 수 없음: {SourcePrefabPath}");
            return false;
        }

        var contents = PrefabUtility.LoadPrefabContents(SourcePrefabPath);
        var popup = contents.GetComponent<SelectPopup>();
        if (popup == null)
        {
            log.AppendLine("⚠ SelectPopup 컴포넌트를 찾을 수 없음");
            PrefabUtility.UnloadPrefabContents(contents);
            return false;
        }

        // 질문 텍스트로 쓰일 TMP_Text 자동 탐색: 버튼 라벨(Yes/No/Select)보다 훨씬 긴 텍스트를 가진 것
        TMP_Text questionText = null;
        int longest = 0;
        foreach (var t in contents.GetComponentsInChildren<TMP_Text>(true))
        {
            if (t.text.Length > longest)
            {
                longest = t.text.Length;
                questionText = t;
            }
        }

        if (questionText == null)
        {
            log.AppendLine("⚠ 질문 텍스트로 쓸 TMP_Text를 찾지 못함 — 수동 연결 필요");
        }
        else
        {
            var so = new SerializedObject(popup);
            so.FindProperty("questionText").objectReferenceValue = questionText;
            so.ApplyModifiedPropertiesWithoutUndo();
            log.AppendLine($"✓ questionText → '{questionText.gameObject.name}' 연결 (기존 텍스트: \"{questionText.text}\")");
        }

        PrefabUtility.SaveAsPrefabAsset(contents, TargetPrefabPath);
        PrefabUtility.UnloadPrefabContents(contents);
        log.AppendLine($"✓ {TargetPrefabPath} 생성");

        if (AssetDatabase.LoadAssetAtPath<GameObject>(ObsoletePrefabPath) != null)
        {
            AssetDatabase.DeleteAsset(ObsoletePrefabPath);
            log.AppendLine($"✓ {ObsoletePrefabPath} 삭제 (SelectPopup.prefab으로 통합됨 — git으로 복원 가능)");
        }

        return true;
    }

    private static void RegisterAddressable(StringBuilder log)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            log.AppendLine("⚠ AddressableAssetSettings 없음 — 먼저 'Tools/Haare Addressables 셋업' 실행 필요");
            return;
        }

        var group = settings.FindGroup(AddressableGroupName) ?? settings.DefaultGroup;
        var guid  = AssetDatabase.AssetPathToGUID(TargetPrefabPath);
        if (string.IsNullOrEmpty(guid))
        {
            log.AppendLine($"⚠ 프리팹을 찾을 수 없어 Addressable 등록 실패: {TargetPrefabPath}");
            return;
        }

        var entry = settings.CreateOrMoveEntry(guid, group, false, false);
        if (entry.address != AddressableAddress)
        {
            entry.SetAddress(AddressableAddress, false);
            log.AppendLine($"✓ {TargetPrefabPath} → 주소 '{AddressableAddress}' 등록");
        }
        else
        {
            log.AppendLine($"✓ 이미 주소 '{AddressableAddress}'로 등록됨");
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true, true);
        AssetDatabase.SaveAssets();
    }
}
