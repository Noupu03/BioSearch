using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Tools 메뉴에서 씬 설정을 자동화하는 에디터 툴.
///
/// - 씬 셋업  : 매니저 간 직렬화 참조를 자동 연결
/// - 씬 정리  : 오브젝트를 그룹 폴더로 정리 + 이름 정정
/// </summary>
public static class SceneSetupTool
{
    // ──────────────────────────────────────────────
    //  Body Buttons 기본 경로 (FileWindow.bodyButtons)
    // ──────────────────────────────────────────────
    private static readonly string[] DefaultBodyButtonPaths =
    {
        "Head",
        "Body/Chest",      "Body/Pelvis",
        "LeftArm/UpperArm","LeftArm/ForeArm","LeftArm/Hand",
        "RightArm/UpperArm","RightArm/ForeArm","RightArm/Hand",
        "Organ/Stomach",
        "LeftLeg/Thigh","LeftLeg/Calf","LeftLeg/Foot",
        "RightLeg/Thigh","RightLeg/Calf","RightLeg/Foot",
    };

    // ──────────────────────────────────────────────
    //  씬 셋업
    // ──────────────────────────────────────────────
    [MenuItem("Tools/씬 셋업", priority = 1)]
    public static void SetupScene()
    {
        var log = new StringBuilder("=== 씬 셋업 ===\n\n");

        // 참조 탐색 (GameEvents 기반으로 바뀐 뒤에도
        //  SelectPopupManager 직렬화 필드는 여전히 필요)
        var selectPopup = Object.FindObjectOfType<SelectPopupManager>();
        var sanity      = Object.FindObjectOfType<SanityManager>();
        var logWindow   = Object.FindObjectOfType<LogWindowManager>();
        var fileWindow  = Object.FindObjectOfType<FileWindow>();

        if (selectPopup != null)
        {
            var so = new SerializedObject(selectPopup);
            SetRef(so, "sanityManager", sanity,    "SelectPopupManager.sanityManager", log);
            SetRef(so, "logWindow",     logWindow, "SelectPopupManager.logWindow",     log);
            SetRef(so, "fileWindow",    fileWindow,"SelectPopupManager.fileWindow",     log);
            so.ApplyModifiedProperties();
        }
        else log.AppendLine("⚠ SelectPopupManager 없음");

        if (fileWindow != null)
            SetupBodyButtons(fileWindow, log);
        else
            log.AppendLine("⚠ FileWindow 없음");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[씬 셋업]\n" + log);
        EditorUtility.DisplayDialog("씬 셋업 완료", log.ToString(), "확인");
    }

    // ──────────────────────────────────────────────
    //  씬 정리 (계층구조 그룹화 + 이름 정정)
    // ──────────────────────────────────────────────
    private static readonly Dictionary<string, string[]> SceneGroups = new Dictionary<string, string[]>
    {
        ["[Managers]"]    = new[] { "GameManagers","DripRevealManager","InputManager",
                                    "GlobalColorManager","AudioHub","BGM","CameraManager" },
        ["[Cameras]"]     = new[] { "Main Camera","Room Camera","MonitorCamera","View" },
        ["[Lighting]"]    = new[] { "Directional Light","Spot Light","Light","Global Volume" },
        ["[Environment]"] = new[] { "door","enemy","00" },
        ["[EscapeRoute]"] = new[] { "point" },
        ["[UI]"]          = new[] { "EventSystem","HUD_Canvas","Canvas" },
    };

    private static readonly Dictionary<string, string> Renames = new Dictionary<string, string>
    {
        ["on-off Button "]   = "MonitorButton",
        ["on-off Button"]    = "MonitorButton",
        ["Time_Canvas"]      = "HUD_Canvas",
        ["Global Color"]     = "GlobalColorManager",
        ["SoundManager"]     = "AudioHub",
        ["GameOver_Manager"] = "GameManagers",
    };

