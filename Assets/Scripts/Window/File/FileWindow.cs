using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Haare.Client.Routine;

[System.Serializable]
public class BodyButtonEntry
{
    public string folderPath; // e.g. "Head", "Body/Chest", "LeftArm/UpperArm"
    public Button button;
}

public partial class FileWindow : MonoRoutine, IStageResettable
{
    public static FileWindow Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject folderIconPrefab;
    [SerializeField] private GameObject fileIconPrefab;

    [Header("Scroll Area")]
    [SerializeField] private Transform contentArea;
    [SerializeField] private TMP_Text  emptyText;

    [Header("Path Panel")]
    [SerializeField] private PathPanelManager pathPanelManager;

    [Header("Back Button")]
    [SerializeField] private Button backButton;

    [Header("Inspector File List")]
    [SerializeField] private List<FileData> fileDatas = new List<FileData>();

    [Header("Body Buttons")]
    [SerializeField] private List<BodyButtonEntry> bodyButtons = new List<BodyButtonEntry>();

    [Header("Special Prefabs")]
    [SerializeField] private GameObject upButtonPrefab;

    private FolderIcon selectedFolderIcon;
    private FileIcon selectedFileIcon;

    private Folder rootFolder;
    private Folder currentFolder;
    private Stack<Folder> folderHistory = new Stack<Folder>();
    private List<File> currentFolderFiles = new List<File>();

    protected override void Constructor()
    {
        base.Constructor();
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        if (pathPanelManager != null)
            pathPanelManager.Initialize(this);
    }

    // MonoRoutine도 private OnDestroy()를 정의하므로(Awake와 같은 문제), 대신 OnDisable 사용.
    void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(true);
        }
        // 콘텐츠 초기화는 GameLoopManager.RequestFirstStage()가 담당
    }

    /// <summary>스테이지 전환 시 파일 트리 및 UI를 초기화한다.</summary>
    public void ResetForNewStage()
    {
        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        folderHistory.Clear();
        currentFolderFiles.Clear();
        selectedFolderIcon = null;
        selectedFileIcon   = null;

        rootFolder = new Folder("Root");
        CreateDefaultFolders();
        LinkBodyButtons();
        InitializeFilesFromInspector();
        AssignAbnormalParameters(rootFolder);
        OpenFolder(rootFolder, false);
    }

    private void InitializeFilesFromInspector()
    {
        float abnormalChance = GameConfig.AbnormalChanceMid;

        foreach (var data in fileDatas)
        {
            Folder targetParent = FindFolderByName(rootFolder, data.parentFolderName);
            if (targetParent == null)
            {
                Debug.LogWarning($"�θ� ���� '{data.parentFolderName}'��(��) ã�� �� �����ϴ�. Root�� �߰��մϴ�.");
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
            targetParent.AddFile(file);
        }
    }

    // Folder 생성자가 parent에 자동 등록하므로 별도 Add 불필요
    void CreateDefaultFolders()
    {
        var Head   = new Folder("Head",     rootFolder);
        _ = new Folder("Eye",   Head);  _ = new Folder("Ear",   Head);
        _ = new Folder("Nose",  Head);  _ = new Folder("Mouth", Head);
        _ = new Folder("Jaw",   Head);  _ = new Folder("Skull", Head);
        _ = new Folder("Brain", Head);

        var Body = new Folder("Body", rootFolder);
        _ = new Folder("Chest",   Body); _ = new Folder("Abdomen", Body);
        _ = new Folder("Back",    Body); _ = new Folder("Pelvis",  Body);

        var LeftArm  = new Folder("LeftArm",  rootFolder);
        _ = new Folder("UpperArm", LeftArm); _ = new Folder("ForeArm", LeftArm);
        _ = new Folder("Hand", LeftArm);

        var RightArm = new Folder("RightArm", rootFolder);
        _ = new Folder("UpperArm", RightArm); _ = new Folder("ForeArm", RightArm);
        _ = new Folder("Hand", RightArm);

        var LeftLeg  = new Folder("LeftLeg",  rootFolder);
        _ = new Folder("Thigh", LeftLeg); _ = new Folder("Calf", LeftLeg);
        _ = new Folder("Foot",  LeftLeg);

        var RightLeg = new Folder("RightLeg", rootFolder);
        _ = new Folder("Thigh", RightLeg); _ = new Folder("Calf", RightLeg);
        _ = new Folder("Foot",  RightLeg);

        var Organ = new Folder("Organ", rootFolder);
        _ = new Folder("Heart",    Organ); _ = new Folder("Lungs",     Organ);
        _ = new Folder("Liver",    Organ); _ = new Folder("Stomach",   Organ);
        _ = new Folder("Intestine",Organ); _ = new Folder("Kidneys",   Organ);
        _ = new Folder("Pancreas", Organ); _ = new Folder("Spleen",    Organ);
        _ = new Folder("Bladder",  Organ);
    }

    private void LinkBodyButtons()
    {
        foreach (var entry in bodyButtons)
        {
            if (entry.button == null || string.IsNullOrEmpty(entry.folderPath)) continue;
            var folder = FindFolderByPath(rootFolder, entry.folderPath);
            if (folder != null)
                folder.linkedBodyButton = entry.button;
            else
                Debug.LogWarning($"[FileWindow] 경로 '{entry.folderPath}'의 폴더를 찾을 수 없습니다.");
        }
    }

    private Folder FindFolderByPath(Folder root, string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        var parts = path.Split('/');
        Folder current = root;
        foreach (var part in parts)
        {
            string trimmed = part.Trim();
            Folder next = null;
            foreach (var child in current.children)
            {
                if (string.Equals(child.name, trimmed, System.StringComparison.OrdinalIgnoreCase))
                { next = child; break; }
            }
            current = next;
            if (current == null) return null;
        }
        return current;
    }

    void AssignAbnormalParameters(Folder folder)
    {
        float abnormalChance = GameConfig.AbnormalChanceMid;
        foreach (var child in folder.children)
        {
            child.abnormalParameter = abnormalChance;
            child.AssignAbnormalByParameter();
            AssignAbnormalParameters(child);
        }
    }

    public Folder GetRootFolder() => rootFolder;
    public Folder GetCurrentFolder() => currentFolder;
}
