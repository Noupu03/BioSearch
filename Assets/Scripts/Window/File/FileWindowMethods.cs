using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// FileWindow UI 렌더링 관련 메서드 모음.
/// </summary>
public partial class FileWindow
{
    public void OpenFolder(Folder folder, bool recordPrevious = true)
    {
        if (recordPrevious && currentFolder != null && folder != currentFolder)
            folderHistory.Push(currentFolder);

        currentFolder = folder;

        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        selectedFolderIcon = null;
        selectedFileIcon   = null;

        bool hasContent = (folder.children.Count > 0) || HasFilesInFolder(folder);
        emptyText.gameObject.SetActive(!hasContent);

        // 루트가 아닐 때 상위 폴더 이동 버튼 생성
        if (upButtonPrefab != null && folder.parent != null)
        {
            var upObj    = Instantiate(upButtonPrefab, contentArea);
            var upButton = upObj.GetComponent<Button>();
            if (upButton != null)
                upButton.onClick.AddListener(() => OpenFolder(folder.parent, true));
        }

        foreach (Folder child in folder.children)
        {
            var icon = Instantiate(folderIconPrefab, contentArea).GetComponent<FolderIcon>();
            icon.Setup(child, this, child.isAbnormal);
        }

        foreach (File file in currentFolderFiles)
        {
            if (file.parent != folder) continue;
            var icon = Instantiate(fileIconPrefab, contentArea).GetComponent<FileIcon>();
            icon.Setup(file, this);
        }

        CreateDummyIconsForFolder(folder);

        if (backButton != null)
            backButton.gameObject.SetActive(true);

        pathPanelManager?.UpdatePathButtons();
    }

    private void CreateDummyIconsForFolder(Folder folder)
    {
        foreach (var dummy in dummyIcons)
        {
            if (dummy.parentFolder == folder && dummy.uiObject == null)
                CreateDummyIconUI(dummy, folder);
        }
    }

    private bool HasFilesInFolder(Folder folder)
    {
        foreach (var f in currentFolderFiles)
            if (f.parent == folder) return true;
        return false;
    }

    public void SetSelectedIcon(FolderIcon icon)
    {
        selectedFolderIcon?.SetSelected(false);
        selectedFileIcon?.SetSelected(false);
        selectedFileIcon   = null;
        selectedFolderIcon = icon;
        selectedFolderIcon?.SetSelected(true);
    }

    public void SetSelectedFileIcon(FileIcon icon)
    {
        selectedFileIcon?.SetSelected(false);
        selectedFolderIcon?.SetSelected(false);
        selectedFolderIcon = null;
        selectedFileIcon   = icon;
        selectedFileIcon?.SetSelected(true);
    }

    public List<object> GetAllFilesAndFolders()
    {
        var result = new List<object>();
        AddFolderAndFilesRecursive(rootFolder, result);
        return result;
    }

    private void AddFolderAndFilesRecursive(Folder folder, List<object> list)
    {
        list.Add(folder);
        foreach (var file in folder.files) list.Add(file);
        foreach (var child in folder.children) AddFolderAndFilesRecursive(child, list);
    }

    private static float GetAbnormalProbabilityBySanity()
    {
        float sanity = SanityManager.currentSanityStatic;

        // 미초기화(0) 또는 양호(70 이상): 최소 확률
        if (sanity <= 0f || sanity >= 70f) return GameConfig.AbnormalChanceMin;
        if (sanity >= 30f)                 return GameConfig.AbnormalChanceMid;
        return                             GameConfig.AbnormalChanceMax;
    }
}
