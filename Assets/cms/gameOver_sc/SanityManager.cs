using UnityEngine;
using TMPro;

public class SanityManager : MonoBehaviour
{
    [Header("���ŷ� ����")]
    public float maxSanity = 100f;
    private float currentSanity;

    [Header("UI ǥ�� (�ӽ� TMP)")]
    public TextMeshProUGUI sanityText;

    [Header("���� ����")]
    public float decreaseAmount = 5f;

    private bool isGameOverTriggered = false;

    void Start()
    {
        currentSanity = maxSanity;
        UpdateSanityUI();
    }

    void Update()
    {
        if (isGameOverTriggered)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DecreaseSanity(decreaseAmount);
        }
    }

    public void DecreaseSanity(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
        UpdateSanityUI();

        if (currentSanity <= 0f)
        {
            isGameOverTriggered = true;
            OnSanityDepleted();
        }
    }

    void UpdateSanityUI()
    {
        if (sanityText != null)
            sanityText.text = $"Sanity: {currentSanity:0} / {maxSanity:0}";
    }

    void OnSanityDepleted()
    {
        //  GameOverManager���� �˸��� ����
        FindObjectOfType<GameOverManager>()?.TriggerGameOver("���ŷ� 0���� ���� ���� ����");
    }

    public void ResetSanity()
    {
        currentSanity = maxSanity;
        isGameOverTriggered = false;
        UpdateSanityUI();
    }
}
