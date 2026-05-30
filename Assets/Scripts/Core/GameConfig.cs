/// <summary>
/// 게임 전역 상수. 매직 넘버를 한 곳에서 관리한다.
/// </summary>
public static class GameConfig
{
    // ── 이상 감지 확률 (정신력에 따라 변동) ─────────
    /// <summary>정신력 70 이상 또는 미초기화 시 이상 확률 (최소)</summary>
    public const float AbnormalChanceMin = 0.03f;
    /// <summary>정신력 30~70일 때 이상 확률 (중간)</summary>
    public const float AbnormalChanceMid = 0.08f;
    /// <summary>정신력 30 미만일 때 이상 확률 (최대)</summary>
    public const float AbnormalChanceMax = 0.20f;

    // ── 폴더 깊이 ────────────────────────────────
    /// <summary>드래그 이동 허용 최대 폴더 깊이</summary>
    public const int MaxFolderDepth = 6;

    // ── 로그창 ───────────────────────────────────
    /// <summary>로그창 최대 표시 줄 수</summary>
    public const int LogMaxLines = 50;
}
