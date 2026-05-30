using UnityEngine;

public class SceneStartManager : MonoBehaviour
{
    [SerializeField] private TimerManager timer;
    [SerializeField] private SanityManager sanity;
    [SerializeField] private GameOverManager gameOver;

    void Start()
    {
        if (timer != null)
        {
            timer.ResetTimer();
            timer.StartTimer();
        }

        if (sanity != null)
            sanity.UpdateSanityUI();

        if (gameOver != null)
            gameOver.ResetGameOver();

        Debug.Log("[SceneStartManager] 초기화 완료: Timer 시작, Sanity UI 갱신, GameOver 상태 초기화");
    }
}
