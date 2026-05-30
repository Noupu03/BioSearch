using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public static class SceneSetupTool
{
    // ──────────────────────────────────────────────
    //  씬 셋업: 매니저 참조 자동 연결
    // ──────────────────────────────────────────────

    private static readonly string[] DefaultBodyButtonPaths = new string[]
    {
        "Head",
        "Body/Chest",
        "LeftArm/UpperArm",
        "LeftArm/ForeArm",
        "LeftArm/Hand",
        "RightArm/UpperArm",
        "RightArm/ForeArm",
        "RightArm/Hand",
        "Organ/Stomach",
        "Body/Pelvis",
        "LeftLeg/Thigh",
        "LeftLeg/Calf",
        "LeftLeg/Foot",
        "RightLeg/Thigh",
        "RightLeg/Calf",
        "RightLeg/Foot"
    };

    [MenuItem("Tools/씬 셋업", priority = 1)]
    public static void SetupScene()
    {
        var log = new StringBuilder();
        log.AppendLine("=== 씬 셋업 결과 ===\n");

        var gameOverManager    = Object.FindObjectOfType<GameOverManager>();
        var sanityManager      = Object.FindObjectOfType<SanityManager>();
        var timerManager       = Object.FindObjectOfType<TimerManager>();
        var sceneStartManager  = Object.FindObjectOfType<SceneStartManager>();
        var selectPopupManager = Object.FindObjectOfType<SelectPopupManager>();
        var logWindowManager   = Object.FindObjectOfType<LogWindowManager>();
        var fileWindow         = Object.FindObjectOfType<FileWindow>();

        Wire(timerManager,       "gameOverManager", gameOverManager, "TimerManager",      log);
        Wire(sanityManager,      "gameOverManager", gameOverManager, "SanityManager",     log);
        Wire(gameOverManager,    "sanityManager",   sanityManager,   "GameOverManager",   log);

        if (sceneStartManager != null)
        {
            var so = new SerializedObject(sceneStartManager);
            SetRef(so, "timer",    timerManager,    "SceneStartManager.timer",    log);
            SetRef(so, "sanity",   sanityManager,   "SceneStartManager.sanity",   log);
            SetRef(so, "gameOver", gameOverManager, "SceneStartManager.gameOver", log);
            so.ApplyModifiedProperties();
        }
        else log.AppendLine("⚠ SceneStartManager 없음 — 스킵");

        if (selectPopupManager != null)
        {
            var so = new SerializedObject(selectPopupManager);
            SetRef(so, "gameOverManager", gameOverManager,  "SelectPopupManager.gameOverManager", log);
            SetRef(so, "sanityManager",   sanityManager,    "SelectPopupManager.sanityManager",   log);
            SetRef(so, "logWindow",       logWindowManager, "SelectPopupManager.logWindow",       log);
            SetRef(so, "fileWindow",      fileWindow,       "SelectPopupManager.fileWindow",       log);
            so.ApplyModifiedProperties();
        }
        else log.AppendLine("⚠ SelectPopupManager 없음 — 스킵");

        if (fileWindow != null)
            SetupBodyButtons(fileWindow, log);
        else
            log.AppendLine("⚠ FileWindow 없음 — body buttons 스킵");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        string summary = log.ToString();
        Debug.Log("[씬 셋업]\n" + summary);
        EditorUtility.DisplayDialog("씬 셋업 완료", summary, "확인");
    }

    // ──────────────────────────────────────────────
    //  씬 정리: 계층구조 그룹화
    // ──────────────────────────────────────────────

    // 그룹 이름 → 그룹에 넣을 GameObject 이름 목록
    private static readonly Dictionary<string, string[]> SceneGroups = new Dictionary<string, string[]>
    {
        ["[Managers]"]    = new[] { "GameOver_Manager", "DripRevealManager", "InputManager",
                                    "Global Color", "SoundManager", "BGM", "CameraManager" },
        ["[Cameras]"]     = new[] { "Main Camera", "Room Camera", "MonitorCamera", "View" },
        ["[Lighting]"]    = new[] { "Directional Light", "Spot Light", "Light", "Global Volume" },
        ["[Environment]"] = new[] { "door", "enemy", "00" },
        ["[EscapeRoute]"] = new[] { "point" },
        ["[UI]"]          = new[] { "EventSystem", "Time_Canvas", "Canvas" },
    };

    // 이름 정정 목록 (현재 이름 → 새 이름)
    private static readonly Dictionary<string, string> Renames = new Dictionary<string, string>
    {
        ["on-off Button "]  = "MonitorButton",
        ["on-off Button"]   = "MonitorButton",
        ["Time_Canvas"]     = "HUD_Canvas",
        ["Global Color"]    = "GlobalColorManager",
        ["SoundManager"]    = "AudioHub",
        ["GameOver_Manager"]= "GameManagers",
    };

    [MenuItem("Tools/씬 정리", priority = 2)]
    public static void OrganizeScene()
    {
        Undo.SetCurrentGroupName("씬 정리");
        int undoGroup = Undo.GetCurrentGroup();

        var log = new StringBuilder();
        log.AppendLine("=== 씬 정리 결과 ===\n");

        // 1. 이름 정정
        log.AppendLine("[ 이름 변경 ]");
        foreach (var kv in Renames)
        {
            var go = GameObject.Find(kv.Key);
            if (go != null)
            {
                Undo.RecordObject(go, "Rename");
                string old = go.name;
                go.name = kv.Value;
                log.AppendLine($"  {old} → {kv.Value}");
            }
        }

        // 2. MonitorButton 는 그룹 밖(루트)에 단독 배치
        EnsureRoot("MonitorButton", log);

        // 3. 그룹 생성 및 오브젝트 이동
        log.AppendLine("\n[ 그룹 구성 ]");
        foreach (var kv in SceneGroups)
        {
            string groupName = kv.Key;
            string[] members = kv.Value;

            // 실제로 씬에 있는 멤버만 확인
            var found = new List<GameObject>();
            foreach (var name in members)
            {
                // 이름 변경 후 이름으로 재탐색
                string resolved = Renames.ContainsValue(name) ? name : name;
                var go = GameObject.Find(name);
                if (go == null)
                {
                    // Renames로 변경됐을 수 있으니 역탐색
                    foreach (var r in Renames)
                        if (r.Key == name) { go = GameObject.Find(r.Value); break; }
                }
                if (go != null) found.Add(go);
            }

            if (found.Count == 0)
            {
                log.AppendLine($"  {groupName}: 해당 오브젝트 없음 — 건너뜀");
                continue;
            }

            // 그룹 오브젝트 찾거나 생성
            var group = GameObject.Find(groupName);
            if (group == null)
            {
                group = new GameObject(groupName);
                Undo.RegisterCreatedObjectUndo(group, "Create Group");
                group.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            log.AppendLine($"  {groupName} ({found.Count}개):");
            foreach (var member in found)
            {
                Undo.SetTransformParent(member.transform, group.transform, "Group");
                log.AppendLine($"    ← {member.name}");
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(undoGroup);

        string summary = log.ToString();
        Debug.Log("[씬 정리]\n" + summary);
        EditorUtility.DisplayDialog("씬 정리 완료", summary + "\n\nCtrl+Z 로 전체 취소 가능합니다.", "확인");
    }

    // ──────────────────────────────────────────────
    //  씬 셋업 + 정리 한번에
    // ──────────────────────────────────────────────

    [MenuItem("Tools/씬 셋업 + 정리 (전체)", priority = 3)]
    public static void SetupAndOrganize()
    {
        SetupScene();
        OrganizeScene();
    }

    // ──────────────────────────────────────────────
    //  내부 헬퍼
    // ──────────────────────────────────────────────

    private static void EnsureRoot(string name, StringBuilder log)
    {
        var go = GameObject.Find(name);
        if (go != null && go.transform.parent != null)
        {
            Undo.SetTransformParent(go.transform, null, "To Root");
            log.AppendLine($"  {name} → 루트로 이동 (그룹 제외)");
        }
    }

    private static void SetupBodyButtons(FileWindow fileWindow, StringBuilder log)
    {
        var so = new SerializedObject(fileWindow);
        var listProp = so.FindProperty("bodyButtons");
        if (listProp == null) { log.AppendLine("⚠ bodyButtons 프로퍼티 없음"); return; }

        bool wasEmpty = listProp.arraySize == 0;
        if (wasEmpty)
        {
            listProp.arraySize = DefaultBodyButtonPaths.Length;
            for (int i = 0; i < DefaultBodyButtonPaths.Length; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("folderPath").stringValue = DefaultBodyButtonPaths[i];
                elem.FindPropertyRelative("button").objectReferenceValue = null;
            }
            so.ApplyModifiedProperties();
            log.AppendLine($"✓ FileWindow.bodyButtons: {DefaultBodyButtonPaths.Length}개 경로 초기화 완료");
            log.AppendLine("  → 인스펙터에서 각 항목의 Button을 직접 할당해주세요.");
        }
        else
        {
            log.AppendLine($"✓ FileWindow.bodyButtons: 기존 {listProp.arraySize}개 유지");
        }

        TryAutoFindButtons(fileWindow, log);
    }

    private static void TryAutoFindButtons(FileWindow fileWindow, StringBuilder log)
    {
        var allButtons = Object.FindObjectsOfType<Button>();
        if (allButtons.Length == 0) return;

        var so = new SerializedObject(fileWindow);
        var listProp = so.FindProperty("bodyButtons");
        int autoFilled = 0;

        for (int i = 0; i < listProp.arraySize; i++)
        {
            var element = listProp.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("button").objectReferenceValue != null) continue;

            string path = element.FindPropertyRelative("folderPath").stringValue;
            string keyword = path.Contains("/") ? path.Substring(path.LastIndexOf('/') + 1) : path;

            Button found = FindButtonByKeyword(allButtons, keyword);
            if (found != null)
            {
                element.FindPropertyRelative("button").objectReferenceValue = found;
                autoFilled++;
            }
        }

        if (autoFilled > 0)
        {
            so.ApplyModifiedProperties();
            log.AppendLine($"  → 이름 매칭으로 {autoFilled}개 버튼 자동 연결");
        }
    }

    private static Button FindButtonByKeyword(Button[] buttons, string keyword)
    {
        string lower = keyword.ToLower();
        foreach (var btn in buttons)
        {
            string name = btn.gameObject.name.ToLower().Replace(" ", "").Replace("_", "");
            if (name.Contains(lower)) return btn;
        }
        return null;
    }

    private static void Wire<TSource, TTarget>(
        TSource source, string propName, TTarget target,
        string sourceName, StringBuilder log)
        where TSource : Object
        where TTarget : Object
    {
        if (source == null) { log.AppendLine($"⚠ {sourceName} 없음 — 스킵"); return; }
        if (target == null) { log.AppendLine($"⚠ {typeof(TTarget).Name} 없음 — {sourceName}.{propName} 스킵"); return; }

        var so = new SerializedObject(source);
        SetRef(so, propName, target, $"{sourceName}.{propName}", log);
        so.ApplyModifiedProperties();
    }

    private static void SetRef(SerializedObject so, string propName, Object value, string label, StringBuilder log)
    {
        if (value == null) { log.AppendLine($"  ⚠ {label}: 대상 없음"); return; }
        var prop = so.FindProperty(propName);
        if (prop == null) { log.AppendLine($"  ⚠ {label}: 프로퍼티 없음"); return; }
        prop.objectReferenceValue = value;
        log.AppendLine($"  ✓ {label} → {value.name}");
    }
}
