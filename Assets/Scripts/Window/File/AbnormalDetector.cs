/// <summary>
/// 폴더/파일 트리에서 이상 여부를 탐색하는 유틸리티.
/// </summary>
public static class AbnormalDetector
{
    /// <summary>트리 내 이상 폴더+파일 총 개수</summary>
    public static int GetAbnormalCount(Folder folder)
    {
        if (folder == null) return 0;

        int count = folder.isAbnormal ? 1 : 0;

        foreach (var child in folder.children)
            count += GetAbnormalCount(child);

        foreach (var file in folder.files)
            if (file.isAbnormal) count++;

        return count;
    }

    /// <summary>트리 내 이상 항목이 하나라도 있는지 여부</summary>
    public static bool HasAbnormal(Folder folder)
    {
        if (folder == null) return false;
        if (folder.isAbnormal) return true;

        foreach (var file in folder.files)
            if (file.isAbnormal) return true;

        foreach (var child in folder.children)
            if (HasAbnormal(child)) return true;

        return false;
    }
}
