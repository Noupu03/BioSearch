using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("설정")]
    [SerializeField] private float totalTime = 60f;

    private float currentTime;
    private bool  isRunning;
    private bool  gameOverFired;

    void OnEnable()
    {
        GameEvents.OnSceneInitialized += HandleSceneInit;
        GameEvents.OnGameOver         += HandleGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnSceneInitialized -= HandleSceneInit;
        GameEvents.OnGameOver         -= HandleGameOver;
    }

    private void HandleSceneInit()
    {
        ResetTimer();
        StartTimer();
    }

    private void HandleGameOver(string _) => isRunning = false;

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        UpdateTimerUI();

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning   = false;
            if (!gameOverFired)
            {
                gameOverFired = true;
                GameEvents.RaiseGameOver("시간 초과");
            }
        }
    }

    public void StartTimer()
    {
        if (isRunning) return;
        isRunning = true;
    }

    public void StopTimer() => isRunning = false;

    public void ResetTimer()
    {
        currentTime   = totalTime;
        isRunning     = false;
        gameOverFired = false;
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}";
    }
}
