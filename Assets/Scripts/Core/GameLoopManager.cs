using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 게임 루프의 유일한 소유자.
/// SerializeField가 비어있으면 Awake에서 FindObjectOfType으로 자동 할당.
/// </summary>
public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("리셋 대상 매니저 (비워두면 자동 검색)")]
    [SerializeField] private SanityManager    sanityManager;
    [SerializeField] private FileWindow       fileWindow;
    [SerializeField] private DummyIconSpawner dummyIconSpawner;
    [SerializeField] private LogWindowManager logWindowManager;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private TimerManager     timerManager;
    [SerializeField] private GameOverManager  gameOverManager;

    [Header("카메라 복귀 (비워두면 자동 검색)")]
    [SerializeField] private HybridCameraController hybridCamera;

    [Header("전환 연출 설정")]
    [SerializeField] private float  transitionFadeDuration = 0.5f;
    [SerializeField] private float  transitionHoldDuration = 1.5f;
    [SerializeField] private string transitionMessage      = "다음 실험체를 받습니다...";

    [Header("설정")]
    [SerializeField] private string startSceneName = "StartScene";

    private StageTransitionUI _transition;
    private bool              _isTransitioning;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        AutoAssign();

        var go = new GameObject("StageTransitionUI");
        _transition = go.AddComponent<StageTransitionUI>();
        _transition.Setup(transitionFadeDuration, transitionHoldDuration, transitionMessage);
    }

    private void AutoAssign()
    {
        if (!sanityManager)    sanityManager    = FindObjectOfType<SanityManager>(true);
        if (!fileWindow)       fileWindow       = FindObjectOfType<FileWindow>(true);
        if (!dummyIconSpawner) dummyIconSpawner = FindObjectOfType<DummyIconSpawner>(true);
        if (!logWindowManager) logWindowManager = FindObjectOfType<LogWindowManager>(true);
        if (!gameStateManager) gameStateManager = FindObjectOfType<GameStateManager>(true);
        if (!timerManager)     timerManager     = FindObjectOfType<TimerManager>(true);
        if (!gameOverManager)  gameOverManager  = FindObjectOfType<GameOverManager>(true);
        if (!hybridCamera)     hybridCamera     = FindObjectOfType<HybridCameraController>(true);

        LogMissingRefs();
    }

    private void LogMissingRefs()
    {
        if (!sanityManager)    Debug.LogWarning("[GameLoopManager] SanityManager 없음");
        if (!fileWindow)       Debug.LogWarning("[GameLoopManager] FileWindow 없음");
        if (!dummyIconSpawner) Debug.LogWarning("[GameLoopManager] DummyIconSpawner 없음");
        if (!logWindowManager) Debug.LogWarning("[GameLoopManager] LogWindowManager 없음");
        if (!gameStateManager) Debug.LogWarning("[GameLoopManager] GameStateManager 없음");
        if (!timerManager)     Debug.LogWarning("[GameLoopManager] TimerManager 없음");
        if (!gameOverManager)  Debug.LogWarning("[GameLoopManager] GameOverManager 없음");
        if (!hybridCamera)     Debug.LogWarning("[GameLoopManager] HybridCameraController 없음 — camera2 스냅 불가");
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

    /// <summary>판정 완료 후 SelectPopupManager가 호출. 전환 연출 후 다음 스테이지 시작.</summary>
    public void RequestNextStage()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        StartCoroutine(DoNextStage());
    }

    private IEnumerator DoNextStage()
    {
        // 1. 검은 화면 페이드 인 + 메시지 표시
        yield return _transition.FadeToBlack();

        // 2. 검은 화면 동안 리셋 (플레이어에게 보이지 않음)
        sanityManager?.ResetForNewStage();
        fileWindow?.ResetForNewStage();
        dummyIconSpawner?.ResetForNewStage();
        logWindowManager?.ResetForNewStage();
        gameStateManager?.ResetForNewStage();
        timerManager?.ResetForNewStage();
        gameOverManager?.ResetForNewStage();

        // 3. 카메라를 초기화면(방뷰)으로 복귀 — S키 누른 것과 동일한 경로
        hybridCamera?.ReturnToRoomView();           // camera2를 view2로 스냅 후 camera1 전환
        InputManager.Instance?.SimulateSPress();    // OnSPressed 발생 → CameraSwitcher, CameraPostFX 반응

        // 4. 페이드 아웃 → 초기화면 노출
        yield return _transition.FadeFromBlack();

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
