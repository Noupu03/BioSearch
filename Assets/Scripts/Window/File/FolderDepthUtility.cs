using System.Collections.Generic;
using UnityEngine;

public static class FolderDepthUtility
{
    // 현재 폴더 깊이 계산 (루트 포함, 루트가 depth 1)
    public static int GetFolderDepth(Folder folder)
    {
        int depth = 1; // 루트 시작
        Folder temp = folder;
        while (temp.parent != null)
        {
            depth++;
            temp = temp.parent;
        }
        return depth;
    }

    // 하위 트리의 최대 깊이 계산 (자기 자신 포함 안 함)
    public static int GetSubtreeMaxDepth(Folder folder)
    {
        if (folder.children == null || folder.children.Count == 0)
            return 0;

        int maxDepth = 0;
        foreach (var child in folder.children)
        {
            int childDepth = 1 + GetSubtreeMaxDepth(child);
            if (childDepth > maxDepth)
                maxDepth = childDepth;
        }
        return maxDepth;
    }

    // 이동 시 새로운 깊이를 계산
    public static bool CanMove(Folder dragged, Folder target, out string warningMessage)
    {
        int targetDepth = GetFolderDepth(target);              // 대상 폴더 깊이
        int subtreeDepth = 1 + GetSubtreeMaxDepth(dragged);    // 드래그된 폴더 포함 하위 깊이
        int newDepth = targetDepth + subtreeDepth;

        if (newDepth > 6)
        {
            warningMessage = "Warning. 최대 깊이(6) 초과.";
            return false;
        }

        warningMessage = null;
        return true;
    }
}
