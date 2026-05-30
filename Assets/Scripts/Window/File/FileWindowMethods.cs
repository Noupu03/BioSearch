using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


/// <summary>
/// FileWindow�� UI ���� ���� �޼��� ����.
/// </summary>
public partial class FileWindow
{
    /// <summary>
    /// ���� ���� �� UI ����
    /// </summary>
    /// <summary>
    /// ���� ���� �� UI ���� (���� ��� + ���� ������ ǥ��)
    /// </summary>
    public void OpenFolder(Folder folder, bool recordPrevious = true)
    {
        // ���� ���� ���
        if (recordPrevious && currentFolder != null && folder != currentFolder)
            folderHistory.Push(currentFolder);

        currentFolder = folder;

        // ������ ���� �ʱ�ȭ
        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        selectedFolderIcon = null;
        selectedFileIcon = null;

        // ������ ���� Ȯ��
        bool hasContent = (folder.children.Count > 0) || HasFilesInFolder(folder);
        emptyText.gameObject.SetActive(!hasContent);

        // ---------------------------------------------
        // "..." ���� ���� �̵� ��ư ���� (��Ʈ ���������� ���� �� ��)
        // ---------------------------------------------
        if (upButtonPrefab != null && folder.parent != null)
        {
            GameObject upObj = Instantiate(upButtonPrefab, contentArea);
            Button upButton = upObj.GetComponent<Button>();
            if (upButton != null)
            {
                upButton.onClick.AddListener(() =>
                {
                    // Ŭ�� �� �θ� ������ �̵�
                    OpenFolder(folder.parent, true);
                });
            }
        }

        // ---------------------------------------------
        // ���� ������ ����
        // ---------------------------------------------
        foreach (Folder child in folder.children)
        {
            GameObject iconObj = Instantiate(folderIconPrefab, contentArea);
            FolderIcon icon = iconObj.GetComponent<FolderIcon>();
            icon.Setup(child, this, child.isAbnormal);
        }

        // ---------------------------------------------
        // ���� ������ ����
        // ---------------------------------------------
        foreach (File file in currentFolderFiles)
        {
            if (file.parent != folder) continue;
            GameObject iconObj = Instantiate(fileIconPrefab, contentArea);
            FileIcon icon = iconObj.GetComponent<FileIcon>();
            icon.Setup(file, this);
        }

        // ---------------------------------------------
        // ���� ������ ���� (���� ������ ���� �͸�)
        // ---------------------------------------------
        CreateDummyIconsForFolder(folder);

        // �ڷΰ��� ��ư Ȱ��ȭ
        if (backButton != null)
            backButton.gameObject.SetActive(true);

        // ��� �г� ������Ʈ
        pathPanelManager?.UpdatePathButtons();
    }

    /// <summary>
    /// ���� ������ ���� ���� �����ܸ� UI�� ����
    /// </summary>
    private void CreateDummyIconsForFolder(Folder folder)
    {
        foreach (var dummy in dummyIcons)
        {
            // �ش� ������ ���� ���� �� ���� UI�� �������� ���� �͸� ����
            if (dummy.parentFolder == folder && dummy.uiObject == null)
            {
                CreateDummyIconUI(dummy, folder);
            }
        }
    }

  

    /// <summary>
    /// �ش� ������ ������ �����ϴ��� Ȯ��
    /// </summary>
    private bool HasFilesInFolder(Folder folder)
    {
        foreach (var f in currentFolderFiles)
        {
            if (f.parent == folder) return true;
        }
        return false;
    }

    /// <summary>
    /// ���� ������ ���� ���� ����
    /// </summary>
    public void SetSelectedIcon(FolderIcon icon)
    {
        // ���� ���� ����
        selectedFolderIcon?.SetSelected(false);
        selectedFileIcon?.SetSelected(false);
        selectedFileIcon = null;

        // ���� ����
        selectedFolderIcon = icon;
        selectedFolderIcon?.SetSelected(true);
    }

    /// <summary>
    /// ���� ������ ���� ���� ����
    /// </summary>
    public void SetSelectedFileIcon(FileIcon icon)
    {
        // ���� ���� ����
        selectedFileIcon?.SetSelected(false);
        selectedFolderIcon?.SetSelected(false);
        selectedFolderIcon = null;

        // ���� ����
        selectedFileIcon = icon;
        selectedFileIcon?.SetSelected(true);
    }
    public List<object> GetAllFilesAndFolders()//���� ����
    {
        List<object> result = new List<object>();
        AddFolderAndFilesRecursive(rootFolder, result);
        return result;
    }

    private void AddFolderAndFilesRecursive(Folder folder, List<object> list)//���� ����
    {
        list.Add(folder);
        foreach (var file in folder.files) list.Add(file);
        foreach (var child in folder.children) AddFolderAndFilesRecursive(child, list);
    }

    /// <summary>
    /// ���� ���ŷ¿� ���� �̻� Ȯ�� ��ȯ
    /// </summary>
    private float GetAbnormalProbabilityBySanity()
    {
        float sanity = SanityManager.currentSanityStatic;

        // 0이면 SanityManager가 아직 초기화되지 않은 것 → 최소 확률 사용
        if (sanity <= 0f) return 0.03f;

        if (sanity >= 70f) return 0.03f;
        else if (sanity >= 30f) return 0.08f;
        else return 0.2f;
    }
}