using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using Haare.Client.UI;

/// <summary>
/// Tools 메뉴 → 씬 설정 자동화.
///
/// 리팩토링 후 배선이 필요한 항목:
///   1. SelectPopupManager 트리거 버튼 / 팝업 프리팹 / popupParent
///   2. FileWindow.bodyButtons 경로 초기화 (버튼은 수동 할당)
///   3. 씬 계층구조 정리 (그룹 폴더 생성 + 이름 정정)
///   4. GameLifetimeScope 생성 (Haare/VContainer DI 루트, 없으면 생성)
///
/// 싱글톤으로 전환된 시스템(FileWindow, SanityManager, LogWindowManager 등)은
/// 자동 배선이 불필요하다.
/// </summary>
public static class SceneSetupTool
{
    // ──────────────────────────────────────────────
    //  Body Buttons 기본 경로
    // ──────────────────────────────────────────────
    private static readonly string[] DefaultBodyButtonPaths =
    {
        "Head",
        "Body/Chest",       "Body/Pelvis",
        "LeftArm/UpperArm", "LeftArm/ForeArm", "LeftArm/Hand",
        "RightArm/UpperArm","RightArm/ForeArm","RightArm/Hand",
        "Organ/Stomach",
        "LeftLeg/Thigh",    "LeftLeg/Calf",    "LeftLeg/Foot",
        "RightLeg/Thigh",   "RightLeg/Calf",   "RightLeg/Foot",
    };

    // ──────────────────────────────────────────────
    //  씬 셋업
    // ──────────────────────────────────────────────
    [MenuItem("Tools/씬 셋업", priority = 1)]
    public static void SetupScene()
    {
        var log = new StringBuilder("=== 씬 셋업 ===\n\n");

        var fw = Object.FindObjectOfType<FileWindow>();
        if (fw != null) SetupBodyButtons(fw, log);
        else            log.AppendLine("⚠ FileWindow 없음");

        var glm = Object.FindObjectOfType<GameLoopManager>();
        if (glm != null) SetupGameLoopManager(glm, log);
        else             log.AppendLine("⚠ GameLoopManager 없음 — 씬에 추가 후 재실행");

        var spm = Object.FindObjectOfType<SelectPopupManager>();
        if (spm != null && glm != null) SetupSelectPopupManager(spm, glm, log);

        var hcc = Object.FindObjectOfType<HybridCameraController>(true);
        if (hcc != null) SetupHybridCameraController(hcc, log);
        else             log.AppendLine("⚠ HybridCameraController 없음");

        SetupGameLifetimeScope(log);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[씬 셋업]\n" + log);
        EditorUtility.DisplayDialog("씬 셋업", log.ToString(), "확인");
    }

    // ──────────────────────────────────────────────
    //  씬 정리
    // ──────────────────────────────────────────────
    private static readonly Dictionary<string, string[]> SceneGroups =
        new Dictionary<string, string[]>
        {
            ["[Managers]"]    = new[] { "GameManagers","DripRevealManager","InputManager",
                                        "GlobalColorManager","AudioHub","BGM","CameraManager" },
            ["[Cameras]"]     = new[] { "Main Camera","Room Camera","MonitorCamera","View" },
            ["[Lighting]"]    = new[] { "Directional Light","Spot Light","Light","Global Volume" },
            ["[Environment]"] = new[] { "door","enemy","00" },
            ["[EscapeRoute]"] = new[] { "point" },
            ["[UI]"]          = new[] { "EventSystem","HUD_Canvas","Canvas" },
        };

    private static readonly Dictionary<string, string> Renames =
        new Dictionary<string, string>
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

        log.AppendLine("[ 이름 변경 ]");
        foreach (var kv in Renames)
        {
            var go = GameObject.Find(kv.Key);
            if (go == null) continue;
            Undo.RecordObject(go, "Rename");
            log.AppendLine($"  {go.name} → {kv.Value}");
            go.name = kv.Value;
        }

        MoveToRoot("MonitorButton", log);

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

