// DripRevealManager.cs
// 전역/루트 단위로 '위→아래 계단식 리빌'을 적용하는 매니저
// - Shader: "BioSearch/QuantizedVerticalReveal_UIStable" 가정
// - 박스 플래시 없음, 하드컷 기본
// - UI(Text/TMP/Mask) 안전: 기본 제외 옵션 제공

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-50)]
public class DripRevealManager : MonoBehaviour
{
    public static DripRevealManager Instance { get; private set; }

    [Header("Material/Shader")]
    [Tooltip("UI/스프라이트 겸용 셰이더 머티리얼 (\"BioSearch/QuantizedVerticalReveal_UIStable\").")]
    public Material revealMaterial;

    [Header("Sequence (Top→Down)")]
    [Tooltip("시퀀스 전체 시작 지연(초).")]
    public float globalDelay = 0f;
    [Tooltip("항목 간 시작 간격(초). 위에서 아래로 차등 배치.")]
    public float perItemDelay = 0.06f;
    [Tooltip("개별 항목 리빌 소요(초).")]
    public float duration = 0.35f;
    [Tooltip("같은 Y(±epsilon) 묶음을 같은 타이밍으로 처리.")]
    public bool groupByRow = false;
    [Tooltip("행 묶음 허용 오차(Y, 월드 단위).")]
    public float rowEpsilon = 0.001f;

    [Header("Look")]
    [Tooltip("계단(‘뚝뚝’) 개수. 높을수록 더 잘게 끊김.")]
    public int steps = 3;
    [Tooltip("경계 부드러움(하드컷 기본이므로 0 권장).")]
    public float feather = 0f;
    [Tooltip("하드컷(1) / 소프트(0).")]
    public bool hardCut = true;
    [Tooltip("위→아래 진행(true) / 아래→위(false).")]
    public bool invertY = true;

    [Header("Filters")]
    [Tooltip("Text, TMP_Text, TextMeshProUGUI는 기본 제외.")]
    public bool excludeTextAndTMP = true;
    [Tooltip("Mask, RectMask2D 보유 오브젝트 기본 제외.")]
    public bool excludeMaskProviders = true;
    [Tooltip("포함 레이어. (기본: 전체)")]
    public LayerMask includeLayers = ~0;
    [Tooltip("제외 태그(옵션).")]
    public string[] ignoreTags;

    [Header("Auto Apply")]
    [Tooltip("Start에서 현재 씬의 루트들에 자동 적용.")]
    public bool applyOnStart = false;
    [Tooltip("씬 로드시 자동 적용.")]
    public bool applyOnSceneLoaded = false;

