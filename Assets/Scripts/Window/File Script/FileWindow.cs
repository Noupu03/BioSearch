using UnityEngine;
using System.Collections.Generic;

public class FileWindow : MonoBehaviour
{
    public Transform contentArea;       // ScrollView �� Content
    public GameObject fileIconPrefab;   // ������ ������

    [HideInInspector] public FileIcon selectedIcon = null; // ���� ���õ� ������

    private List<FileIcon> fileIcons = new List<FileIcon>();

    void Start()
    {
        // �ӽ� �׽�Ʈ ���ϵ� ����
        CreateFile("head");
        CreateFile("body");
        CreateFile("leg");
        CreateFile("arm");
        CreateFile("hand");
        CreateFile("organ");
    }

    void CreateFile(string fileName)
    {
        GameObject obj = Instantiate(fileIconPrefab, contentArea);
        FileIcon icon = obj.GetComponent<FileIcon>();
        icon.Setup(fileName, this); // FileWindow ���� ����
        fileIcons.Add(icon);
    }
}
