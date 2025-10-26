using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ���� �� ���� UI�� �����ϴ� Ŭ����.
/// ���� ���� ����, ���� ����, ��� �̵�, ���� ���� ���� ���� ���.
/// </summary>
public partial class FileWindow : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject folderIconPrefab;    // ���� ������ ������
    public GameObject fileIconPrefab;      // ���� ������ ������
    public GameObject dummyFolderIconPrefab;
    public GameObject dummyFileIconPrefab;

    [Header("Scroll Area")]
    public Transform contentArea;          // ��ũ�� ������ ����
    public TMP_Text emptyText;             // �������� ���� �� ǥ�õǴ� �ؽ�Ʈ

    [Header("Path Panel")]
    public PathPanelManager pathPanelManager;  // ��� �г� �Ŵ���

    [Header("Back Button")]
    public Button backButton;              // �ڷΰ��� ��ư

    [Header("Inspector File List")]
    public List<FileData> fileDatas = new List<FileData>(); // �ν����Ϳ��� �ԷµǴ� ���� ������

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
    public GameObject upButtonPrefab;  // ���� ������ �̵��ϴ� "..." ������ ������

    // ���� ���õ� ������
    private FolderIcon selectedFolderIcon;
    private FileIcon selectedFileIcon;

    // ���� ���� ����
    private Folder rootFolder;                 // ��Ʈ ����
    private Folder currentFolder;              // ���� ���� �ִ� ����
    private Stack<Folder> folderHistory = new Stack<Folder>(); // ���� ���� ��� (�ڷΰ����)

    // ���� ������ ���� ���
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
        // ��� �г� �ʱ�ȭ
        if (pathPanelManager != null)
            pathPanelManager.Initialize(this);
    }

    void Start()
    {
        // �⺻ ���� ���� ����
        rootFolder = new Folder("Root");
        CreateDefaultFolders();

        // Inspector�� FileData ������� ���� ����
        InitializeFilesFromInspector();

        // �̻� ���� Ȯ�� ����
        AssignAbnormalParameters(rootFolder);

        // �ڷΰ��� ��ư �ʱ�ȭ
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(true);
        }

        // ��Ʈ ���� ����
        OpenFolder(rootFolder, false);
    }

    /// <summary>
    /// Inspector���� �Էµ� ���� �����͸� �������� ���� ��ü ����
    /// </summary>
    private void InitializeFilesFromInspector()
    {
        float abnormalChance = GetAbnormalProbabilityBySanity();

        foreach (var data in fileDatas)
        {
            Folder targetParent = FindFolderByName(rootFolder, data.parentFolderName);

            if (targetParent == null)
            {
                Debug.LogWarning($"�θ� ���� '{data.parentFolderName}'��(��) ã�� �� �����ϴ�. Root�� �߰��մϴ�.");
                targetParent = rootFolder;
            }

            // Sanity ��� Ȯ���� ������ ���� ����
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
    /// �⺻ ���� ������ �����ϰ� UI ��ư�� ����
    /// </summary>
    void CreateDefaultFolders()
    {
        // Head
        Folder Head = new Folder("Head", rootFolder);
        Head.children.Add(new Folder("Eye", Head));
        Head.children.Add(new Folder("Ear", Head));
        Head.children.Add(new Folder("Nose", Head));
        Head.children.Add(new Folder("Mouth", Head));
        Head.children.Add(new Folder("Jaw", Head));
        Head.children.Add(new Folder("Skull", Head));
        Head.children.Add(new Folder("Brain", Head));

        // Body
        Folder Body = new Folder("Body", rootFolder);
        Body.children.Add(new Folder("Chest", Body));
        Body.children.Add(new Folder("Abdomen", Body));
        Body.children.Add(new Folder("Back", Body));
        Body.children.Add(new Folder("Pelvis", Body));

        // Left Arm
        Folder LeftArm = new Folder("Left Arm", rootFolder);
        LeftArm.children.Add(new Folder("Shoulder", LeftArm));
        LeftArm.children.Add(new Folder("Upper_arm", LeftArm));
        LeftArm.children.Add(new Folder("Forearm", LeftArm));
        LeftArm.children.Add(new Folder("Hand", LeftArm));

        Folder LeftHand = new Folder("Left Hand", rootFolder);

        // Right Arm
        Folder RightArm = new Folder("Right Arm", rootFolder);
        RightArm.children.Add(new Folder("Shoulder", RightArm));
        RightArm.children.Add(new Folder("Upper_arm", RightArm));
        RightArm.children.Add(new Folder("Forearm", RightArm));
        RightArm.children.Add(new Folder("Hand", RightArm));

        Folder RightHand = new Folder("Right Hand", rootFolder);

        // Left Leg
        Folder LeftLeg = new Folder("Left Leg", rootFolder);
        LeftLeg.children.Add(new Folder("Thigh", LeftLeg));
        LeftLeg.children.Add(new Folder("Knee", LeftLeg));
        LeftLeg.children.Add(new Folder("Lower_leg", LeftLeg));
        LeftLeg.children.Add(new Folder("Foot", LeftLeg));

        Folder LeftFoot = new Folder("Left Foot", rootFolder);

        // Right Leg
        Folder RightLeg = new Folder("Right Leg", rootFolder);
        RightLeg.children.Add(new Folder("Thigh", RightLeg));
        RightLeg.children.Add(new Folder("Knee", RightLeg));
        RightLeg.children.Add(new Folder("Lower_leg", RightLeg));
        RightLeg.children.Add(new Folder("Foot", RightLeg));

        Folder RightFoot = new Folder("Right Foot", rootFolder);

        // Organ
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

        // ��Ʈ ������ �߰�
        rootFolder.children.AddRange(new List<Folder>
    {
        Head, Body, LeftArm, LeftHand, RightArm, RightHand, LeftLeg, LeftFoot, RightLeg, RightFoot, Organ
    });
    

        // UI ��ư ����
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
    /// �ڽ� ������ �̻� ���θ� Ȯ�������� ����
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

