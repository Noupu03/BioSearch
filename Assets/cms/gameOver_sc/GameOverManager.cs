using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    private bool isGameOver = false;

    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($"[GameOver] ����: {reason}");
        // TODO: ���ӿ��� UI / �� ��ȯ ��
    }
}
