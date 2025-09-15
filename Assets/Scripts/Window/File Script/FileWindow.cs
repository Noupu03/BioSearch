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
        // PathPanelManager ���� �ʱ�ȭ
        if (pathPanelManager != null)
            pathPanelManager.Initialize(this);
    }

    void Start()
    {
        // ���� ����
        rootFolder = new Folder("Root");
        Folder head = new Folder("head", rootFolder);
        head.children.Add(new Folder("mouse", head));
        head.children.Add(new Folder("left eye", head));
        head.children.Add(new Folder("right eye", head));
        head.children.Add(new Folder("nose", head));

        Folder body = new Folder("body", rootFolder);

        Folder organ = new Folder("organ", rootFolder);
        organ.children.Add(new Folder("heart", organ));

        Folder left_arm = new Folder("left arm", rootFolder);
        left_arm.children.Add(new Folder("left hand", left_arm));
        Folder right_arm = new Folder("right arm", rootFolder);
        right_arm.children.Add(new Folder("right hand", right_arm));

        Folder left_leg = new Folder("left leg", rootFolder);
        left_leg.children.Add(new Folder("left foot", left_leg));
        Folder right_leg = new Folder("right leg", rootFolder);
        right_leg.children.Add(new Folder("right foot", right_leg));


        rootFolder.children.Add(head);
        rootFolder.children.Add(body);
        rootFolder.children.Add(organ);
        rootFolder.children.Add(left_arm);
        rootFolder.children.Add(right_arm);
        rootFolder.children.Add(left_leg);
        rootFolder.children.Add(right_leg);

        // �̻� ���� ���� ���� + ���� ���� ���
        PickAbnormalFolderRecursive(rootFolder);

        // Back ��ư �׻� Ȱ��ȭ
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
        if (backButton != null)
            backButton.gameObject.SetActive(true);

        OpenFolder(rootFolder, false);
    }

    void PickAbnormalFolderRecursive(Folder folder)
    {
        if (folder.children.Count == 0)
        {
            folder.isAbnormal = Random.value < 0.1f; // 10% Ȯ��
            return;
        }
        int index = Random.Range(0, folder.children.Count);
        folder.children[index].isAbnormal = true;
        foreach (var child in folder.children)
            PickAbnormalFolderRecursive(child);
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
            icon.Setup(child, this, folder.isAbnormal);
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
