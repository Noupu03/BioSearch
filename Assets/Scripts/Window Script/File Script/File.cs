using UnityEngine;
public class File
{
    public string name;
    public string extension;
    public Folder parent;

    // ���� ������
    public string textContent;   // txt�� ���
    public Sprite imageContent;  // png�� ���

    public File(string name, string extension, Folder parent, string textContent = null, Sprite imageContent = null)
    {
        this.name = name;
        this.extension = extension;
        this.parent = parent;
        this.textContent = textContent;
        this.imageContent = imageContent;
    }

}
