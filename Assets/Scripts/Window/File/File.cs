using UnityEngine;

[System.Serializable]
public class File
{
    public string name       { get; }
    public string extension  { get; set; }  // ExtenseCommandManager가 변경 가능
    public string textContent  { get; }
    public Sprite imageContent { get; }
    public bool   isAbnormal   { get; set; }

    // Folder.AddFile / RemoveFile 에서만 설정
    public Folder parent { get; internal set; }

    public File(string name, string extension, Folder parent = null,
                string textContent = null, Sprite imageContent = null, bool isAbnormal = false)
    {
        this.name         = name;
        this.extension    = extension;
        this.parent       = parent;
        this.textContent  = textContent;
        this.imageContent = imageContent;
        this.isAbnormal   = isAbnormal;
    }
}
