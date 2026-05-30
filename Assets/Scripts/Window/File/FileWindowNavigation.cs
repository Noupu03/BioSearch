using System.Collections.Generic;

/// <summary>
/// FileWindow 경로 이동 및 탐색 관련 메서드 모음.
/// </summary>
public partial class FileWindow
{
    /// <summary>현재 경로를 루트부터 순서대로 반환</summary>
    public List<Folder> GetCurrentPathList()
    {
        var pathList = new List<Folder>();
        Folder temp = currentFolder;
        while (temp != null)
        {
            pathList.Insert(0, temp);
            temp = temp.parent;
        }
        return pathList;
    }

    /// <summary>경로 패널의 특정 인덱스로 이동</summary>
    public void NavigateToPathIndex(int index)
    {
        var pathList = GetCurrentPathList();
        if (index < 0 || index >= pathList.Count) return;
        OpenFolder(pathList[index], false);
    }

    private void OnBackButtonClicked()
    {
        if (folderHistory.Count == 0) return;
        OpenFolder(folderHistory.Pop(), false);
    }

    public void RefreshFolder(Folder folder) => OpenFolder(folder, false);

    public void RefreshWindow()
    {
        if (currentFolder != null)
            OpenFolder(currentFolder, false);
    }

    /// <summary>루트 트리에서 이름으로 폴더 탐색 (대소문자 무시)</summary>
    public Folder FindFolder(string name) => FindFolderByName(rootFolder, name);

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
}
