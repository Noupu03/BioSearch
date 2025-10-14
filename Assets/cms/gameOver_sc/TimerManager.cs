using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public float gameDuration = 60f; // ���� �ð�
    private float remainingTime;

    public TextMeshProUGUI timerText; // UI �����
    public bool IsTimeOver => remainingTime <= 0f;

    private bool isGameOverTriggered = false; // �ߺ� ȣ�� ����

    void Start()
    {
        remainingTime = gameDuration;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameOverTriggered) return; // �̹� �������� �ߴ�

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0f, remainingTime);
        UpdateTimerUI();

        if (remainingTime <= 0f)
        {
            isGameOverTriggered = true;
            OnTimeOver(); //  �ð� �� ���� �� ȣ��
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    void OnTimeOver()
    {
        Debug.Log("[TimeManager] ���� �ð� ����! ���� ���� ���� �ߵ� ����");
        // TODO: ���߿� GameOverManager�� ���� ����
        // ex) FindObjectOfType<GameOverManager>()?.TriggerGameOver();
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }

    public void ResetTimer()
    {
        remainingTime = gameDuration;
        isGameOverTriggered = false;
        UpdateTimerUI();
    }
}
