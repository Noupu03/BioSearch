using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    private bool isGameOver = false;
    public float returnDelay = 5f; // 5초 후 복귀
    public string startSceneName = "StartScene"; // 복귀할 씬 이름

    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return; // 중복 호출 방지
        isGameOver = true;

        Debug.Log($"[GameOver] 발생! 이유: {reason}");
        StartCoroutine(ReturnToStartScene());
    }

    private IEnumerator ReturnToStartScene()
    {
        Debug.Log("[GameOverManager] 5초 후 시작 화면으로 복귀합니다...");
        yield return new WaitForSeconds(returnDelay);

        SceneManager.LoadScene(startSceneName);
    }

    public void ResetGameOver()
    {
        isGameOver = false;
        Debug.Log("[GameOverManager] 상태 초기화됨.");
    }

    public bool IsGameOver()
    {
        return isGameOver;

    }
}