            var grp = GameObject.Find(kv.Key) ?? CreateGroup(kv.Key);
            log.AppendLine($"  {kv.Key} ({members.Count}개):");
            foreach (var m in members)
            {
                Undo.SetTransformParent(m.transform, grp.transform, "Group");
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
    private static void SetupGameLoopManager(GameLoopManager glm, StringBuilder log)
    {
        var so = new SerializedObject(glm);

        // stageResettables 배열 — 순서가 실행 순서이므로 고정
        var resettables = new List<Object>
        {
            Object.FindObjectOfType<SanityManager>(),
            Object.FindObjectOfType<FileWindow>(),
            Object.FindObjectOfType<DummyIconSpawner>(),
            Object.FindObjectOfType<LogWindowManager>(),
            Object.FindObjectOfType<GameStateManager>(),
            Object.FindObjectOfType<TimerManager>(),
        };
        resettables.RemoveAll(r => r == null);

        var arr = so.FindProperty("stageResettables");
        if (arr != null)
        {
            arr.ClearArray();
            arr.arraySize = resettables.Count;
            for (int i = 0; i < resettables.Count; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = resettables[i];
            log.AppendLine($"✓ GameLoopManager.stageResettables: {resettables.Count}개 설정");
        }

        void TryLink(string field, Object target)
        {
            if (target == null) return;
            var prop = so.FindProperty(field);
            if (prop == null || prop.objectReferenceValue != null) return;
            prop.objectReferenceValue = target;
            log.AppendLine($"  ✓ GameLoopManager.{field} → {target.name}");
        }

        TryLink("sanityManager", Object.FindObjectOfType<SanityManager>());
        TryLink("hybridCamera",  Object.FindObjectOfType<HybridCameraController>());

        so.ApplyModifiedProperties();
    }

    private static void SetupHybridCameraController(HybridCameraController hcc, StringBuilder log)
    {
        var so  = new SerializedObject(hcc);
        int hit = 0;

        // camera1/camera2는 잘못 설정돼 있을 수 있으므로 항상 덮어씀
        void ForceLink(string field, Object obj)
        {
            if (obj == null) return;
            var prop = so.FindProperty(field);
            if (prop == null) return;
            var old = prop.objectReferenceValue;
            prop.objectReferenceValue = obj;
            hit++;
            string arrow = (old != null && old != obj) ? $"{old.name} → " : "";
            log.AppendLine($"  ✓ HybridCamera.{field}: {arrow}{obj.name}");
        }

        // 선택적 연결 (이미 있으면 유지)
        void Link(string field, Object obj)
        {
            if (obj == null) return;
            var prop = so.FindProperty(field);
            if (prop == null || prop.objectReferenceValue != null) return;
            prop.objectReferenceValue = obj;
            hit++;
            log.AppendLine($"  ✓ HybridCamera.{field} → {obj.name}");
        }

        // ── camera1(방) / camera2(모니터): 항상 이름 기반으로 강제 설정 ─────
        // camera1 = 방뷰 카메라 (기본 활성), camera2 = 모니터 카메라 (W 누를 때 활성)
        Camera roomCam    = null;
        Camera monitorCam = null;
        foreach (var cam in Object.FindObjectsOfType<Camera>(true))
        {
            string n = cam.name.ToLower();
            if (n.Contains("room"))    { roomCam    = cam; }
            if (n.Contains("monitor")) { monitorCam = cam; }
        }
        if (roomCam    != null) ForceLink("camera1", roomCam);
        if (monitorCam != null) ForceLink("camera2", monitorCam);

        // 이름 매칭 실패 시 경고만
        if (roomCam    == null) log.AppendLine("  ⚠ camera1: 'room' 포함 카메라 없음 — 수동 설정");
        if (monitorCam == null) log.AppendLine("  ⚠ camera2: 'monitor' 포함 카메라 없음 — 수동 설정");

        // ── view 트랜스폼: 이름 후보로 검색 ────────────────────────────
        GameObject FindByNames(params string[] names)
        {
            foreach (var n in names)
            {
                // 씬에서 비활성 포함 전체 검색
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                    if (go.scene.IsValid() && go.name == n) return go;
            }
            return null;
        }

        var view2GO     = FindByNames("view2",    "View2",    "View",       "MonitorView",  "CamView2");
        var leftViewGO  = FindByNames("leftView", "LeftView", "Left View",  "CamLeft",  "LeftMonitorView");
        var rightViewGO = FindByNames("rightView","RightView","Right View", "CamRight", "RightMonitorView");

        if (view2GO)     Link("view2",     view2GO.transform);
        if (leftViewGO)  Link("leftView",  leftViewGO.transform);
        if (rightViewGO) Link("rightView", rightViewGO.transform);

        // ── Canvas: "Canvas" → "HUD_Canvas" 순으로 시도 ─────────────────
        var canvasGO = GameObject.Find("Canvas") ?? GameObject.Find("HUD_Canvas");
        if (canvasGO) Link("targetCanvas", canvasGO.GetComponent<Canvas>());

        // ── TMP_InputField: LogWindowManager에서 복사 ───────────────────
        var lwm = Object.FindObjectOfType<LogWindowManager>(true);
        if (lwm != null)
        {
            var lwmSO    = new SerializedObject(lwm);
            var inputRef = lwmSO.FindProperty("inputField")?.objectReferenceValue;
            if (inputRef) Link("targetInputField", inputRef);
        }

        so.ApplyModifiedProperties();
        log.AppendLine($"✓ HybridCameraController: {hit}개 연결");

        // 핵심 필드 미연결 경고
        foreach (var req in new[] { "camera1", "camera2", "view2" })
            if (so.FindProperty(req)?.objectReferenceValue == null)
                log.AppendLine($"  ⚠ {req} 미연결 — 수동 설정 필요");
    }

    private const string CoreUIManagerPrefabPath = "Assets/Prefabs/Haare/CoreUIManager.prefab";

    private static void SetupGameLifetimeScope(StringBuilder log)
    {
        var scope = Object.FindObjectOfType<GameLifetimeScope>(true);
        if (scope == null)
        {
            var go = new GameObject("GameLifetimeScope");
            Undo.RegisterCreatedObjectUndo(go, "Create GameLifetimeScope");
            scope = go.AddComponent<GameLifetimeScope>();
            log.AppendLine("✓ GameLifetimeScope 생성 (VContainer 루트 DI 스코프)");
        }
        else
        {
            log.AppendLine("✓ GameLifetimeScope: 기존 유지");
        }

        SetupCoreUIManagerPrefab(scope, log);
    }

    private static void SetupCoreUIManagerPrefab(GameLifetimeScope scope, StringBuilder log)
    {
        var so   = new SerializedObject(scope);
        var prop = so.FindProperty("coreUIManagerPrefab");
        if (prop == null)
        {
            log.AppendLine("⚠ GameLifetimeScope.coreUIManagerPrefab 필드 없음");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<CoreUIManager>(CoreUIManagerPrefabPath);
        if (prefab == null)
        {
            prefab = CreateCoreUIManagerPrefab(log);
        }
        else
        {
            log.AppendLine("✓ CoreUIManager 프리팹: 기존 유지");
        }

        if (prop.objectReferenceValue == null && prefab != null)
        {
            prop.objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            log.AppendLine("✓ GameLifetimeScope.coreUIManagerPrefab 연결");
        }
    }

    private static CoreUIManager CreateCoreUIManagerPrefab(StringBuilder log)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Haare"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Haare");

        var root = new GameObject("CoreUIManager", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 팝업이 다른 UI 위에 뜨도록

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var panelRootGO = new GameObject("PanelRoot", typeof(RectTransform));
        panelRootGO.transform.SetParent(root.transform, false);
        var panelRootRect = (RectTransform)panelRootGO.transform;
        panelRootRect.anchorMin = Vector2.zero;
        panelRootRect.anchorMax = Vector2.one;
        panelRootRect.offsetMin = Vector2.zero;
        panelRootRect.offsetMax = Vector2.zero;

        var coreUIManager = root.AddComponent<CoreUIManager>();
        var cuiSo = new SerializedObject(coreUIManager);
        cuiSo.FindProperty("safePannelRect").objectReferenceValue = panelRootRect;
        cuiSo.ApplyModifiedPropertiesWithoutUndo();

        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, CoreUIManagerPrefabPath);
        Object.DestroyImmediate(root);

        log.AppendLine($"✓ CoreUIManager 프리팹 생성: {CoreUIManagerPrefabPath}");
        return savedPrefab.GetComponent<CoreUIManager>();
    }

    private static void SetupSelectPopupManager(SelectPopupManager spm, GameLoopManager glm, StringBuilder log)
    {
        var so   = new SerializedObject(spm);
        var prop = so.FindProperty("gameLoopManager");
        if (prop != null && prop.objectReferenceValue == null)
        {
            prop.objectReferenceValue = glm;
            so.ApplyModifiedProperties();
            log.AppendLine("✓ SelectPopupManager.gameLoopManager 연결");
        }
    }

    private static void SetupBodyButtons(FileWindow fw, StringBuilder log)
    {
        var so   = new SerializedObject(fw);
        var list = so.FindProperty("bodyButtons");
        if (list == null) { log.AppendLine("⚠ bodyButtons 프로퍼티 없음"); return; }

        if (list.arraySize == 0)
        {
            list.arraySize = DefaultBodyButtonPaths.Length;
            for (int i = 0; i < DefaultBodyButtonPaths.Length; i++)
            {
                var elem = list.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("folderPath").stringValue      = DefaultBodyButtonPaths[i];
                elem.FindPropertyRelative("button").objectReferenceValue = null;
            }
            so.ApplyModifiedProperties();
            log.AppendLine($"✓ FileWindow.bodyButtons: {DefaultBodyButtonPaths.Length}개 경로 초기화");

            int matched = TryAutoMatchButtons(fw);
            log.AppendLine(matched > 0
                ? $"  → {matched}개 버튼 이름 자동 연결"
                : "  → 인스펙터에서 Button 직접 할당 필요");
        }
        else
        {
            log.AppendLine($"✓ FileWindow.bodyButtons: 기존 {list.arraySize}개 유지");
        }
    }

    private static int TryAutoMatchButtons(FileWindow fw)
    {
        var allBtns = Object.FindObjectsOfType<Button>();
        if (allBtns.Length == 0) return 0;

        var so   = new SerializedObject(fw);
        var list = so.FindProperty("bodyButtons");
        int n    = 0;

        for (int i = 0; i < list.arraySize; i++)
        {
            var elem = list.GetArrayElementAtIndex(i);
            if (elem.FindPropertyRelative("button").objectReferenceValue != null) continue;

            string path    = elem.FindPropertyRelative("folderPath").stringValue;
            string keyword = path.Contains("/") ? path.Substring(path.LastIndexOf('/') + 1) : path;
            string lower   = keyword.ToLower();

            foreach (var btn in allBtns)
            {
                if (btn.gameObject.name.ToLower().Replace(" ","").Replace("_","").Contains(lower))
                {
                    elem.FindPropertyRelative("button").objectReferenceValue = btn;
                    n++;
                    break;
                }
            }
        }
        if (n > 0) so.ApplyModifiedProperties();
        return n;
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
}
