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
            sanity.UpdateSanityUI(); // ���� ��ġ �״�� ǥ��
        }

        if (gameOver != null)
        {
            gameOver.ResetGameOver();
        }

        Debug.Log("[SceneStartManager] �ʱ�ȭ �Ϸ�, Timer ����, Sanity UI ����, GameOver ���� �ʱ�ȭ");
    }
}
