using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStartManager : MonoBehaviour
{
    void Start()
    {
        TimerManager timer = FindObjectOfType<TimerManager>();
        SanityManager sanity = FindObjectOfType<SanityManager>();
        GameOverManager gameOver = FindObjectOfType<GameOverManager>();

        if (timer != null)
        {
            timer.ResetTimer();
            timer.StartTimer();
        }

        if (sanity != null)
        {
            sanity.UpdateSanityUI(); // 현재 수치 그대로 표시
        }

        if (gameOver != null)
        {
            gameOver.ResetGameOver();
        }

        Debug.Log("[SceneStartManager] 초기화 완료, Timer 시작, Sanity UI 갱신, GameOver 상태 초기화");
    }
}
