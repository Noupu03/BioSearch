using UnityEngine;

[System.Serializable]
public class FileData
{
    public string fileName;
    public string extension;
    [TextArea] public string textContent;
    public Sprite imageContent;
    public bool isAbnormal = false;
    [Header("�θ� ���� �̸� (Root ����)")]
    public string parentFolderName; // Inspector���� ���ڿ��� �Է�

    
}
