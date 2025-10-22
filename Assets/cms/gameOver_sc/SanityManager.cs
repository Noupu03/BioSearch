using UnityEngine;
using TMPro;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;

    // static으로 선언하여 씬 Reload 시에도 값 유지
    public static float currentSanityStatic;

    [Header("UI (TMP Text)")]
    public TextMeshProUGUI sanityText;

    void Awake()
    {
        // 첫 실행이면 초기화
        if (currentSanityStatic == 0)
            currentSanityStatic = maxSanity;
        
        UpdateSanityUI();
    }

    public void DecreaseSanity(float amount)
    {
        currentSanityStatic -= amount;
        currentSanityStatic = Mathf.Clamp(currentSanityStatic, 0f, maxSanity);
        UpdateSanityUI();

        if (currentSanityStatic <= 0f)
        {
            // 더 이상 다음 스테이지로 진행하지 않고 게임오버 처리
            GameOverManager gameOver = FindObjectOfType<GameOverManager>();
            if (gameOver != null)
                gameOver.TriggerGameOver("Sanity reached zero");
        }
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

    // 현재 정신력 반환
    public float GetCurrentSanity() => currentSanityStatic;
}
