using UnityEngine;

public class GameOverManager : MonoBehaviour, IStageResettable
{
    private bool isGameOver;

    void OnEnable()  => GameEvents.OnGameOver += HandleGameOver;
    void OnDisable() => GameEvents.OnGameOver -= HandleGameOver;

    private void HandleGameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
        // 실제 씬 전환 및 리셋은 GameLoopManager.HandleGameOver가 담당
    }

    /// <summary>새 스테이지 시작 시 GameLoopManager가 호출.</summary>
    public void ResetForNewStage() => isGameOver = false;

    public bool IsGameOver() => isGameOver;
}
