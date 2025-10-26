using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


/// <summary>
/// FileWindow의 UI 동작 관련 메서드 모음.
/// </summary>
public partial class FileWindow
{
    /// <summary>
    /// 폴더 열기 및 UI 갱신
    /// </summary>
    /// <summary>
    /// 폴더 열기 및 UI 갱신 (기존 기능 + 더미 아이콘 표시)
    /// </summary>
    public void OpenFolder(Folder folder, bool recordPrevious = true)
    {
        // 이전 폴더 기록
        if (recordPrevious && currentFolder != null && folder != currentFolder)
            folderHistory.Push(currentFolder);

        currentFolder = folder;

        // 콘텐츠 영역 초기화
        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        selectedFolderIcon = null;
        selectedFileIcon = null;

        // 콘텐츠 유무 확인
        bool hasContent = (folder.children.Count > 0) || HasFilesInFolder(folder);
        emptyText.gameObject.SetActive(!hasContent);

        // ---------------------------------------------
        // "..." 상위 폴더 이동 버튼 생성 (루트 폴더에서는 생성 안 함)
        // ---------------------------------------------
        if (upButtonPrefab != null && folder.parent != null)
        {
            GameObject upObj = Instantiate(upButtonPrefab, contentArea);
            Button upButton = upObj.GetComponent<Button>();
            if (upButton != null)
            {
                upButton.onClick.AddListener(() =>
                {
                    // 클릭 시 부모 폴더로 이동
                    OpenFolder(folder.parent, true);
                });
            }
        }

        // ---------------------------------------------
        // 폴더 아이콘 생성
        // ---------------------------------------------
        foreach (Folder child in folder.children)
        {
            GameObject iconObj = Instantiate(folderIconPrefab, contentArea);
            FolderIcon icon = iconObj.GetComponent<FolderIcon>();
            icon.Setup(child, this, child.isAbnormal);
        }

        // ---------------------------------------------
        // 파일 아이콘 생성
        // ---------------------------------------------
        foreach (File file in currentFolderFiles)
        {
            if (file.parent != folder) continue;
            GameObject iconObj = Instantiate(fileIconPrefab, contentArea);
            FileIcon icon = iconObj.GetComponent<FileIcon>();
            icon.Setup(file, this);
        }

        // ---------------------------------------------
        // 더미 아이콘 생성 (현재 폴더에 속한 것만)
        // ---------------------------------------------
        CreateDummyIconsForFolder(folder);

        // 뒤로가기 버튼 활성화
        if (backButton != null)
            backButton.gameObject.SetActive(true);

        // 경로 패널 업데이트
        pathPanelManager?.UpdatePathButtons();
    }

    /// <summary>
    /// 현재 폴더에 속한 더미 아이콘만 UI로 생성
    /// </summary>
    private void CreateDummyIconsForFolder(Folder folder)
    {
        foreach (var dummy in dummyIcons)
        {
            // 해당 폴더에 속한 더미 중 아직 UI가 생성되지 않은 것만 생성
            if (dummy.parentFolder == folder && dummy.uiObject == null)
            {
                CreateDummyIconUI(dummy, folder);
            }
        }
    }

  

    /// <summary>
    /// 해당 폴더에 파일이 존재하는지 확인
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
    /// 폴더 아이콘 선택 상태 갱신
    /// </summary>
    public void SetSelectedIcon(FolderIcon icon)
    {
        // 기존 선택 해제
        selectedFolderIcon?.SetSelected(false);
        selectedFileIcon?.SetSelected(false);
        selectedFileIcon = null;

        // 새로 선택
        selectedFolderIcon = icon;
        selectedFolderIcon?.SetSelected(true);
    }

    /// <summary>
    /// 파일 아이콘 선택 상태 갱신
    /// </summary>
    public void SetSelectedFileIcon(FileIcon icon)
    {
        // 기존 선택 해제
        selectedFileIcon?.SetSelected(false);
        selectedFolderIcon?.SetSelected(false);
        selectedFolderIcon = null;

        // 새로 선택
        selectedFileIcon = icon;
        selectedFileIcon?.SetSelected(true);
    }
    public List<object> GetAllFilesAndFolders()//더미 관련
    {
        List<object> result = new List<object>();
        AddFolderAndFilesRecursive(rootFolder, result);
        return result;
    }

    private void AddFolderAndFilesRecursive(Folder folder, List<object> list)//더미 관련
    {
        list.Add(folder);
        foreach (var file in folder.files) list.Add(file);
        foreach (var child in folder.children) AddFolderAndFilesRecursive(child, list);
    }

    /// <summary>
    /// 현재 정신력에 따라 이상 확률 반환
    /// </summary>
    private float GetAbnormalProbabilityBySanity()
    {
        float sanity = SanityManager.currentSanityStatic;

        if (sanity >= 70f) return 0.03f;   // 고 sanity 구간
        else if (sanity >= 30f) return 0.08f; // 중간 sanity 구간
        else return 0.2f; // 낮은 sanity 구간
    }
}