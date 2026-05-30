using UnityEngine;

/// <summary>
/// extense 명령어를 처리하는 클래스
/// - 특정 파일의 확장자를 변경
/// </summary>
public class ExtenseCommandManager : MonoBehaviour
{
    [Header("References")]
    public FileWindow fileWindow;          // 확장자 변경 대상이 되는 파일 구조
    public LogWindowManager logWindow;     // 로그 출력 창

    private void OnEnable()
    {
        if (logWindow != null)
            logWindow.OnExtenseCommandEntered += HandleExtenseCommand;
    }

    private void OnDisable()
    {
        if (logWindow != null)
            logWindow.OnExtenseCommandEntered -= HandleExtenseCommand;
    }

    /// <summary>
    /// extense 명령어 입력 시 호출되는 처리 함수
    /// </summary>
    private void HandleExtenseCommand(string commandArgs)
    {
        // 인자 파싱: "파일명 확장자"
        string[] parts = commandArgs.Split(' ');
        if (parts.Length < 2)
        {
            logWindow.Log("사용법: extense [파일명] [새 확장자]");
            return;
        }

        string fileName = parts[0];
        string newExtension = parts[1];

        ChangeFileExtension(fileName, newExtension);
    }

    /// <summary>
    /// 파일의 확장자를 변경
    /// </summary>
    private void ChangeFileExtension(string fileName, string newExtension)
    {
        if (fileWindow == null)
        {
            logWindow.Log("FileWindow 참조가 없습니다.");
            return;
        }

        Folder root = fileWindow.GetRootFolder();
        File target = FindFileByName(root, fileName);

        if (target == null)
        {
            logWindow.Log($"파일 '{fileName}'을(를) 찾을 수 없습니다.");
            return;
        }

        string oldExt = target.extension;
        target.extension = newExtension;

        // 파일 이름과 동일한 팝업이 열려있다면 즉시 닫기
        if (FilePopupManager.Instance != null)
            FilePopupManager.Instance.ClosePopup(fileName);

        logWindow.Log($"'{target.name}' 파일 확장자 변경: {oldExt} → {newExtension}");

        fileWindow.RefreshWindow(); // UI 갱신
    }

    /// <summary>
    /// 파일 이름으로 재귀 검색
    /// </summary>
    private File FindFileByName(Folder folder, string name)
    {
        // 현재 폴더 내 파일 검색
        foreach (var file in folder.files)
        {
            if (file.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return file;
        }

        // 하위 폴더 재귀 탐색
        foreach (var child in folder.children)
        {
            var found = FindFileByName(child, name);
            if (found != null) return found;
        }

        return null;
    }
}
