// DummyIconSpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class DummyIconSpawner : MonoBehaviour
{
    public FileWindow fileWindow;
    public float spawnInterval = 1f;

    private float timer = 0f;

    void Update()
    {
        if (fileWindow == null) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        float chance = 0.7f; // 70% Ȯ��
        if (Random.value < chance) return;

        List<object> allItems = fileWindow.GetAllFilesAndFolders();
        if (allItems.Count == 0) return;

        object original = allItems[Random.Range(0, allItems.Count)];
        bool isFolder = original is Folder;

        // ��� ���� ����
        List<Folder> allFolders = new List<Folder>();
        CollectAllFolders(fileWindow.GetRootFolder(), allFolders);
        if (allFolders.Count == 0) return;

        // ���� ���� ����
        Folder targetFolder = allFolders[Random.Range(0, allFolders.Count)];

        // DummyIcon ���� �� ����Ʈ�� �߰�
        FileWindow.DummyIcon dummy = new FileWindow.DummyIcon
        {
            name = (isFolder ? ((Folder)original).name : ((File)original).name) + "_dummy",
            isFolder = isFolder,
            parentFolder = targetFolder
        };

        fileWindow.dummyIcons.Add(dummy);

        // ���� �����ִ� ������ ������ UI ����
        if (fileWindow.GetCurrentFolder() == targetFolder)
            fileWindow.CreateDummyIconUI(dummy, targetFolder);
    }

    private void CollectAllFolders(Folder folder, List<Folder> list)
    {
        if (folder == null) return;
        list.Add(folder);
        foreach (var child in folder.children)
            CollectAllFolders(child, list);
    }
}
