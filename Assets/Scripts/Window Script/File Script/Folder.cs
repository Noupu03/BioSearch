using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ���� ������ ����.
/// �ڽ� ���� ����, �̻� ���� ǥ��.
/// </summary>
[System.Serializable]
public class Folder
{
    public string name;
    public List<Folder> children = new List<Folder>();
    public Folder parent;

    // �̻� ���� ����
    public bool isAbnormal = false;

    // �̻� ���� Ž���� �Ķ���� (0~1 ����, �ܺο��� ���� ����)
    public float abnormalParameter = 0f;

    public Folder(string name, Folder parent = null)
    {
        this.name = name;
        this.parent = parent;
    }

    // ���� �Լ�: �̻� ���� ���θ� �Ķ���� ������� ���� ����
    public void AssignAbnormalByParameter()
    {
        isAbnormal = Random.value < abnormalParameter;
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
}
