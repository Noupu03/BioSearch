using UnityEngine;

/// <summary>
/// FileWindow 내에서 사용되는 더미 아이콘 데이터 클래스.
/// 실제 UI를 가지며, 폴더인지 파일인지 정보와 원본 폴더 참조를 포함.
/// </summary>
[System.Serializable]
public class DummyIcon
{
    public string name;          // 아이콘 이름
    public bool isFolder;        // 폴더인지 파일인지
    public Folder parentFolder;  // 원본 아이콘의 부모 폴더
    public GameObject uiObject;  // 실제 UI GameObject
}
