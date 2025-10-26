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

        // ���ŷ� ��� Ȯ�� ���
        float chance = GetDummySpawnProbabilityBySanity();
        if (Random.value > chance) return;

        Debug.Log("����");
        Debug.Log($"chance : {chance}");


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
        DummyIcon dummy = new DummyIcon
        {
            name = (isFolder ? ((Folder)original).name : ((File)original).name),
            isFolder = isFolder,
            parentFolder = targetFolder
        };

        fileWindow.dummyIcons.Add(dummy);

        // ���� �����ִ� ������ ������ UI ����
        if (fileWindow.GetCurrentFolder() == targetFolder)
            fileWindow.CreateDummyIconUI(dummy, targetFolder);
    }

    /// <summary>
    /// ���ŷ¿� ���� Dummy ������ ���� Ȯ�� ��ȯ
 
    /// </summary>
    private float GetDummySpawnProbabilityBySanity()
    {
        float sanity = SanityManager.currentSanityStatic;

        if (sanity <= 30f) return 0.4f;
        else if (sanity <= 70f) return 0.2f;
        else return 0.1f;
    }


    private void CollectAllFolders(Folder folder, List<Folder> list)
    {
        if (folder == null) return;
        list.Add(folder);
        foreach (var child in folder.children)
            CollectAllFolders(child, list);
    }
}
