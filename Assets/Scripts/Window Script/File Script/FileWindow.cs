using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 씬에 존재하는 FileWindow 오브젝트에 붙여서
/// 데이터와 기능을 담당.
/// FileProgram 프리팹은 OpenProgram()에서 생성될 때 연결됨.
/// </summary>
public partial class FileWindow : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject folderIconPrefab;
    public GameObject fileIconPrefab;
    public GameObject upButtonPrefab;

    [Header("Inspector File List")]
    public List<FileData> fileDatas = new List<FileData>();

    [Header("UI Components (Prefab에서 연결됨)")]
    public Transform contentArea;
    public TMP_Text emptyText;
    public PathPanelManager pathPanelManager;
    public Button backButton;

    [Header("FileProgram Prefab (UI)")]
    public GameObject fileProgramPrefab; // ProgramOpen에서 Instantiate할 프리팹

    private FileProgram instantiatedFileProgram;

    // 선택된 아이콘
    private FolderIcon selectedFolderIcon;
    private FileIcon selectedFileIcon;

    // 폴더 및 파일 관리
    private Folder rootFolder;
    private Folder currentFolder;
    private Stack<Folder> folderHistory = new Stack<Folder>();
    private List<File> currentFolderFiles = new List<File>();

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

    //  ProgramOpen에서 FileProgram이 Instantiate될 때 자동으로 연결됨
    public void LinkWithProgram(FileProgram program)
    {
        instantiatedFileProgram = program;

        if (contentArea == null) contentArea = program.contentArea;
        if (emptyText == null) emptyText = program.emptyText;
        if (pathPanelManager == null) pathPanelManager = program.pathPanelManager;
        if (backButton == null) backButton = program.backButton;

        if (pathPanelManager != null)
            pathPanelManager.Initialize(this);

        Debug.Log("[FileWindow] FileProgram과 연결 완료");
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
        // (생략 없이 기존 코드 유지)
        Folder Work_Base = new Folder("Work_Base", rootFolder);

        Folder Machine = new Folder("Machine", rootFolder);
        Machine.children.Add(new Folder("Generator", Machine));

        Folder Fax = new Folder("Fax", rootFolder);
        

        Folder Computer = new Folder("Computer", rootFolder);
     
        
        rootFolder.children.AddRange(new List<Folder> {
            Work_Base, Machine, Fax, Computer
        });
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
    /// <summary>
    /// FileProgram을 외부에서 수동으로 초기화할 때 호출
    /// </summary>
    public void InitializeFileProgram()
    {
        if (fileProgramPrefab != null && instantiatedFileProgram == null)
        {
            GameObject go = Instantiate(fileProgramPrefab, this.transform);
            instantiatedFileProgram = go.GetComponent<FileProgram>();

            if (instantiatedFileProgram != null)
            {
                if (contentArea == null) contentArea = instantiatedFileProgram.contentArea;
                if (emptyText == null) emptyText = instantiatedFileProgram.emptyText;
                if (pathPanelManager == null) pathPanelManager = instantiatedFileProgram.pathPanelManager;
                if (backButton == null) backButton = instantiatedFileProgram.backButton;

                if (pathPanelManager != null)
                    pathPanelManager.Initialize(this);
            }

            Debug.Log("[FileWindow] FileProgram 수동 초기화 완료");
        }

        // 이후 Start()에서 수행하던 초기화 루틴을 재호출
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

    // 나머지 기존 함수 (OpenFolder, NavigateToPathIndex 등) 그대로 유지
}
