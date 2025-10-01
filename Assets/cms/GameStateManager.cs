using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Normal,
        EscapePreparing,
        Escape
    }

    public GameState currentState = GameState.Normal;
    public float escapeDelay = 3f;

    private Coroutine escapeCoroutine;

    [Header("ī�޶� ����")]
    public Camera mainCamera; // Inspector���� ī�޶� ����

    void Update()
    {
        // EŰ ������ Ż�� �غ� ����
        if (Input.GetKeyDown(KeyCode.E) && currentState == GameState.Normal)
        {
            ChangeState(GameState.EscapePreparing);
            escapeCoroutine = StartCoroutine(EscapeDelayCoroutine());
        }

        // ���콺 Ŭ�� ����
        if (Input.GetMouseButtonDown(0))
        {
            CheckClickOnObject();
        }
    }

    void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log("���� ����: " + currentState);
    }

    IEnumerator EscapeDelayCoroutine()
    {
        yield return new WaitForSeconds(escapeDelay);

        // Ż�� ���� ���̾����� Escape ���·� ��ȯ
        if (currentState == GameState.EscapePreparing)
        {
            ChangeState(GameState.Escape);
        }
    }

    void CheckClickOnObject()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Ŭ���� ������Ʈ: " + hit.collider.name);

            // EscapeCancelObject �±׸� ������ Ż�� ���
            if (hit.collider.CompareTag("EscapeCancelObject"))
            {
                CancelEscape();
            }
        }
    }

    void CancelEscape()
    {
        // ���� ���� ������� Ż�� ���
        if (escapeCoroutine != null)
        {
            StopCoroutine(escapeCoroutine);
            escapeCoroutine = null;
        }

        ChangeState(GameState.Normal);
        Debug.Log("Ż�� ���� ��ҵ�!");
    }
}
