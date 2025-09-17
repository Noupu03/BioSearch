using UnityEngine;

public class ScanCommandManager : MonoBehaviour
{
    [Header("References")]
    public FileWindow fileWindow;          // 스캔할 폴더 구조
    public LogWindowManager logWindow;     // 로그 출력

    private void OnEnable()
    {
        if (logWindow != null)
            logWindow.OnScanCommandEntered += HandleScanCommand;
    }

    private void OnDisable()
    {
        if (logWindow != null)
            logWindow.OnScanCommandEntered -= HandleScanCommand;
    }

    private void HandleScanCommand(string fileName)
    {
        ScanFile(fileName);
    }

    private void ScanFile(string fileName)
    {
        if (fileWindow == null)
        {
            logWindow.Log("FileWindow 참조가 없습니다.");
            return;
        }

        Folder root = fileWindow.GetRootFolder();
        Folder target = FindFolderByName(root, fileName);

        if (target == null)
        {
            logWindow.Log($"폴더 '{fileName}'을(를) 찾을 수 없습니다.");
            return;
        }

        int abnormalCount = CountAbnormalFoldersRecursive(target);

        if (abnormalCount > 0)
            logWindow.Log($"{abnormalCount}개의 이상 감지됨.");
        else
            logWindow.Log("이상 없음.");
    }

    private Folder FindFolderByName(Folder folder, string name)
    {
        if (folder.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            return folder;

        foreach (var child in folder.children)
        {
            var found = FindFolderByName(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private int CountAbnormalFoldersRecursive(Folder folder)
    {
        int count = folder.isAbnormal ? 1 : 0;
        foreach (var child in folder.children)
            count += CountAbnormalFoldersRecursive(child);
        return count;
    }
}

// FileWindow 루트 폴더 접근용 확장
public static class FileWindowExtensions
{
    public static Folder GetRootFolder(this FileWindow window)
    {
        var field = typeof(FileWindow).GetField("rootFolder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field.GetValue(window) as Folder;
    }
}
