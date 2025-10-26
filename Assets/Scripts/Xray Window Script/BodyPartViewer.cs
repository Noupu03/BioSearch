using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class BodyPart
{
    public string partName;       // ���� �̸� (��: "LeftUpperArm")
    public Button button;         // �ش� ���� ��ư
    public Sprite normalSprite;   // ���� ���� ��������Ʈ
    public Sprite abnormalSprite; // �̻� ���� ��������Ʈ
}

public class BodyPartViewer : MonoBehaviour
{
    [Header("���� ī�޶�â")]
    public Image cameraPanel;  // ��������Ʈ�� ǥ���� Image

    [Header("���� & ��ư ����")]
    public BodyPart[] bodyParts;  // Inspector���� ����, ��ư, ��������Ʈ �� ���

    [Header("File Window ����")]
    public FileWindow fileWindow; // ���� �̻� ���� ������ �������� ���� ����

    // ������ �� Folder ��� ����
    private Dictionary<string, string[]> partNameToFolderPath = new Dictionary<string, string[]>
    {
        { "Head", new string[] { "Head" } },
        { "Chest", new string[] { "Body", "Chest" } },
        { "LeftUpperArm", new string[] { "LeftArm", "UpperArm" } },
        { "LeftForeArm", new string[] { "LeftArm", "ForeArm" } },
        { "LeftHand", new string[]  { "LeftArm", "Hand" } },
        { "RightUpperArm", new string[] { "RightArm", "UpperAarm" } },
        { "RightForeArm", new string[] { "RightArm", "ForeArm" } },
        { "RightHand", new string[] { "RightArm", "Hand" } },
        { "Stomach", new string[] { "Organ", "Stomach" } },
        { "Pelvis", new string[] { "Body", "Pelvis" } },
        { "LeftThigh", new string[] { "LeftLeg", "Thigh" } },
        { "LeftCalf", new string[] { "LeftLeg", "Calf" } },
        { "LeftFoot", new string[] { "LeftLeg", "Foot" } },
        { "RightThigh", new string[] { "RightLeg", "Thigh" } },
        { "RightCalf", new string[] { "RightLeg", "Calf" } },
        { "RightFoot", new string[] { "RightLeg", "Foot" } }
    };

    void Start()
    {
        // �� ��ư Ŭ�� �̺�Ʈ ���
        foreach (var part in bodyParts)
        {
            string partName = part.partName; // ���� ���� ���� (���� ĸó ����)
            if (part.button != null)
            {
                part.button.onClick.AddListener(() => ShowPart(partName));
            }
        }
    }

    /// <summary>
    /// ��ư Ŭ�� �� �ش� ������ ��������Ʈ ǥ��
    /// </summary>
    public void ShowPart(string partName)
    {
        if (cameraPanel == null)
        {
            Debug.LogWarning("cameraPanel�� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (fileWindow == null)
        {
            Debug.LogWarning("fileWindow�� ������� �ʾҽ��ϴ�.");
            return;
        }

        // ������ �� Folder ��� ���� Ȯ��
        if (!partNameToFolderPath.TryGetValue(partName, out var path))
        {
            Debug.LogWarning("���� ��θ� ã�� �� �����ϴ�: " + partName);
            return;
        }

        // ��Ʈ �������� ��� ���� Folder Ž��
        Folder targetFolder = fileWindow.GetRootFolder();
        foreach (var node in path)
        {
            targetFolder = targetFolder.children.Find(f => f.name == node);
            if (targetFolder == null)
            {
                Debug.LogWarning("���� ��� �� �Ϻθ� ã�� �� �����ϴ�: " + string.Join("/", path));
                return;
            }
        }

        // BodyPartViewer�� bodyParts ��Ͽ��� �����Ǵ� BodyPart ã��
        var partObj = System.Array.Find(bodyParts, p => p.partName == partName);
        if (partObj == null) return;

        // Folder �̻� ���� ����
        bool shouldShowAbnormal = targetFolder.isAbnormal || HasAbnormalInChildren(targetFolder);

        // ��������Ʈ ����
        cameraPanel.sprite = (shouldShowAbnormal && partObj.abnormalSprite != null) ? partObj.abnormalSprite : partObj.normalSprite;
    }

    /// <summary>
    /// �ڽ� ���� + ���� �̻� ���� Ȯ�� (���)
    /// </summary>
    private bool HasAbnormalInChildren(Folder folder)
    {
        if (folder == null) return false;

        foreach (var file in folder.files)
        {
            if (file.isAbnormal) return true;
        }

        foreach (var child in folder.children)
        {
            if (child.isAbnormal || HasAbnormalInChildren(child)) return true;
        }

        return false;
    }
}
