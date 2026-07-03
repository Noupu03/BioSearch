using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

/// <summary>
/// Haare SceneService/Panel 시스템이 필요로 하는 Addressable 등록을 자동화한다.
/// StartScene/scene ssh를 각각 고정 주소("StartScene"/"SceneSSH")로 Addressable 그룹에 등록한다.
/// 파일명을 그대로 주소로 쓰지 않는 이유: "scene ssh"는 공백이 있어 SceneService의 enum 매핑과
/// 어긋나므로, 항상 명시적인 주소 문자열을 지정한다.
/// </summary>
public static class HaareAddressablesSetupTool
{
    private const string GroupName = "BioSearch";

    private static readonly (string ScenePath, string Address)[] Scenes =
    {
        ("Assets/Scenes/StartScene.unity", "StartScene"),
        ("Assets/Scenes/scene ssh.unity",  "SceneSSH"),
    };

    [MenuItem("Tools/Haare Addressables 셋업", priority = 10)]
    public static void SetupAddressables()
    {
        var log = new StringBuilder("=== Haare Addressables 셋업 ===\n\n");

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            log.AppendLine("✓ AddressableAssetSettings 신규 생성");
        }

        var group = settings.FindGroup(GroupName);
        if (group == null)
        {
            group = settings.CreateGroup(GroupName, false, false, true, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            log.AppendLine($"✓ '{GroupName}' 그룹 생성");
        }
        else
        {
            log.AppendLine($"✓ '{GroupName}' 그룹 기존 유지");
        }

        foreach (var (scenePath, address) in Scenes)
        {
            var guid = AssetDatabase.AssetPathToGUID(scenePath);
            if (string.IsNullOrEmpty(guid))
            {
                log.AppendLine($"⚠ 씬을 찾을 수 없음: {scenePath}");
                continue;
            }

            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            if (entry.address != address)
            {
                entry.SetAddress(address, false);
                log.AppendLine($"✓ {scenePath} → 주소 '{address}' 설정");
            }
            else
            {
                log.AppendLine($"✓ {scenePath}: 이미 주소 '{address}'로 등록됨");
            }
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true, true);
        AssetDatabase.SaveAssets();

        Debug.Log("[Haare Addressables 셋업]\n" + log);
        EditorUtility.DisplayDialog("Haare Addressables 셋업", log.ToString(), "확인");
    }
}
