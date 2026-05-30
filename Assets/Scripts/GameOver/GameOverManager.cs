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
        // SanityManager가 OnGameOver를 받아 스스로 리셋하므로 여기서 직접 호출 불필요
        SanityManager.currentSanityStatic = 0f; // 다음 씬 Awake에서 maxSanity로 초기화됨
        SceneManager.LoadScene(startSceneName);
    }

    public bool IsGameOver() => isGameOver;
}
