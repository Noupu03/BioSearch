using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Haare.Client.Routine.Service.SceneService;

/// <summary>
/// 게임 루프의 유일한 소유자.
///
/// DIP(의존 역전 원칙): 구체 매니저 참조 대신 IStageResettable 배열로
/// 리셋 대상을 받아, 새 매니저 추가 시 이 파일을 수정하지 않아도 된다.
/// </summary>
public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("리셋 순서대로 배치 (IStageResettable 구현체)")]
    [SerializeField] private MonoBehaviour[] stageResettables;

    [Header("카메라 복귀 (비워두면 자동 검색)")]
    [SerializeField] private HybridCameraController hybridCamera;

    [Header("전환 연출 설정")]
    [SerializeField] private float  transitionFadeDuration = 0.5f;
    [SerializeField] private float  transitionHoldDuration = 1.5f;
    [SerializeField] private string transitionMessage      = "다음 실험체를 받습니다...";

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
        if (!hybridCamera)  hybridCamera  = FindObjectOfType<HybridCameraController>(true);

        if (stageResettables == null || stageResettables.Length == 0)
        {
            var list = new List<MonoBehaviour>();
            var fw  = FindObjectOfType<FileWindow>(true);       if (fw  != null) list.Add(fw);
            var lwm = FindObjectOfType<LogWindowManager>(true); if (lwm != null) list.Add(lwm);
            var tm  = FindObjectOfType<TimerManager>(true);     if (tm  != null) list.Add(tm);
            stageResettables = list.ToArray();
            Debug.Log($"[GameLoopManager] stageResettables 자동 구성: {stageResettables.Length}개");
        }

        if (!hybridCamera)  Debug.LogWarning("[GameLoopManager] HybridCameraController 없음");
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnEnable()  => GameEvents.OnGameOver += HandleGameOver;
    void OnDisable() => GameEvents.OnGameOver -= HandleGameOver;

    /// <summary>씬 최초 로드 시 SceneStartManager가 호출.</summary>
    public void RequestFirstStage()
    {
        ResetAll();
        GameEvents.RaiseStageStarted();
    }

    /// <summary>판정 완료 후 SelectPopupManager가 호출.</summary>
    public void RequestNextStage()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        StartCoroutine(DoNextStage());
    }

    private void ResetAll()
    {
        foreach (var r in stageResettables)
            (r as IStageResettable)?.ResetForNewStage();
    }

    private IEnumerator DoNextStage()
    {
        yield return _transition.FadeToBlack();

        ResetAll();
        hybridCamera?.ReturnToRoomView();
        InputManager.Instance?.SimulateSPress();

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
        yield return SceneService.Instance.LoadScene(SceneName.StartScene).ToCoroutine();
    }
}
