using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string startSceneName = "StartScene";

    private bool isGameOver;

    void OnEnable()
    {
        GameEvents.OnGameOver         += HandleGameOver;
        GameEvents.OnSceneInitialized += HandleSceneInit;
    }

    void OnDisable()
    {
        GameEvents.OnGameOver         -= HandleGameOver;
        GameEvents.OnSceneInitialized -= HandleSceneInit;
    }

    private void HandleSceneInit() => isGameOver = false;

    private void HandleGameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log($"[GameOver] {reason}");
        ScoreCount.Reset();
        StartCoroutine(ReturnToStart());
    }

    private IEnumerator ReturnToStart()
    {
        yield return null;
        SanityManager.PrepareForNewGame(); // 다음 게임 씬 Awake에서 maxSanity로 재초기화됨
        SceneManager.LoadScene(startSceneName);
    }

    public bool IsGameOver() => isGameOver;
}
