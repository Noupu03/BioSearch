using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float totalTime = 60f;
    private float currentTime;
    private bool isRunning = false;

    private GameOverManager gameOverManager;

    void Start()
    {
        currentTime = totalTime;
        UpdateTimerText();
        gameOverManager = FindObjectOfType<GameOverManager>();

        //  �ڵ� ���� ����
        // isRunning = true;  �� �� �� ����
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        UpdateTimerText();

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;

            if (gameOverManager != null)
                gameOverManager.TriggerGameOver("�ð� �ʰ��� ���� ���� ����");

            
        }
    }

    public void StartTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            Debug.Log("[TimerManager] Ÿ�̸� ����!");
        }
    }

    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("[TimerManager] Ÿ�̸� ����!");
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}";
    }
}
