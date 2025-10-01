using UnityEngine;

[System.Serializable]
public class File
{
    public string name;
    public string extension;

    public string textContent;
    public Sprite imageContent;

    public Folder parent;

    //  �ν����Ϳ��� ���� ������ �� �ִ� �̻� ����
    public bool isAbnormal = false;

    public File(string name, string extension, Folder parent = null, string textContent = null, Sprite imageContent = null, bool isAbnormal = false)
    {
        this.name = name;
        this.extension = extension;
        this.parent = parent;
        this.textContent = textContent;
        this.imageContent = imageContent;
        this.isAbnormal = isAbnormal;
    }
}
