using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public partial class FileWindow : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject folderIconPrefab;
    public GameObject fileIconPrefab;

    [Header("Scroll Area")]
    public Transform contentArea;
    public TMP_Text emptyText;

    [Header("Path Panel")]
    public PathPanelManager pathPanelManager;

    [Header("Back Button")]
    public Button backButton;

    [Header("Inspector File List")]
    public List<FileData> fileDatas = new List<FileData>();

    [Header("Special Prefabs")]
    public GameObject upButtonPrefab;

    private FolderIcon selectedFolderIcon;
    private FileIcon selectedFileIcon;

    private Folder rootFolder;
    private Folder currentFolder;
    private Stack<Folder> folderHistory = new Stack<Folder>();
    private List<File> currentFolderFiles = new List<File>();

    void Awake()
    {
        if (pathPanelManager != null) pathPanelManager.Initialize(this);
    }

    void Start()
    {
        rootFolder = new Folder("Root");
        CreateDefaultFolders();
        InitializeFilesFromInspector();
        AssignAbnormalParameters(rootFolder);

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(true);
        }

        OpenFolder(rootFolder, false);
    }

    private void InitializeFilesFromInspector()
    {
        float abnormalChance = GetAbnormalProbabilityBySanity();
        foreach (var data in fileDatas)
        {
            Folder targetParent = FindFolderByName(rootFolder, data.parentFolderName);
            if (targetParent == null)
            {
                Debug.LogWarning($"부모 폴더 '{data.parentFolderName}'을(를) 찾을 수 없습니다. Root에 추가합니다.");
                targetParent = rootFolder;
            }
            bool randomAbnormal = Random.value < abnormalChance;
            File file = new File(
                data.fileName,
                data.extension,
                targetParent,
                data.textContent,
                data.imageContent,
                data.isAbnormal || randomAbnormal
            );
            currentFolderFiles.Add(file);
            targetParent.files.Add(file);
        }
    }

    void CreateDefaultFolders()
    {
        // Head Folder
        Folder Head = new Folder("Head", rootFolder);
        Head.children.Add(new Folder("Eye", Head));
        Head.children.Add(new Folder("Ear", Head));
        Head.children.Add(new Folder("Nose", Head));
        Head.children.Add(new Folder("Mouth", Head));
        Head.children.Add(new Folder("Jaw", Head));
        Head.children.Add(new Folder("Skull", Head));
        Head.children.Add(new Folder("Brain", Head));

        // Body Folder
        Folder Body = new Folder("Body", rootFolder);
        Body.children.Add(new Folder("Chest", Body));
        Body.children.Add(new Folder("Abdomen", Body));
        Body.children.Add(new Folder("Back", Body));
        Body.children.Add(new Folder("Pelvis", Body));

        // Left Arm Folder
        Folder LeftArm = new Folder("LeftArm", rootFolder);
        LeftArm.children.Add(new Folder("UpperArm", LeftArm));
        LeftArm.children.Add(new Folder("ForeArm", LeftArm));
        LeftArm.children.Add(new Folder("Hand", LeftArm));

        // Right Arm Folder
        Folder RightArm = new Folder("RightArm", rootFolder);
        RightArm.children.Add(new Folder("UpperArm", RightArm));
        RightArm.children.Add(new Folder("ForeArm", RightArm));
        RightArm.children.Add(new Folder("Hand", RightArm));

        // Left Leg Folder
        Folder LeftLeg = new Folder("LeftLeg", rootFolder);
        LeftLeg.children.Add(new Folder("Thigh", LeftLeg));
        LeftLeg.children.Add(new Folder("Calf", LeftLeg));
        LeftLeg.children.Add(new Folder("Foot", LeftLeg));

        // Right Leg Folder
        Folder RightLeg = new Folder("RightLeg", rootFolder);
        RightLeg.children.Add(new Folder("Thigh", RightLeg));
        RightLeg.children.Add(new Folder("Calf", RightLeg));
        RightLeg.children.Add(new Folder("Foot", RightLeg));

        // Organ Folder
        Folder Organ = new Folder("Organ", rootFolder);
        Organ.children.Add(new Folder("Heart", Organ));
        Organ.children.Add(new Folder("Lungs", Organ));
        Organ.children.Add(new Folder("Liver", Organ));
        Organ.children.Add(new Folder("Stomach", Organ));
        Organ.children.Add(new Folder("Intestine", Organ));
        Organ.children.Add(new Folder("Kidneys", Organ));
        Organ.children.Add(new Folder("Pancreas", Organ));
        Organ.children.Add(new Folder("Spleen", Organ));
        Organ.children.Add(new Folder("Bladder", Organ));

        rootFolder.children.AddRange(new List<Folder> { Head, Body, LeftArm, RightArm, LeftLeg, RightLeg, Organ });
    }

    void AssignAbnormalParameters(Folder folder)
    {
        float abnormalChance = GetAbnormalProbabilityBySanity();
        foreach (var child in folder.children)
        {
            child.abnormalParameter = abnormalChance;
            child.AssignAbnormalByParameter();
            AssignAbnormalParameters(child);
        }
    }

    public Folder GetRootFolder() => rootFolder;
    public Folder GetCurrentFolder() => currentFolder;

    /// <summary>
    /// 파일 탐색기 초기화: 경로 패널 0번(루트)으로 이동
    /// </summary>
    public void FileExplorerInitialize()
    {
        Debug.Log("실행");
        if (pathPanelManager != null)
        {
            NavigateToPathIndex(0);
            Debug.Log("[FileWindow] FileExplorerInitialize: 0번 인덱스로 이동 완료");
        }
        else
        {
            if (rootFolder != null)
            {
                OpenFolder(rootFolder, false);
                Debug.Log("[FileWindow] FileExplorerInitialize: 경로 패널 없으므로 rootFolder로 초기화");
            }
        }
    }
}
