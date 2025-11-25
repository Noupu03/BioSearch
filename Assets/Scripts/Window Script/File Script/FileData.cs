using UnityEngine;

[System.Serializable]
public class FileData
{
    // 파일 이름 (확장자를 제외한 순수 이름)
    public string fileName;

    // 파일 확장자 (예: "txt", "png" 등)
    public string extension;

    // 텍스트 파일의 내용 (TextArea로 Inspector에서 여러 줄 입력 가능)
    [TextArea]
    public string textContent;

    // 이미지 파일의 내용 (Sprite로 참조)
    public Sprite imageContent;


    [Header("부모 폴더 이름 (Root 기준)")]
    // 부모 폴더 이름 (실제 Folder 객체 대신 문자열로 저장)
    // Inspector에서 직접 문자열 입력 가능
    public string parentFolderName;
}
