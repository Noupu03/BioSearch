using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float totalTime = 60f;
    private float currentTime;
    public bool isRunning = false;

    private GameOverManager gameOverManager;

    void Start()
    {
        currentTime = totalTime;
        UpdateTimerText();
        gameOverManager = FindObjectOfType<GameOverManager>();
    }

    void Update()
    {
        if (!isRunning || gameOverManager == null || gameOverManager.IsGameOver()) return;

        currentTime -= Time.deltaTime;
        UpdateTimerText();

        if (currentTime <= 0f)
        {

            currentTime = 0f;
            isRunning = false;
            
            gameOverManager.TriggerGameOver("시간 초과로 인한 게임 오버");
        }
    }

    public void StartTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            Debug.Log("[TimerManager] 타이머 시작!");
        }
    }

    public void StopTimer() => isRunning = false;

    public void ResetTimer()
    {
        currentTime = totalTime;
        UpdateTimerText();
        isRunning = false;
        Debug.Log("[TimerManager] 타이머 초기화 완료!");
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}";
    }
}
