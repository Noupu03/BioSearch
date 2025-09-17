using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FileWindow : MonoBehaviour
{
    [Header("Scroll Area")]
    public GameObject fileIconPrefab;
    public Transform contentArea;
    public TMP_Text emptyText;

    [Header("Path Panel")]
    public PathPanelManager pathPanelManager;

    [Header("Back Button")]
    public Button backButton;

    private FolderIcon selectedIcon;
    private Folder rootFolder;
    private Folder currentFolder;
    private Stack<Folder> folderHistory = new Stack<Folder>();

    void Awake()
    {
        if (pathPanelManager != null)
            pathPanelManager.Initialize(this);
    }

    void Start()
    {
        // 폴더 구조 생성
        rootFolder = new Folder("Root");
        Folder Head = new Folder("Head", rootFolder);
        Head.children.Add(new Folder("Mouse", Head));
        Head.children.Add(new Folder("LeftEye", Head));
        Head.children.Add(new Folder("RightEye", Head));
        Head.children.Add(new Folder("Nose", Head));

        Folder Body = new Folder("Body", rootFolder);

        Folder Organ = new Folder("Organ", rootFolder);
        Organ.children.Add(new Folder("Heart", Organ));

        Folder LeftArm = new Folder("LeftArm", rootFolder);
        LeftArm.children.Add(new Folder("LeftHand", LeftArm));
        Folder RightArm = new Folder("RightArm", rootFolder);
        RightArm.children.Add(new Folder("RightHand", RightArm));

        Folder LeftLeg = new Folder("LeftLeg", rootFolder);
        LeftLeg.children.Add(new Folder("LeftFoot", LeftLeg));
        Folder RightLeg = new Folder("RightLeg", rootFolder);
        RightLeg.children.Add(new Folder("RightFoot", RightLeg));

        rootFolder.children.Add(Head);
        rootFolder.children.Add(Body);
        rootFolder.children.Add(Organ);
        rootFolder.children.Add(LeftArm);
        rootFolder.children.Add(RightArm);
        rootFolder.children.Add(LeftLeg);
        rootFolder.children.Add(RightLeg);

        // 이상 폴더 확률 설정 (0~1)
        AssignAbnormalParameters(rootFolder);

        // Back 버튼
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(true);
        }

        OpenFolder(rootFolder, false);
    }

    // 각 폴더별로 abnormalParameter를 설정하고, 확률에 따라 isAbnormal 결정
    void AssignAbnormalParameters(Folder folder)
    {
        foreach (var child in folder.children)
        {
            child.abnormalParameter = 0.1f; // 10% 확률
            child.AssignAbnormalByParameter();

            // 재귀 호출, 하위 폴더도 독립적으로 설정
            AssignAbnormalParameters(child);
        }
    }

    public void OpenFolder(Folder folder, bool recordPrevious = true)
    {
        if (recordPrevious && currentFolder != null && folder != currentFolder)
            folderHistory.Push(currentFolder);

        currentFolder = folder;

        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        selectedIcon = null;
        emptyText.gameObject.SetActive(folder.children.Count == 0);

        foreach (Folder child in folder.children)
        {
            GameObject iconObj = Instantiate(fileIconPrefab, contentArea);
            FolderIcon icon = iconObj.GetComponent<FolderIcon>();
            icon.Setup(child, this, child.isAbnormal);
        }

        if (backButton != null)
            backButton.gameObject.SetActive(true);

        if (pathPanelManager != null)
            pathPanelManager.UpdatePathButtons();
    }

    public void SetSelectedIcon(FolderIcon icon)
    {
        if (selectedIcon != null)
            selectedIcon.SetSelected(false);

        selectedIcon = icon;
        selectedIcon.SetSelected(true);
    }

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

    public void NavigateToPathIndex(int index)
    {
        var pathList = GetCurrentPathList();
        if (index < 0 || index >= pathList.Count) return;
        OpenFolder(pathList[index], false);
    }

    private void OnBackButtonClicked()
    {
        if (folderHistory.Count == 0) return;
        Folder previous = folderHistory.Pop();
        OpenFolder(previous, false);
    }
}
