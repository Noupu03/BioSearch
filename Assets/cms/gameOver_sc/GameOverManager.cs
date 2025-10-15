using UnityEngine;
using TMPro; // UI ǥ�ÿ� (����)

public class GameOverManager : MonoBehaviour
{
    private bool isGameOver = false;

    [Header("UI ���� (���� ����)")]
    public TextMeshProUGUI gameOverText; //  ���ӿ��� �޽��� TMP �����

    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($"[GameOverManager] ���� ���� �߻�! ����: {reason}");

        // UI ǥ�� (�ӽ� �ؽ�Ʈ)
        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER\n({reason})";
            gameOverText.gameObject.SetActive(true);
        }

       
    }

    public void ResetGameOver()
    {
        isGameOver = false;
        Time.timeScale = 1f;

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}
