using UnityEngine;
using TMPro;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    public float maxSanity = 100f;

    // static���� �����Ͽ� �� Reload �ÿ��� �� ����
    public static float currentSanityStatic;

    [Header("UI (TMP Text)")]
    public TextMeshProUGUI sanityText;

    void Awake()
    {
        // ù �����̸� �ʱ�ȭ
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
            // �� �̻� ���� ���������� �������� �ʰ� ���ӿ��� ó��
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

    // ���� ���ŷ� ��ȯ
    public float GetCurrentSanity() => currentSanityStatic;
}
