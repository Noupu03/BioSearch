using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ��ü ������ ��ư, ����/�̻� ��������Ʈ�� ����.
/// </summary>
[System.Serializable]
public class BodyPart
{
    public string partName;       // ���� �̸� (��: "Head")
    public Button button;         // �ش� ���� ��ư
    public Sprite normalSprite;   // ���� ���� ��������Ʈ
    public Sprite abnormalSprite; // �̻� ���� ��������Ʈ
}

/// <summary>
/// ������ ��ư Ŭ�� �� FileWindow�� ���� ������ �������
/// ����/�̻� ��������Ʈ�� ��ȯ�ϴ� ���� Ŭ����.
/// </summary>
public class BodyPartViewer : MonoBehaviour
{
    [Header("���� ī�޶�â")]
    [SerializeField] private Image cameraPanel;  // ��������Ʈ ǥ�ÿ� Image

    [Header("���� & ��ư ����")]
    [SerializeField] private BodyPart[] bodyParts;  // Inspector ��Ͽ� �迭

    [Header("File Window ����")]
    [SerializeField] private FileWindow fileWindow; // ���� ���� ������

    // ���� ��ȸ�� ���� ĳ��
    private Dictionary<string, BodyPart> bodyPartLookup = new Dictionary<string, BodyPart>();

    void Awake()
    {
        // ĳ�� �ʱ�ȭ
        foreach (var part in bodyParts)
        {
            if (string.IsNullOrEmpty(part.partName))
            {
                Debug.LogWarning("BodyPart �̸��� ��� �ֽ��ϴ�.", this);
                continue;
            }

            if (bodyPartLookup.ContainsKey(part.partName))
            {
                Debug.LogWarning($"�ߺ��� BodyPart �̸�: {part.partName}", this);
                continue;
            }

            bodyPartLookup[part.partName] = part;
        }
    }

    void Start()
    {
        // �� ��ư Ŭ�� �̺�Ʈ ���
        foreach (var part in bodyParts)
        {
            if (part.button == null)
            {
                Debug.LogWarning($"{part.partName} ��ư�� �������� �ʾҽ��ϴ�.", this);
                continue;
            }

            string partName = part.partName; // ���� ���� (���� ĸó ����)
            part.button.onClick.AddListener(() => OnBodyPartClicked(partName));
        }
    }

    /// <summary>
    /// ��ư Ŭ�� �� �ش� ������ ����/�̻� ��������Ʈ ǥ��
    /// </summary>
    private void OnBodyPartClicked(string partName)
    {
        if (cameraPanel == null)
        {
            Debug.LogWarning("cameraPanel�� �������� �ʾҽ��ϴ�.", this);
            return;
        }

        if (fileWindow == null)
        {
            Debug.LogWarning("fileWindow�� ������� �ʾҽ��ϴ�.", this);
            return;
        }

        Folder root = fileWindow.GetRootFolder();
        if (root == null)
        {
            Debug.LogWarning("FileWindow�� ��Ʈ ������ �����ϴ�.", this);
            return;
        }

        Folder targetFolder = FindFolderByName(root, partName);
        if (targetFolder == null)
        {
            Debug.LogWarning($"'{partName}' ������ ã�� �� �����ϴ�.", this);
            return;
        }

        if (!bodyPartLookup.TryGetValue(partName, out BodyPart part))
        {
            Debug.LogWarning($"'{partName}'�� �����Ǵ� BodyPart�� �����ϴ�.", this);
            return;
        }

        // �̻� ���¿� ���� ��������Ʈ ��ü
        cameraPanel.sprite = targetFolder.isAbnormal && part.abnormalSprite != null
            ? part.abnormalSprite
            : part.normalSprite;
    }

    /// <summary>
    /// ���� �̸����� Folder�� ��� Ž��
    /// </summary>
    private Folder FindFolderByName(Folder folder, string name)
    {
        if (folder == null)
            return null;

        if (folder.name == name)
            return folder;

        foreach (var child in folder.children)
        {
            Folder found = FindFolderByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
