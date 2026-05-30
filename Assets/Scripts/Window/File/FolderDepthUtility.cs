public static class FolderDepthUtility
{
    public static int GetFolderDepth(Folder folder)
    {
        int depth = 1;
        Folder temp = folder;
        while (temp.parent != null) { depth++; temp = temp.parent; }
        return depth;
    }

    public static int GetSubtreeMaxDepth(Folder folder)
    {
        int max = 0;
        foreach (var child in folder.children)
        {
            int d = 1 + GetSubtreeMaxDepth(child);
            if (d > max) max = d;
        }
        return max;
    }

    public static bool CanMove(Folder dragged, Folder target, out string warning)
    {
        int newDepth = GetFolderDepth(target) + 1 + GetSubtreeMaxDepth(dragged);
        if (newDepth > GameConfig.MaxFolderDepth)
        {
            warning = $"최대 폴더 깊이({GameConfig.MaxFolderDepth})를 초과합니다.";
            return false;
        }
        warning = null;
        return true;
    }
}
