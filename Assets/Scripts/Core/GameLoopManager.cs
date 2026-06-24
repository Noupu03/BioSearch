using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 게임 루프의 유일한 소유자.
/// - 스테이지 전환: RequestNextStage() → 각 매니저 ResetForNewStage() 순서 호출
/// - 게임오버: OnGameOver 수신 → StartScene 전환
/// </summary>
public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("리셋 대상 매니저 (순서 고정)")]
    [SerializeField] private SanityManager     sanityManager;
    [SerializeField] private FileWindow        fileWindow;
    [SerializeField] private DummyIconSpawner  dummyIconSpawner;
    [SerializeField] private LogWindowManager  logWindowManager;
    [SerializeField] private GameStateManager  gameStateManager;
    [SerializeField] private TimerManager      timerManager;
    [SerializeField] private GameOverManager   gameOverManager;

    [Header("설정")]
    [SerializeField] private string startSceneName = "StartScene";

    private bool _isTransitioning;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnEnable()  => GameEvents.OnGameOver += HandleGameOver;
    void OnDisable() => GameEvents.OnGameOver -= HandleGameOver;

    /// <summary>씬 최초 로드 시 SceneStartManager가 호출. 리셋 없이 타이머만 시작.</summary>
    public void RequestFirstStage()
    {
        timerManager?.ResetForNewStage();
        gameOverManager?.ResetForNewStage();
        GameEvents.RaiseStageStarted();
    }

    /// <summary>판정 완료 후 SelectPopupManager가 호출. 다음 스테이지로 전환.</summary>
    public void RequestNextStage()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        StartCoroutine(DoNextStage());
    }

    private IEnumerator DoNextStage()
    {
        yield return null;
        sanityManager?.ResetForNewStage();
        fileWindow?.ResetForNewStage();
        dummyIconSpawner?.ResetForNewStage();
        logWindowManager?.ResetForNewStage();
        gameStateManager?.ResetForNewStage();
        timerManager?.ResetForNewStage();
        gameOverManager?.ResetForNewStage();
        GameEvents.RaiseStageStarted();
        _isTransitioning = false;
    }

    private void HandleGameOver(string reason)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        Debug.Log($"[GameLoopManager] 게임오버: {reason}");
        StartCoroutine(DoGameOver());
    }

    private IEnumerator DoGameOver()
    {
        yield return null;
        ScoreCount.Reset();
        sanityManager?.ResetSanityForNewGame();
        SceneManager.LoadScene(startSceneName);
    }
}
