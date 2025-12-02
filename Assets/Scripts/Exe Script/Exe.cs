using UnityEngine;

[System.Serializable]
public class Exe
{
    public string name;         // exe 이름
    public Folder parent;       // 상위 폴더
    public string exeContent;   // 텍스트 기반 미니게임 내용

    public Exe() { }
    public Exe(string name, Folder parent = null, string exeContent = "")
    {
        this.name = name;
        this.parent = parent;
        this.exeContent = exeContent;
    }
}
