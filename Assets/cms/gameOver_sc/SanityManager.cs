using UnityEngine;
using TMPro;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;

    public static float currentSanityStatic;

    [Header("UI (TMP Text)")]
    public TextMeshProUGUI sanityText;

    [SerializeField] private GameOverManager gameOverManager;

    void Awake()
    {
        if (currentSanityStatic == 0)
            currentSanityStatic = maxSanity;

        UpdateSanityUI();
    }

    public void DecreaseSanity(float amount)
    {
        currentSanityStatic -= amount;
        currentSanityStatic = Mathf.Clamp(currentSanityStatic, 0f, maxSanity);
        UpdateSanityUI();

        if (currentSanityStatic <= 0f && gameOverManager != null)
            gameOverManager.TriggerGameOver("Sanity reached zero");
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
    }

    public float GetCurrentSanity() => currentSanityStatic;
}
