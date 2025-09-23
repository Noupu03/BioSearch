using UnityEngine;

/// <summary>
/// scan ��ɾ ó���ϴ� Ŭ����
/// - Ư�� ���� Ž��
/// - �̻� ���� ���� Ȯ��
/// </summary>
public class ScanCommandManager : MonoBehaviour
{
    [Header("References")]
    public FileWindow fileWindow;          // ��ĵ�� ���� ����
    public LogWindowManager logWindow;     // �α� ��� â

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

    /// <summary>
    /// scan ��ɾ� �Է� �� ȣ��Ǵ� ó�� �Լ�
    /// </summary>
    private void HandleScanCommand(string fileName)
    {
        ScanFile(fileName);
    }

    /// <summary>
    /// Ư�� ������ �˻� �� �̻� ���� �˻�
    /// </summary>
    private void ScanFile(string fileName)
    {
        if (fileWindow == null)
        {
            logWindow.Log("FileWindow ������ �����ϴ�.");
            return;
        }

        Folder root = fileWindow.GetRootFolder();
        Folder target = FindFolderByName(root, fileName);

        if (target == null)
        {
            logWindow.Log($"���� '{fileName}'��(��) ã�� �� �����ϴ�.");
            return;
        }

        int abnormalCount = CountAbnormalFoldersRecursive(target);

        if (abnormalCount > 0)
            logWindow.Log($"{abnormalCount}���� �̻� ������.");
        else
            logWindow.Log("�̻� ����.");
    }

    /// <summary>
    /// ���� �̸����� ��� �˻�
    /// </summary>
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

    /// <summary>
    /// �̻� ���� ������ ��������� ���
    /// </summary>
    private int CountAbnormalFoldersRecursive(Folder folder)
    {
        int count = folder.isAbnormal ? 1 : 0;
        foreach (var child in folder.children)
            count += CountAbnormalFoldersRecursive(child);
        return count;
    }
}

/// <summary>
/// FileWindow Ŭ������ ��Ʈ ���� ���� Ȯ�� �޼���
/// </summary>
public static class FileWindowExtensions
{
    public static Folder GetRootFolder(this FileWindow window)
    {
        var field = typeof(FileWindow).GetField("rootFolder",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field.GetValue(window) as Folder;
    }
}
