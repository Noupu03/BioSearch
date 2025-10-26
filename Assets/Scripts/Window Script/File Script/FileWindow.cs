using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 파일 및 폴더 UI를 관리하는 클래스.
/// 폴더 구조 생성, 파일 생성, 경로 이동, 선택 상태 관리 등을 담당.
/// </summary>
public partial class FileWindow : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject folderIconPrefab;    // 폴더 아이콘 프리팹
    public GameObject fileIconPrefab;      // 파일 아이콘 프리팹
    public GameObject dummyFolderIconPrefab;
    public GameObject dummyFileIconPrefab;

    [Header("Scroll Area")]
    public Transform contentArea;          // 스크롤 콘텐츠 영역
    public TMP_Text emptyText;             // 콘텐츠가 없을 때 표시되는 텍스트

    [Header("Path Panel")]
    public PathPanelManager pathPanelManager;  // 경로 패널 매니저

    [Header("Back Button")]
    public Button backButton;              // 뒤로가기 버튼

    [Header("Inspector File List")]
    public List<FileData> fileDatas = new List<FileData>(); // 인스펙터에서 입력되는 파일 데이터

    [Header("Body Buttons")]
    public Button headButton;
    public Button bodyButton;
    public Button leftArmButton;
    public Button leftHandButton;
    public Button rightArmButton;
    public Button rightHandButton;
    public Button leftLegButton;
    public Button leftFootButton;
    public Button rightLegButton;
    public Button rightFootButton;

    [Header("Special Prefabs")]
    public GameObject upButtonPrefab;  // 상위 폴더로 이동하는 "..." 아이콘 프리팹

    // 현재 선택된 아이콘
    private FolderIcon selectedFolderIcon;
    private FileIcon selectedFileIcon;

    // 폴더 구조 관리
    private Folder rootFolder;                 // 루트 폴더
    private Folder currentFolder;              // 현재 열려 있는 폴더
    private Stack<Folder> folderHistory = new Stack<Folder>(); // 이전 폴더 기록 (뒤로가기용)

    // 현재 생성된 파일 목록
    private List<File> currentFolderFiles = new List<File>();

    [Header("Dummy Icons")]
    public List<DummyIcon> dummyIcons = new List<DummyIcon>();
    // FileWindow.cs
    public void CreateDummyIconUI(DummyIcon dummy, Folder parentFolder)
    {
        if (dummy == null) return;

        GameObject prefab = dummy.isFolder ? dummyFolderIconPrefab : dummyFileIconPrefab;
        if (prefab == null || contentArea == null) return;

        GameObject go = Instantiate(prefab, contentArea);
        dummy.uiObject = go;

        TMP_Text textComp = go.GetComponentInChildren<TMP_Text>();
        if (textComp != null)
        {
            textComp.text = dummy.name;
            textComp.color = Color.white;
        }
            

        Button btn = go.GetComponent<Button>();
        if (btn != null)
            btn.interactable = false;
    }

    void Awake()
    {
        // 경로 패널 초기화
        if (pathPanelManager != null)
            pathPanelManager.Initialize(this);
    }

    void Start()
    {
        // 기본 폴더 구조 생성
        rootFolder = new Folder("Root");
        CreateDefaultFolders();

        // Inspector의 FileData 기반으로 파일 생성
        InitializeFilesFromInspector();

        // 이상 폴더 확률 설정
        AssignAbnormalParameters(rootFolder);

        // 뒤로가기 버튼 초기화
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(true);
        }

        // 루트 폴더 열기
        OpenFolder(rootFolder, false);
    }

    /// <summary>
    /// Inspector에서 입력된 파일 데이터를 바탕으로 파일 객체 생성
    /// </summary>
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

            // Sanity 기반 확률로 비정상 여부 설정
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

    /// <summary>
    /// 기본 폴더 구조를 생성하고 UI 버튼과 연결
    /// </summary>
    void CreateDefaultFolders()
    {
        // 폴더 생성
        Folder Head = new Folder("Head", rootFolder);
        Head.children.Add(new Folder("Mouse", Head));
        Head.children.Add(new Folder("LeftEye", Head));
        Head.children.Add(new Folder("RightEye", Head));
        Head.children.Add(new Folder("Nose", Head));

        Folder Body = new Folder("Body", rootFolder);

        Folder Organ = new Folder("Organ", rootFolder);
        Organ.children.Add(new Folder("Heart", Organ));

        Folder LeftArm = new Folder("LeftArm", rootFolder);
        Folder LeftHand = new Folder("LeftHand", rootFolder);
        Folder RightArm = new Folder("RightArm", rootFolder);
        Folder RightHand = new Folder("RightHand", rootFolder);

        Folder LeftLeg = new Folder("LeftLeg", rootFolder);
        Folder LeftFoot = new Folder("LeftFoot", rootFolder);
        Folder RightLeg = new Folder("RightLeg", rootFolder);
        Folder RightFoot = new Folder("RightFoot", rootFolder);

        // 루트에 추가
        rootFolder.children.AddRange(new List<Folder>
        {
            Head, Body, Organ, LeftArm, LeftHand, RightArm, RightHand,
            LeftLeg, LeftFoot, RightLeg, RightFoot
        });

        // UI 버튼 연결
        if (headButton != null) Head.linkedBodyButton = headButton;
        if (bodyButton != null) Body.linkedBodyButton = bodyButton;
        if (leftArmButton != null) LeftArm.linkedBodyButton = leftArmButton;
        if (leftHandButton != null) LeftHand.linkedBodyButton = leftHandButton;
        if (rightArmButton != null) RightArm.linkedBodyButton = rightArmButton;
        if (rightHandButton != null) RightHand.linkedBodyButton = rightHandButton;
        if (leftLegButton != null) LeftLeg.linkedBodyButton = leftLegButton;
        if (leftFootButton != null) LeftFoot.linkedBodyButton = leftFootButton;
        if (rightLegButton != null) RightLeg.linkedBodyButton = rightLegButton;
        if (rightFootButton != null) RightFoot.linkedBodyButton = rightFootButton;
    }

    /// <summary>
    /// 자식 폴더에 이상 여부를 확률적으로 설정
    /// </summary>
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

    public Folder GetRootFolder()
    {
        return rootFolder;
    }

    public Folder GetCurrentFolder() => currentFolder;
}

