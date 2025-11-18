using UnityEngine;

[System.Serializable]
public class File
{
    // 파일 이름 (확장자를 제외한 이름)
    public string name;

    // 파일 확장자 (예: "txt", "png" 등)
    public string extension;

    // 텍스트 파일의 내용 (텍스트 파일일 경우 사용)
    public string textContent;

    // 이미지 파일의 내용 (이미지 파일일 경우 사용)
    public Sprite imageContent;

    // 상위 폴더를 참조 (파일이 포함된 폴더)
    public Folder parent;

    //  인스펙터에서 직접 지정할 수 있는 이상 여부
    //  true일 경우, 비정상 파일로 표시됨
    public bool isImportant = false;

    // 파일 생성자
    // name : 파일 이름
    // extension : 확장자
    // parent : 부모 폴더
    // textContent : 텍스트 파일 내용
    // imageContent : 이미지 파일 내용
    // isImportant : 이상 여부 (기본값 false)
    public File(string name, string extension, Folder parent = null, string textContent = null, Sprite imageContent = null, bool isImportant = false)
    {
        this.name = name;
        this.extension = extension;
        this.parent = parent;
        this.textContent = textContent;
        this.imageContent = imageContent;
        this.isImportant = isImportant;
    }
}
