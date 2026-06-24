using UnityEngine;
using TMPro;

public class SanityManager : MonoBehaviour, IStageResettable
{
    public static SanityManager Instance { get; private set; }

    [Header("설정")]
    public float maxSanity = 100f;

    public static float currentSanityStatic;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI sanityText;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        currentSanityStatic = maxSanity;
        UpdateSanityUI();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    /// <summary>스테이지 전환 시: Sanity 값은 유지하고 UI만 갱신.</summary>
    public void ResetForNewStage() => UpdateSanityUI();

    /// <summary>게임오버 후 새 게임 시작 시 GameLoopManager가 호출.</summary>
    public void ResetSanityForNewGame()
    {
        currentSanityStatic = maxSanity;
        UpdateSanityUI();
        GameEvents.RaiseSanityChanged(currentSanityStatic, maxSanity);
    }

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

    public float GetCurrentSanity() => currentSanityStatic;
}
