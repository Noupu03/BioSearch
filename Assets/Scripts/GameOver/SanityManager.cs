using UnityEngine;
using TMPro;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance { get; private set; }

    [Header("설정")]
    public float maxSanity = 100f;

    // static: 씬 리로드 사이에도 값 유지
    public static float currentSanityStatic;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI sanityText;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (currentSanityStatic <= 0f)
            currentSanityStatic = maxSanity;

        UpdateSanityUI();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnEnable()  => GameEvents.OnSceneInitialized += HandleSceneInit;
    void OnDisable() => GameEvents.OnSceneInitialized -= HandleSceneInit;

    private void HandleSceneInit() => UpdateSanityUI();

    public void DecreaseSanity(float amount)
    {
        currentSanityStatic = Mathf.Clamp(currentSanityStatic - amount, 0f, maxSanity);
        UpdateSanityUI();
        GameEvents.RaiseSanityChanged(currentSanityStatic, maxSanity);

        if (currentSanityStatic <= 0f)
            GameEvents.RaiseGameOver("정신력 고갈");
    }

    public void UpdateSanityUI()
    {
        if (sanityText != null)
            sanityText.text = $"Sanity: {currentSanityStatic:0}";
    }

    public void ResetSanity()
    {
        currentSanityStatic = maxSanity;
        UpdateSanityUI();
        GameEvents.RaiseSanityChanged(currentSanityStatic, maxSanity);
    }

    public float GetCurrentSanity() => currentSanityStatic;
}
