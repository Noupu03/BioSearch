using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    private bool isGameOver = false;
    public float returnDelay = 0f; // 5초 후 복귀
    public string startSceneName = "StartScene"; // 복귀할 씬 이름

    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return; // 중복 호출 방지
        isGameOver = true;

        Debug.Log($"[GameOver] 발생! 이유: {reason}");

        // 게임오버 시 스테이지/성공/실패 카운트 초기화
        SelectPopupManager popupManager = FindObjectOfType<SelectPopupManager>();
        if (popupManager != null)
            popupManager.ResetCounts();
       

        StartCoroutine(ReturnToStartScene());


    }

 IEnumerator ReturnToStartScene()
{
    // optional delay
    yield return new WaitForSeconds(returnDelay);

    // 게임오버 시 초기화
    SanityManager sanity = FindObjectOfType<SanityManager>();
    if (sanity != null)
        sanity.ResetSanity();

    // 시작 씬 로드
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
