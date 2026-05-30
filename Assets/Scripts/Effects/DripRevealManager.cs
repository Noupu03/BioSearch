// DripRevealManager.cs
// ����/��Ʈ ������ '����Ʒ� ��ܽ� ����'�� �����ϴ� �Ŵ���
// - Shader: "BioSearch/QuantizedVerticalReveal_UIStable" ����
// - �ڽ� �÷��� ����, �ϵ��� �⺻
// - UI(Text/TMP/Mask) ����: �⺻ ���� �ɼ� ����

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
    [SerializeField] private Material revealMaterial;

    [Header("Sequence (Top��Down)")]
    [Tooltip("������ ��ü ���� ����(��).")]
    public float globalDelay = 0f;
    [Tooltip("�׸� �� ���� ����(��). ������ �Ʒ��� ���� ��ġ.")]
    public float perItemDelay = 0.06f;
    [Tooltip("���� �׸� ���� �ҿ�(��).")]
    public float duration = 0.35f;
    [Tooltip("���� Y(��epsilon) ������ ���� Ÿ�̹����� ó��.")]
    public bool groupByRow = false;
    [Tooltip("�� ���� ��� ����(Y, ���� ����).")]
    public float rowEpsilon = 0.001f;

    [Header("Look")]
    [Tooltip("���(���Ҷҡ�) ����. �������� �� �߰� ����.")]
    public int steps = 3;
    [Tooltip("��� �ε巯��(�ϵ��� �⺻�̹Ƿ� 0 ����).")]
    public float feather = 0f;
    [Tooltip("�ϵ���(1) / ����Ʈ(0).")]
    public bool hardCut = true;
    [Tooltip("����Ʒ� ����(true) / �Ʒ�����(false).")]
    public bool invertY = true;

    [Header("Filters")]
    [Tooltip("Text, TMP_Text, TextMeshProUGUI�� �⺻ ����.")]
    public bool excludeTextAndTMP = true;
    [Tooltip("Mask, RectMask2D ���� ������Ʈ �⺻ ����.")]
    public bool excludeMaskProviders = true;
    [Tooltip("���� ���̾�. (�⺻: ��ü)")]
    public LayerMask includeLayers = ~0;
    [Tooltip("���� �±�(�ɼ�).")]
    public string[] ignoreTags;

    [Header("Auto Apply")]
    [Tooltip("Start���� ���� ���� ��Ʈ�鿡 �ڵ� ����.")]
    public bool applyOnStart = false;
    [Tooltip("�� �ε�� �ڵ� ����.")]
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
            // ���� ���� ��� ��Ʈ�� ����
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

    /// <summary>���� ��Ʈ(������/�г� ��) ������ ����Ʒ� ��� ���� ����.</summary>
    public void RevealRoot(Transform root)
    {
        if (!root || !revealMaterial) return;

        var items = CollectTargets(root);
        if (items.Count == 0) return;

        ApplyCommonParams(items);

        // ���� �� Ÿ�Ӷ��� ��ġ
        var baseTime = Time.time + globalDelay;
        if (groupByRow)
            AssignStartTimesGrouped(items, baseTime, perItemDelay, rowEpsilon);
        else
            AssignStartTimesLinear(items, baseTime, perItemDelay);
    }

    /// <summary>�� ��ü(��� ��Ʈ) ����.</summary>
    public void RevealWholeScene()
    {
        foreach (var go in gameObject.scene.GetRootGameObjects())
            RevealRoot(go.transform);
    }

    /// <summary>��Ʈ ���� �ٽ� ����(���� ������ ����).</summary>
    public void ResetHidden(Transform root, float delay = 1000f)
    {
        // _Start�� �̷��� ũ�� �о�θ� prog=0���� ������
        var items = CollectTargets(root);
        var start = Time.time + delay;
        foreach (var it in items) it.SetStart(start);
    }

    // =========================
    // ���� ����
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
            s.sharedMaterial = revealMaterial; // ��Ī ����

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

        // UI Image (�ν��Ͻ� ��Ƽ���� �ʿ�: ���� �ٸ� _Start)
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

        // ����(����Ʒ�)
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
        // ���� Y(��eps)�� �ϳ��� �׷����� ���� ���� Start ��ġ
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
