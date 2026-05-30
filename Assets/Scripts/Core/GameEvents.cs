using System;

/// <summary>
/// 매니저 간 직접 참조 없이 통신하기 위한 정적 이벤트 허브.
/// 구독은 OnEnable/Awake에서, 해제는 OnDisable/OnDestroy에서.
/// </summary>
public static class GameEvents
{
    // ── 씬 생명주기 ──────────────────────────────
    /// <summary>씬이 초기화 완료됐을 때 (SceneStartManager가 발생)</summary>
    public static event Action OnSceneInitialized;

    // ── 게임오버 ─────────────────────────────────
    /// <summary>게임오버 발생. reason = 원인 문자열</summary>
    public static event Action<string> OnGameOver;

    // ── 정신력 ───────────────────────────────────
    /// <summary>정신력이 변경됐을 때. (current, max)</summary>
    public static event Action<float, float> OnSanityChanged;

    // ── 스코어/스테이지 ──────────────────────────
    /// <summary>스코어 값이 바뀌었을 때 (UI 갱신 트리거)</summary>
    public static event Action OnScoreChanged;

    // ── 발생 메서드 ──────────────────────────────
    public static void RaiseSceneInitialized()                  => OnSceneInitialized?.Invoke();
    public static void RaiseGameOver(string reason)             => OnGameOver?.Invoke(reason);
    public static void RaiseSanityChanged(float cur, float max) => OnSanityChanged?.Invoke(cur, max);
    public static void RaiseScoreChanged()                      => OnScoreChanged?.Invoke();
}
