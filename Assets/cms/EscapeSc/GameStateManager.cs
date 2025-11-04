using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        Normal,
        EscapePreparing,
        Escape,
        Overwhelmed
    }

    public GameState currentState = GameState.Normal;
    public float escapeDelay = 3f;

    public EscapePatternSimple escapeMover; 

    private Coroutine escapeCoroutine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentState == GameState.Normal)
        {
            ChangeState(GameState.EscapePreparing);

            if (escapeCoroutine != null) StopCoroutine(escapeCoroutine);
            escapeCoroutine = StartCoroutine(EscapeDelayCoroutine());
        }
    }


    void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log("[GameStateManager] 상태 변경: " + currentState);

        switch (currentState)
        {
            case GameState.Normal:
                if (escapeMover != null)
                    escapeMover.MoveBack();
                break;

            case GameState.EscapePreparing:

                break;

            case GameState.Escape:

                if (escapeMover != null)
                    escapeMover.MoveToEscape();
                break;
        }
    }

    IEnumerator EscapeDelayCoroutine()
    {
        yield return new WaitForSeconds(escapeDelay);
   
        escapeCoroutine = null;

        if (currentState == GameState.EscapePreparing)
            ChangeState(GameState.Escape);
    }

   
    public void RequestCancelEscape()
    {
        if (escapeCoroutine != null)
        {
            StopCoroutine(escapeCoroutine);
            escapeCoroutine = null;
        }

        
        ChangeState(GameState.Normal);
        Debug.Log("[GameStateManager] RequestCancelEscape 호출 - Normal로 전환");
    }
}
