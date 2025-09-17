using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Normal,          // �⺻ ����
        EscapePreparing, // Ż�� ������
        Escape           // Ż�� (���� �ߵ�)
    }

    public GameState currentState = GameState.Normal;

    public float escapeDelay = 3f; // �� �� �� Ż�� Ȯ��

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentState == GameState.Normal)
        {
            ChangeState(GameState.EscapePreparing);
            StartCoroutine(EscapeDelayCoroutine());
        }
    }

    void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log("���� ����: " + currentState);

        switch (currentState)
        {
            case GameState.Normal:
                // TODO: �Ϲ� ���� ȿ��
                break;
            case GameState.EscapePreparing:
                // TODO: Ż�� ������ (���, UI, �Ҹ� ���� �غ� ��)
                break;
            case GameState.Escape:
                // TODO: Ż�� �ߵ� (�� ����, ����� ���� ��)
                break;
        }
    }

    IEnumerator EscapeDelayCoroutine()
    {
        yield return new WaitForSeconds(escapeDelay);
        if (currentState == GameState.EscapePreparing)
        {
            ChangeState(GameState.Escape);
        }
    }
}
