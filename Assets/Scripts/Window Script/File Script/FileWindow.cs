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
    public static FileWindow Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [Header("Prefabs")]
    public GameObject folderIconPrefab;
    public GameObject fileIconPrefab;
    public GameObject upButtonPrefab;

    [Header("ScriptableObject File List")]
    public FileDataSO fileDataSO;


    [Header("ScriptableObject Exe List")]
    public ExeDataSO exeDataSO;
    public GameObject exeIconPrefab;


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
    private ExeIcon selectedExeIcon;

    // 폴더 및 파일 관리
    private Folder rootFolder;
    private Folder currentFolder;
    private Stack<Folder> folderHistory = new Stack<Folder>();
    private List<File> currentFolderFiles = new List<File>();

    void Start()
    {
        rootFolder = new Folder("Root");
        CreateDefaultFolders();
        InitializeFilesFromSO();
        InitializeExeFromSO();
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

    private void InitializeFilesFromSO()
    {
        // FileDataSO가 없거나 내부 리스트가 없으면 아무 것도 안 함
        if (fileDataSO == null)
        {
            Debug.LogWarning("[FileWindow] fileDataSO가 할당되어 있지 않습니다. 파일 초기화를 건너뜁니다.");
            return;
        }

        if (fileDataSO.fileDatas == null || fileDataSO.fileDatas.Count == 0)
        {
            Debug.Log("[FileWindow] fileDataSO에 파일 데이터가 없습니다.");
            return;
        }

        // (원본 InitializeFilesFromInspector 동작 유지)
        foreach (var data in fileDataSO.fileDatas)
        {
            Folder targetParent = FindFolderByName(rootFolder, data.parentFolderName);
            if (targetParent == null)
            {
                Debug.LogWarning($"부모 폴더 '{data.parentFolderName}'을(를) 찾을 수 없습니다. Root에 추가합니다.");
                targetParent = rootFolder;
            }

            File file = new File(
                data.fileName,
                data.extension,
                targetParent,
                data.textContent,
                data.imageContent
            );
            RegisterFile(file);

            currentFolderFiles.Add(file);
            targetParent.files.Add(file);
        }

        Debug.Log($"[FileWindow] InitializeFilesFromSO: {fileDataSO.fileDatas.Count}개 파일 로드 완료.");
    }




    void CreateDefaultFolders()
    {
        // (생략 없이 기존 코드 유지)
        Folder Work_Base = new Folder("Work_Base", rootFolder);

        Folder Machine = new Folder("Machine", rootFolder);
        Machine.children.Add(new Folder("Generator", Machine));
        //Machine.isImportant = true;

        Folder Fax = new Folder("Fax", rootFolder);
        

        Folder Computer = new Folder("Computer", rootFolder);
     
        
        rootFolder.children.AddRange(new List<Folder> {
            Work_Base, Machine, Fax, Computer
        });
    }

    void AssignAbnormalParameters(Folder folder)
    {
        //float abnormalChance = GetAbnormalProbabilityBySanity();
        foreach (var child in folder.children)
        {
            //child.importantParameter = abnormalChance;
            //child.AssignAbnormalByParameter();
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
        InitializeFilesFromSO();
        AssignAbnormalParameters(rootFolder);

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(true);
        }

        OpenFolder(rootFolder, false);
    }

    // 나머지 기존 함수 (OpenFolder, NavigateToPathIndex 등) 그대로 유지
    // 모든 File 객체를 관리하는 리스트
    public List<File> allFiles = new List<File>();

    public void RegisterFile(File file)
    {
        if (file != null && !allFiles.Contains(file))
            allFiles.Add(file);
    }
    public void InitializeAllFileState()
    {
        foreach (var file in allFiles) // allFiles는 FileWindow에서 관리하는 모든 File 객체 리스트
        {
            file.InitializeFileState(); // File.cs 안에서 isImportant, isChecked 초기화
        }

        Debug.Log("[FileWindow] 모든 파일 상태 초기화 완료");
    }

    // EXE 아이콘 선택 함수 (ExeIcon 전용)
    public void SetSelectedExeIcon(ExeIcon exeIcon)
    {
        if (exeIcon == null)
        {
            Debug.LogWarning("[FileWindow] SetSelectedExeIcon 호출됨, 하지만 exeIcon이 null입니다.");
            return;
        }

        // 기존 파일/폴더 선택 해제
        if (selectedFileIcon != null)
        {
            selectedFileIcon.SetSelected(false);
            selectedFileIcon = null;
        }
        if (selectedFolderIcon != null)
        {
            selectedFolderIcon.SetSelected(false);
            selectedFolderIcon = null;
        }

        // 기존 EXE 선택 해제
        if (selectedExeIcon != null && selectedExeIcon != exeIcon)
        {
            selectedExeIcon.SetSelected(false);
        }

        selectedExeIcon = exeIcon;
        selectedExeIcon.SetSelected(true);

        Debug.Log("[FileWindow] EXE 아이콘 선택됨: " + (exeIcon != null && exeIcon.GetExe() != null ? exeIcon.GetExe().name : "(unknown)"));
    }

    private List<Exe> allExeList = new List<Exe>();

    private void InitializeExeFromSO()
    {
        if (exeDataSO == null)
        {
            Debug.LogWarning("[FileWindow] exeDataSO가 비어 있습니다.");
            return;
        }

        if (exeDataSO.exeDatas == null || exeDataSO.exeDatas.Count == 0)
        {
            Debug.Log("[FileWindow] exeDataSO 안에 EXE 데이터가 없습니다.");
            return;
        }

        foreach (var data in exeDataSO.exeDatas)
        {
            Folder targetParent = FindFolderByName(rootFolder, data.parentFolderName);
            if (targetParent == null)
            {
                Debug.LogWarning($"EXE 부모 폴더 '{data.parentFolderName}'을 찾을 수 없어 Root에 추가합니다.");
                targetParent = rootFolder;
            }

            Exe exe = new Exe
            {
                name = data.exeName,
                parent = targetParent,
                exeContent = data.exeContent
            };

            allExeList.Add(exe);
            targetParent.exes.Add(exe);
        }

        Debug.Log($"[FileWindow] InitializeExeFromSO: {exeDataSO.exeDatas.Count}개 EXE 로드 완료");
    }

}
