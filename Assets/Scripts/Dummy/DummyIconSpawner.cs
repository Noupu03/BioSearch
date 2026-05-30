using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 정신력에 비례한 확률로 더미 아이콘을 생성한다.
/// FileWindow는 Instance로 접근하므로 인스펙터 크로스 참조가 없다.
/// </summary>
public class DummyIconSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1f;

    private float timer;

    void Update()
    {
        var fw = FileWindow.Instance;
        if (fw == null) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        if (Random.value > SpawnChanceBySanity()) return;

        var allItems = fw.GetAllFilesAndFolders();
        if (allItems.Count == 0) return;

        object original = allItems[Random.Range(0, allItems.Count)];
        bool   isFolder = original is Folder;

        var allFolders = new List<Folder>();
        CollectFolders(fw.GetRootFolder(), allFolders);
        if (allFolders.Count == 0) return;

        Folder targetFolder = allFolders[Random.Range(0, allFolders.Count)];
        string dummyName    = isFolder ? ((Folder)original).name : ((File)original).name;

        // AddDummyIcon이 목록 추가 + UI 생성을 모두 처리
        fw.AddDummyIcon(new DummyIcon
        {
            name         = dummyName,
            isFolder     = isFolder,
            parentFolder = targetFolder
        });
    }

    private static float SpawnChanceBySanity()
    {
        float s = SanityManager.currentSanityStatic;
        if (s <= 30f) return 0.4f;
        if (s <= 70f) return 0.2f;
        return 0.1f;
    }

    private static void CollectFolders(Folder folder, List<Folder> list)
    {
        if (folder == null) return;
        list.Add(folder);
        foreach (var child in folder.children)
            CollectFolders(child, list);
    }
}
