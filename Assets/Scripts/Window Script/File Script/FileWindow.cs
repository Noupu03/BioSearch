using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ������ ������ ǥ���ϰ� Ž���ϴ� UI â
/// </summary>
public class FileWindow : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject folderIconPrefab;
    public GameObject txtIconPrefab;
    public GameObject pngIconPrefab;

    [Header("Scroll Area")]
    public Transform contentArea;
    public TMP_Text emptyText;

    [Header("Path Panel")]
    public PathPanelManager pathPanelManager;

    [Header("Back Button")]
    public Button backButton;

    private FolderIcon selectedFolderIcon;
    private FileIcon selectedFileIcon;

    private Folder rootFolder;
    private Folder currentFolder;
    private Stack<Folder> folderHistory = new Stack<Folder>();

    // ���� ���� ���� ���ϵ�
    private List<File> currentFolderFiles = new List<File>();

    void Awake()
    {
        if (pathPanelManager != null)
            pathPanelManager.Initialize(this);
    }

    void Start()
    {
        // ���� ���� ����
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

        // �׽�Ʈ�� ���� �߰�
        currentFolderFiles.Add(new File("Readme", "txt", rootFolder));
        currentFolderFiles.Add(new File("ImageSample", "png", rootFolder));

        // �̻� ���� Ȯ�� ���� (0~1)
        AssignAbnormalParameters(rootFolder);

        // Back ��ư
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(true);
        }

        OpenFolder(rootFolder, false);
    }

    // �� �������� abnormalParameter�� �����ϰ�, Ȯ���� ���� isAbnormal ����
    void AssignAbnormalParameters(Folder folder)
    {
        foreach (var child in folder.children)
        {
            child.abnormalParameter = 0.1f; // 10% Ȯ��
            child.AssignAbnormalByParameter();

            // ��� ȣ��, ���� ������ ���������� ����
            AssignAbnormalParameters(child);
        }
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public void OpenFolder(Folder folder, bool recordPrevious = true)
    {
        if (recordPrevious && currentFolder != null && folder != currentFolder)
            folderHistory.Push(currentFolder);

        currentFolder = folder;

        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        selectedFolderIcon = null;
        selectedFileIcon = null;

        bool hasContent = (folder.children.Count > 0) || HasFilesInFolder(folder);
        emptyText.gameObject.SetActive(!hasContent);

        // ���� ������ ����
        foreach (Folder child in folder.children)
        {
            GameObject iconObj = Instantiate(folderIconPrefab, contentArea);
            FolderIcon icon = iconObj.GetComponent<FolderIcon>();
            icon.Setup(child, this, child.isAbnormal);
        }

        // ���� ������ ����
        foreach (File file in currentFolderFiles)
        {
            if (file.parent != folder) continue;

            GameObject prefab = null;

            if (file.extension == "txt")
                prefab = txtIconPrefab;
            else if (file.extension == "png")
                prefab = pngIconPrefab;

            if (prefab == null) continue;

            GameObject iconObj = Instantiate(prefab, contentArea);
            FileIcon icon = iconObj.GetComponent<FileIcon>();
            icon.Setup(file, this);
        }

        if (backButton != null)
            backButton.gameObject.SetActive(true);

        if (pathPanelManager != null)
            pathPanelManager.UpdatePathButtons();
    }

    private bool HasFilesInFolder(Folder folder)
    {
        foreach (var f in currentFolderFiles)
        {
            if (f.parent == folder) return true;
        }
        return false;
    }

    public void SetSelectedIcon(FolderIcon icon)
    {
        if (selectedFolderIcon != null)
            selectedFolderIcon.SetSelected(false);

        selectedFolderIcon = icon;
        selectedFolderIcon.SetSelected(true);
    }

    public void SetSelectedFileIcon(FileIcon icon)
    {
        if (selectedFileIcon != null)
            selectedFileIcon.SetSelected(false);

        selectedFileIcon = icon;
        selectedFileIcon.SetSelected(true);
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
    public void RefreshFolder(Folder folder)
    {
        OpenFolder(folder, false); // �����ִ� ���� �ٽ� ǥ��
    }
    public void RefreshWindow()
    {
        // ���� ���� �ִ� ���� UI�� �ٽ� �׸���
        if (currentFolder != null)
        {
            OpenFolder(currentFolder, false);
        }
    }
}
