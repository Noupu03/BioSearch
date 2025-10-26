using UnityEngine;

/// <summary>
/// ���� ���ھ�(����/����/�������� ���൵)�� �����ϴ� ���� Ŭ����.
/// �� Reload �Ŀ��� ���� �����ǵ��� static���� ����.
/// </summary>
public static class ScoreCount
{
    public static int successCount = 0;
    public static int failCount = 0;
    public static int stageCount = 1;

    /// <summary>
    /// ��� ��� �ʱ�ȭ
    /// </summary>
    public static void Reset()
    {
        successCount = 0;
        failCount = 0;
        stageCount = 1;
    }
}
