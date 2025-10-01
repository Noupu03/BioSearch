using System.Collections;
using UnityEngine;

public class ScanCommandManager : MonoBehaviour
{
    public static ScanCommandManager Instance;

    public FileWindow fileWindow;
    public LogWindowManager logWindow;

    private bool isScanning = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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

    private void HandleScanCommand(string folderName)
    {
        if (isScanning)
        {
            logWindow.Log("��ĵ ���Դϴ�...");
            return;
        }

        Folder root = fileWindow.GetRootFolder();
        Folder target = FindFolderByName(root, folderName);

        if (target == null)
        {
            logWindow.Log($"���� '{folderName}'��(��) ã�� �� �����ϴ�.");
            return;
        }

        StartCoroutine(ScanFolderCoroutine(target));
    }

    private IEnumerator ScanFolderCoroutine(Folder folder)
    {
        isScanning = true;
        logWindow.DisableInput(); // �Է� ��Ȱ��ȭ


        int totalItems = CountAllFilesAndFolders(folder); // ���� + ���� ����
        int progressBarLength = 10;

        float timePerBar = totalItems * 1f; // 1�� * totalItems �� �� ĭ ���� �ð�
        int abnormalCount = CountAbnormal(folder);

        // �ʱ� �� ���� �� ��� (�� �α�)
        logWindow.Log($"�̻� ��ĵ�� {new string('��', progressBarLength)}");

        for (int i = 0; i < progressBarLength; i++)
        {
            yield return new WaitForSeconds(timePerBar); // �� ĭ�� ��ٸ�

            // �� ĭ ������ �����
            string progress = new string('��', i + 1) + new string('��', progressBarLength - i - 1);
            logWindow.ReplaceLastScanLog($"�̻� ��ĵ�� {progress}");
        }

        logWindow.ReplaceLastScanLog($"��ĵ �Ϸ�: �̻� {abnormalCount}�� �߰ߵ�.");
 

        logWindow.EnableInput(); // �Է� ��Ȱ��ȭ
        isScanning = false;
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

    private int CountAllFilesAndFolders(Folder folder)
    {
        int count = 1 + folder.files.Count;
        foreach (var child in folder.children)
            count += CountAllFilesAndFolders(child);
        return count;
    }

    private int CountAbnormal(Folder folder)
    {
        int count = folder.isAbnormal ? 1 : 0;
        foreach (var child in folder.children)
            count += CountAbnormal(child);
        foreach (var file in folder.files)
            if (file.isAbnormal) count++;
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
