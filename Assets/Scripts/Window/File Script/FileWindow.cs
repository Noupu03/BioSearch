using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FileWindow : MonoBehaviour
{
    [Header("Scroll Area")]
    public GameObject fileIconPrefab;   // ������ ������
    public Transform contentArea;       // ScrollView Content
    public TMP_Text emptyText;          // Empty Folder ǥ�ÿ� �ؽ�Ʈ

    [Header("Top Bar")]
    public Button backButton;           // TopBar Back ��ư
    public TMP_Text pathText;           // ���� ��� ǥ��

    private FileIcon selectedIcon;
    private Folder rootFolder;
    private Folder currentFolder;

    private Stack<Folder> folderHistory = new Stack<Folder>();

    void Awake()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
    }

    void Start()
    {
        // ���� ���� ����
        rootFolder = new Folder("Root");

        Folder head = new Folder("head", rootFolder);
        head.children.Add(new Folder("mouse", head));
        head.children.Add(new Folder("left eye", head));
        head.children.Add(new Folder("right eye", head));

        Folder organ = new Folder("organ", rootFolder);
        organ.children.Add(new Folder("heart", organ));

        rootFolder.children.Add(head);
        rootFolder.children.Add(new Folder("body", rootFolder));
        rootFolder.children.Add(new Folder("leg", rootFolder));
        rootFolder.children.Add(new Folder("arm", rootFolder));
        rootFolder.children.Add(new Folder("hand", rootFolder));
        rootFolder.children.Add(organ);

        // ��Ʈ ���� ����
        OpenFolder(rootFolder, false);
    }

    // ���� ����
    public void OpenFolder(Folder folder, bool recordPrevious = true)
    {
        if (recordPrevious && currentFolder != null && folder != currentFolder)
            folderHistory.Push(currentFolder);

        currentFolder = folder;

        // ScrollView Content �ʱ�ȭ
        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        selectedIcon = null;

        // Back ��ư �׻� Ȱ��ȭ
        if (backButton != null)
            backButton.gameObject.SetActive(true);

        // ���� ��� ǥ��
        if (pathText != null)
            pathText.text = GetFullPath(currentFolder);

        // ����ִ� ���� ó��
        emptyText.gameObject.SetActive(folder.children.Count == 0);

        // ������ ����
        foreach (Folder child in folder.children)
        {
            GameObject iconObj = Instantiate(fileIconPrefab, contentArea);
            FileIcon icon = iconObj.GetComponent<FileIcon>();
            icon.Setup(child, this);
        }
    }

    public void SetSelectedIcon(FileIcon icon)
    {
        if (selectedIcon != null)
            selectedIcon.SetSelected(false);

        selectedIcon = icon;
        selectedIcon.SetSelected(true);
    }

    public void OpenSelected()
    {
        if (selectedIcon == null) return;

        Folder folder = selectedIcon.GetFolder();
        OpenFolder(folder);
    }

    private void OnBackButtonClicked()
    {
        // ��Ʈ ���������� �̵����� ����
        if (currentFolder == rootFolder)
            return;

        if (folderHistory.Count > 0)
        {
            if (selectedIcon != null)
                selectedIcon.SetSelected(false);

            Folder previous = folderHistory.Pop();
            OpenFolder(previous, false);
        }
    }

    // ��ü ��� ���ڿ� ��ȯ
    private string GetFullPath(Folder folder)
    {
        List<string> pathList = new List<string>();
        Folder temp = folder;
        while (temp != null)
        {
            pathList.Insert(0, temp.name);
            temp = temp.parent;
        }
        return string.Join(" / ", pathList);
    }
}

// Folder Ŭ����
[System.Serializable]
public class Folder
{
    public string name;
    public List<Folder> children = new List<Folder>();
    public Folder parent;

    public Folder(string name, Folder parent = null)
    {
        this.name = name;
        this.parent = parent;
    }
}