    [MenuItem("Tools/씬 정리", priority = 2)]
    public static void OrganizeScene()
    {
        Undo.SetCurrentGroupName("씬 정리");
        int group = Undo.GetCurrentGroup();
        var log   = new StringBuilder("=== 씬 정리 ===\n\n");

        // 이름 변경
        log.AppendLine("[ 이름 변경 ]");
        foreach (var kv in Renames)
        {
            var go = GameObject.Find(kv.Key);
            if (go == null) continue;
            Undo.RecordObject(go, "Rename");
            log.AppendLine($"  {go.name} → {kv.Value}");
            go.name = kv.Value;
        }

        // MonitorButton은 루트에 단독 배치
        MoveToRoot("MonitorButton", log);

        // 그룹화
        log.AppendLine("\n[ 그룹 구성 ]");
        foreach (var kv in SceneGroups)
        {
            var members = new List<GameObject>();
            foreach (var name in kv.Value)
            {
                var go = GameObject.Find(name);
                if (go != null) members.Add(go);
            }
            if (members.Count == 0) continue;

            var group2 = GameObject.Find(kv.Key)
                ?? CreateGroup(kv.Key);

            log.AppendLine($"  {kv.Key} ({members.Count}개):");
            foreach (var m in members)
            {
                Undo.SetTransformParent(m.transform, group2.transform, "Group");
                log.AppendLine($"    ← {m.name}");
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(group);

        string msg = log.ToString();
        Debug.Log("[씬 정리]\n" + msg);
        EditorUtility.DisplayDialog("씬 정리 완료", msg + "\nCtrl+Z로 전체 취소 가능", "확인");
    }

    [MenuItem("Tools/씬 셋업 + 정리 (전체)", priority = 3)]
    public static void SetupAndOrganize() { SetupScene(); OrganizeScene(); }

    // ──────────────────────────────────────────────
    //  내부 헬퍼
    // ──────────────────────────────────────────────

    private static void SetupBodyButtons(FileWindow fileWindow, StringBuilder log)
    {
        var so       = new SerializedObject(fileWindow);
        var listProp = so.FindProperty("bodyButtons");
        if (listProp == null) { log.AppendLine("⚠ bodyButtons 프로퍼티 없음"); return; }

        if (listProp.arraySize == 0)
        {
            listProp.arraySize = DefaultBodyButtonPaths.Length;
            for (int i = 0; i < DefaultBodyButtonPaths.Length; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("folderPath").stringValue       = DefaultBodyButtonPaths[i];
                elem.FindPropertyRelative("button").objectReferenceValue  = null;
            }
            so.ApplyModifiedProperties();
            log.AppendLine($"✓ FileWindow.bodyButtons: {DefaultBodyButtonPaths.Length}개 경로 초기화");

            // 이름 자동 매칭 시도
            int matched = TryAutoFindButtons(fileWindow);
            if (matched > 0) log.AppendLine($"  → {matched}개 버튼 이름 자동 연결");
            else             log.AppendLine("  → 버튼은 인스펙터에서 직접 할당해주세요");
        }
        else
        {
            log.AppendLine($"✓ FileWindow.bodyButtons: 기존 {listProp.arraySize}개 유지");
        }
    }

    private static int TryAutoFindButtons(FileWindow fileWindow)
    {
        var allButtons = Object.FindObjectsOfType<Button>();
        if (allButtons.Length == 0) return 0;

        var so       = new SerializedObject(fileWindow);
        var listProp = so.FindProperty("bodyButtons");
        int filled   = 0;

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var elem = listProp.GetArrayElementAtIndex(i);
            if (elem.FindPropertyRelative("button").objectReferenceValue != null) continue;

            string path    = elem.FindPropertyRelative("folderPath").stringValue;
            string keyword = path.Contains("/") ? path.Substring(path.LastIndexOf('/') + 1) : path;
            string lower   = keyword.ToLower();

            foreach (var btn in allButtons)
            {
                string n = btn.gameObject.name.ToLower().Replace(" ", "").Replace("_", "");
                if (n.Contains(lower))
                {
                    elem.FindPropertyRelative("button").objectReferenceValue = btn;
                    filled++;
                    break;
                }
            }
        }

        if (filled > 0) so.ApplyModifiedProperties();
        return filled;
    }

    private static GameObject CreateGroup(string name)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create Group");
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        return go;
    }

    private static void MoveToRoot(string name, StringBuilder log)
    {
        var go = GameObject.Find(name);
        if (go != null && go.transform.parent != null)
        {
            Undo.SetTransformParent(go.transform, null, "To Root");
            log.AppendLine($"  {name} → 루트 이동");
        }
    }

    private static void SetRef(SerializedObject so, string prop, Object val, string label, StringBuilder log)
    {
        if (val == null) { log.AppendLine($"  ⚠ {label}: 대상 없음"); return; }
        var p = so.FindProperty(prop);
        if (p == null) { log.AppendLine($"  ⚠ {label}: 프로퍼티 없음"); return; }
        p.objectReferenceValue = val;
        log.AppendLine($"  ✓ {label} → {val.name}");
    }
}
