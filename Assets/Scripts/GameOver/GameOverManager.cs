using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    private bool isGameOver = false;
    public float returnDelay = 0f;
    public string startSceneName = "StartScene";

    [SerializeField] private SanityManager sanityManager;

    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($"[GameOver] 발생! 원인: {reason}");
        ScoreCount.Reset();
        StartCoroutine(ReturnToStartScene());
    }

    IEnumerator ReturnToStartScene()
    {
        yield return null;

        if (sanityManager != null)
            sanityManager.ResetSanity();

        SceneManager.LoadScene(startSceneName);
    }

    public void ResetGameOver()
    {
        isGameOver = false;
        Debug.Log("[GameOverManager] 상태 초기화됨.");
    }

    public bool IsGameOver() => isGameOver;
}
