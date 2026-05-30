/// <summary>
/// 씬 리로드 사이에도 유지되는 스코어 데이터.
/// 외부에서 직접 필드를 쓰는 대신 메서드를 통해 변경한다.
/// </summary>
public static class ScoreCount
{
    public static int SuccessCount { get; private set; } = 0;
    public static int FailCount    { get; private set; } = 0;
    public static int StageCount   { get; private set; } = 1;

    public static void AddSuccess() => SuccessCount++;
    public static void AddFail()    => FailCount++;
    public static void NextStage()  => StageCount++;

    public static void Reset()
    {
        SuccessCount = 0;
        FailCount    = 0;
        StageCount   = 1;
    }
}
