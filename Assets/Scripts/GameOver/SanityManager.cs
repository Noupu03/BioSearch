using UnityEngine;
using TMPro;

public class SanityManager : MonoBehaviour
{
    [Header("설정")]
    public float maxSanity = 100f;

    // static으로 씬 리로드 시에도 값 유지
    public static float currentSanityStatic;

    [Header("UI")]
    public TextMeshProUGUI sanityText;

    void Awake()
    {
        if (currentSanityStatic <= 0f)
            currentSanityStatic = maxSanity;

        UpdateSanityUI();
    }

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
