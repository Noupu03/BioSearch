using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ���� ������ ����.
/// �ڽ� ���� ����, �̻� ���� ǥ��.
/// </summary>

public class Folder
{
    public string name;
    public List<Folder> children = new List<Folder>();
    public List<File> files = new List<File>();   // ���� ����Ʈ �߰�
    public Folder parent;

    // �̻� ���� ����
    public bool isAbnormal = false;

    // �̻� ���� Ž���� �Ķ���� (0~1 ����, �ܺο��� ���� ����)
    public float abnormalParameter = 0f;

    // ����� ��ü ��ư
    public Button linkedBodyButton;

    public Folder(string name, Folder parent = null, Button linkedButton = null)
    {
        this.name = name;
        this.parent = parent;
        this.linkedBodyButton = linkedButton;
    }

    // Ȯ���� ���� �̻� ���� ���� (�ڽ� ���� ����)
    public void AssignAbnormalByParameter()
    {
        isAbnormal = Random.value < abnormalParameter;

        // �ڽ� ������ ��������� �̻� ���� ����
        foreach (var child in children)
        {
            child.AssignAbnormalByParameter();
        }

        // ��ư ���� ���� (�ڽŰ� �θ����)
        UpdateLinkedButtonColor();
        UpdateParentButtonColor();
    }
    // 연결된 버튼 색상 업데이트 (자식 포함 이상 여부 반영)
    public void UpdateLinkedButtonColor()
    {
        if (linkedBodyButton == null) return;

        bool shouldBeRed = isAbnormal || HasAbnormalInChildren();
        Color targetColor = shouldBeRed ? Color.red : Color.white;

        var colors = linkedBodyButton.colors;
        colors.normalColor      = targetColor;
        colors.highlightedColor = targetColor;
        colors.pressedColor     = targetColor;
        colors.selectedColor    = targetColor;
        linkedBodyButton.colors = colors;
    }

    // �θ� ��ư ���� ����
    private void UpdateParentButtonColor()
    {
        parent?.UpdateLinkedButtonColor();
        parent?.UpdateParentButtonColor();
    }

    // ��������� �ڽ� ���� �̻� ���� Ȯ��
    // ��������� �ڽ� ���� + ���� �̻� ���� Ȯ��
    private bool HasAbnormalInChildren()
    {
        // 1) ���� ���� �˻�
        foreach (var file in files)
        {
            if (file.isAbnormal)    // ���Ͽ� �̻��� �ϳ��� ������ �̻� ó��
                return true;
        }

        // 2) �ڽ� ���� �˻�
        foreach (var child in children)
        {
            if (child.isAbnormal || child.HasAbnormalInChildren())
                return true;
        }

        return false;
    }



    // �ڽ� �߰�
    public void AddChild(Folder child)
    {
        if (!children.Contains(child))
        {
            children.Add(child);
            child.parent = this;
        }
    }

    // �ڽ� ����
    public void RemoveChild(Folder child)
    {
        if (children.Contains(child))
        {
            children.Remove(child);
            child.parent = null;
        }
    }

    // ���� �߰�
    public void AddFile(File file)
    {
        if (!files.Contains(file))
        {
            files.Add(file);
            file.parent = this;
        }
    }

    // ���� ����
    public void RemoveFile(File file)
    {
        if (files.Contains(file))
        {
            files.Remove(file);
            file.parent = null;
        }
    }
}
