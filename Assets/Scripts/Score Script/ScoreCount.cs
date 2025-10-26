using UnityEngine;

/// <summary>
/// 게임 스코어(성공/실패/스테이지 진행도)를 관리하는 정적 클래스.
/// 씬 Reload 후에도 값이 유지되도록 static으로 관리.
/// </summary>
public static class ScoreCount
{
    public static int successCount = 0;
    public static int failCount = 0;
    public static int stageCount = 1;

    /// <summary>
    /// 모든 기록 초기화
    /// </summary>
    public static void Reset()
    {
        successCount = 0;
        failCount = 0;
        stageCount = 1;
    }
}
