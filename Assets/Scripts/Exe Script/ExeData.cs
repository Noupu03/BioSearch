using UnityEngine;

[System.Serializable]
public class ExeData
{
    public string exeName;    // EXE 이름 (확장자 X)
    public string parentFolderName;  // 상위 폴더 (문자열)
    [TextArea]
    public string exeContent; // 스크립트/게임/텍스트 내용
}