    // Shader property IDs
    static readonly int ID_Start = Shader.PropertyToID("_Start");
    static readonly int ID_Dur = Shader.PropertyToID("_Duration");
    static readonly int ID_Steps = Shader.PropertyToID("_Steps");
    static readonly int ID_Feather = Shader.PropertyToID("_Feather");
    static readonly int ID_InvertY = Shader.PropertyToID("_InvertY");
    static readonly int ID_HardCut = Shader.PropertyToID("_HardCut");

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (applyOnSceneLoaded) SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (applyOnStart)
        {
            // 현재 씬의 모든 루트에 적용
            foreach (var root in gameObject.scene.GetRootGameObjects())
                RevealRoot(root.transform);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!applyOnSceneLoaded) return;
        foreach (var root in scene.GetRootGameObjects())
            RevealRoot(root.transform);
    }

    // =========================
    // Public API
    // =========================

    /// <summary>지정 루트(프리팹/패널 등) 하위에 위→아래 계단 리빌 적용.</summary>
    public void RevealRoot(Transform root)
    {
        if (!root || !revealMaterial) return;

        var items = CollectTargets(root);
        if (items.Count == 0) return;

        ApplyCommonParams(items);

        // 정렬 및 타임라인 배치
        var baseTime = Time.time + globalDelay;
        if (groupByRow)
            AssignStartTimesGrouped(items, baseTime, perItemDelay, rowEpsilon);
        else
            AssignStartTimesLinear(items, baseTime, perItemDelay);
    }

    /// <summary>씬 전체(모든 루트) 적용.</summary>
    public void RevealWholeScene()
    {
        foreach (var go in gameObject.scene.GetRootGameObjects())
            RevealRoot(go.transform);
    }

    /// <summary>루트 하위 다시 숨김(다음 리빌을 위해).</summary>
    public void ResetHidden(Transform root, float delay = 1000f)
    {
        // _Start를 미래로 크게 밀어두면 prog=0으로 유지됨
        var items = CollectTargets(root);
        var start = Time.time + delay;
        foreach (var it in items) it.SetStart(start);
    }

    // =========================
    // 내부 구현
    // =========================

    struct Item
    {
        public Transform tr;
        public System.Action SetupCommon;    // duration/steps/feather/invertY/hardCut
        public System.Action<float> SetStart; // _Start(t)
        public float y;
        public bool isUI;
    }

    List<Item> CollectTargets(Transform root)
    {
        var list = new List<Item>();

        // SpriteRenderer
        var sprites = root.GetComponentsInChildren<SpriteRenderer>(includeInactive: true)
            .Where(s => s && Keep(s.gameObject)).ToList();

        foreach (var s in sprites)
        {
            var mpb = new MaterialPropertyBlock();
            s.GetPropertyBlock(mpb);
            s.sharedMaterial = revealMaterial; // 배칭 유지

            void Setup()
            {
                mpb.SetFloat(ID_Dur, duration);
                mpb.SetFloat(ID_Steps, steps);
                mpb.SetFloat(ID_Feather, feather);
                mpb.SetFloat(ID_InvertY, invertY ? 1f : 0f);
                mpb.SetFloat(ID_HardCut, hardCut ? 1f : 0f);
                s.SetPropertyBlock(mpb);
            }
            void SetStart(float t)
            {
                mpb.SetFloat(ID_Start, t);
                s.SetPropertyBlock(mpb);
            }

            list.Add(new Item
            {
                tr = s.transform,
                SetupCommon = Setup,
                SetStart = SetStart,
                y = s.transform.position.y,
                isUI = false
            });
        }

        // UI Image (인스턴스 머티리얼 필요: 서로 다른 _Start)
        var images = root.GetComponentsInChildren<Image>(includeInactive: true)
            .Where(i => i && Keep(i.gameObject)).ToList();

        foreach (var img in images)
        {
            var inst = new Material(revealMaterial) { name = "Reveal(UI) (Instance)" };
            img.material = inst;

            void Setup()
            {
                inst.SetFloat(ID_Dur, duration);
                inst.SetFloat(ID_Steps, steps);
                inst.SetFloat(ID_Feather, feather);
                inst.SetFloat(ID_InvertY, invertY ? 1f : 0f);
                inst.SetFloat(ID_HardCut, hardCut ? 1f : 0f);
            }
            void SetStart(float t)
            {
                inst.SetFloat(ID_Start, t);
            }

            list.Add(new Item
            {
                tr = img.transform,
                SetupCommon = Setup,
                SetStart = SetStart,
                y = img.transform.position.y,
                isUI = true
            });
        }

        // 정렬(위→아래)
        list = list.OrderByDescending(it => it.y).ToList();
        return list;
    }

    void ApplyCommonParams(List<Item> items)
    {
        foreach (var it in items) it.SetupCommon?.Invoke();
    }

    void AssignStartTimesLinear(List<Item> items, float baseTime, float delta)
    {
        for (int i = 0; i < items.Count; i++)
            items[i].SetStart(baseTime + i * delta);
    }

    void AssignStartTimesGrouped(List<Item> items, float baseTime, float delta, float eps)
    {
        // 같은 Y(±eps)를 하나의 그룹으로 묶어 동일 Start 배치
        var groups = new List<List<Item>>();
        foreach (var it in items)
        {
            var g = groups.FirstOrDefault(grp => Mathf.Abs(grp[0].y - it.y) <= eps);
            if (g == null) groups.Add(new List<Item> { it });
            else g.Add(it);
        }
        for (int gi = 0; gi < groups.Count; gi++)
        {
            float t = baseTime + gi * delta;
            foreach (var it in groups[gi]) it.SetStart(t);
        }
    }

    bool Keep(GameObject go)
    {
        if (!go || !go.activeInHierarchy) return false;
        if (((1 << go.layer) & includeLayers.value) == 0) return false;
        if (ignoreTags != null && ignoreTags.Length > 0 && ignoreTags.Contains(go.tag)) return false;

        if (excludeTextAndTMP)
        {
            if (go.GetComponent<Text>() != null) return false;
            if (go.GetComponent<TMP_Text>() != null) return false;
            if (go.GetComponent<TextMeshProUGUI>() != null) return false;
        }
        if (excludeMaskProviders)
        {
            if (go.GetComponent<Mask>() != null) return false;
            if (go.GetComponent<RectMask2D>() != null) return false;
        }
        return true;
    }
}
