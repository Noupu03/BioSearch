using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState { Normal, EscapePreparing, Escape, Overwhelmed }

    [SerializeField] private GameState           currentState = GameState.Normal;
    [SerializeField] private float               escapeDelay  = 3f;
    [SerializeField] private EscapePatternSimple escapeMover;

    private Coroutine escapeCoroutine;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentState == GameState.Normal)
        {
            ChangeState(GameState.EscapePreparing);
            if (escapeCoroutine != null) StopCoroutine(escapeCoroutine);
            escapeCoroutine = StartCoroutine(EscapeDelayCoroutine());
        }
    }

    private void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log("[GameStateManager] 상태 변경: " + currentState);

        switch (currentState)
        {
            case GameState.Normal:  escapeMover?.MoveBack();       break;
            case GameState.Escape:  escapeMover?.MoveToEscape();   break;
        }
    }

    private IEnumerator EscapeDelayCoroutine()
    {
        yield return new WaitForSeconds(escapeDelay);
        escapeCoroutine = null;
        if (currentState == GameState.EscapePreparing)
            ChangeState(GameState.Escape);
    }

    public void RequestCancelEscape()
    {
        if (escapeCoroutine != null) { StopCoroutine(escapeCoroutine); escapeCoroutine = null; }
        ChangeState(GameState.Normal);
    }
}
