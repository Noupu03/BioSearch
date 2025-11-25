using System.Collections.Generic;

/// <summary>
/// FileWindow의 경로 이동 및 네비게이션 관련 메서드 모음.
/// </summary>
public partial class FileWindow
{
    /// <summary>
    /// 현재 폴더까지의 경로를 리스트로 반환
    /// </summary>
    public List<Folder> GetCurrentPathList()
    {
        List<Folder> pathList = new List<Folder>();
        Folder temp = currentFolder;
        while (temp != null)
        {
            pathList.Insert(0, temp);
            temp = temp.parent;
        }
        return pathList;
    }

    /// <summary>
    /// 경로 패널에서 특정 인덱스로 이동
    /// </summary>
    public void NavigateToPathIndex(int index)
    {
        var pathList = GetCurrentPathList();
        if (index < 0 || index >= pathList.Count) return;
        OpenFolder(pathList[index], false);
    }

    /// <summary>
    /// 뒤로가기 버튼 클릭 시 실행
    /// </summary>
    private void OnBackButtonClicked()
    {
        if (folderHistory.Count == 0) return;
        Folder previous = folderHistory.Pop();
        OpenFolder(previous, false);
    }

    /// <summary>
    /// 특정 폴더 갱신
    /// </summary>
    public void RefreshFolder(Folder folder)
    {
        OpenFolder(folder, false);
    }

    /// <summary>
    /// 현재 폴더 갱신
    /// </summary>
    public void RefreshWindow()
    {
        if (currentFolder != null)
            OpenFolder(currentFolder, false);
    }

    /// <summary>
    /// 폴더 이름으로 폴더 검색
    /// </summary>
    private Folder FindFolderByName(Folder folder, string name)
    {
        if (folder.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            return folder;

        foreach (var child in folder.children)
        {
            var found = FindFolderByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
    /// <summary>
    /// 파일 이름으로 FileData 검색
    /// </summary>
    public File FindFileByName(string fileName)
    {
        if (rootFolder == null) return null;
        return SearchFileRecursive(rootFolder, fileName);
    }

    private File SearchFileRecursive(Folder folder, string fileName)
    {
        // 1) 현재 폴더의 파일들 검사
        if (folder.files != null)
        {
            foreach (var file in folder.files)
            {
                if (file.name.Equals(fileName, System.StringComparison.OrdinalIgnoreCase))
                    return file;
            }
        }

        // 2) 하위 폴더 재귀 검사
        if (folder.children != null)
        {
            foreach (var child in folder.children)
            {
                var result = SearchFileRecursive(child, fileName);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

}
