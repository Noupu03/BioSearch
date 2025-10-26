using UnityEngine;

/// <summary>
/// FileWindow ������ ���Ǵ� ���� ������ ������ Ŭ����.
/// ���� UI�� ������, �������� �������� ������ ���� ���� ������ ����.
/// </summary>
[System.Serializable]
public class DummyIcon
{
    public string name;          // ������ �̸�
    public bool isFolder;        // �������� ��������
    public Folder parentFolder;  // ���� �������� �θ� ����
    public GameObject uiObject;  // ���� UI GameObject
}
