using UnityEngine;

/// <summary>
/// 'extense 파일명 확장자' 명령어를 처리한다.
/// FileWindow와 LogWindowManager는 Instance로 접근하므로
/// 인스펙터 크로스 참조가 없다.
/// </summary>
public class ExtenseCommandManager : MonoBehaviour
{
    void OnEnable()
    {
        if (LogWindowManager.Instance != null)
            LogWindowManager.Instance.OnExtenseCommandEntered += HandleExtenseCommand;
    }

    void OnDisable()
    {
        if (LogWindowManager.Instance != null)
            LogWindowManager.Instance.OnExtenseCommandEntered -= HandleExtenseCommand;
    }

    private void HandleExtenseCommand(string args)
    {
        var log = LogWindowManager.Instance;
        string[] parts = args.Split(' ');
        if (parts.Length < 2)
        {
            log?.Log("사용법: extense [파일명] [새 확장자]");
            return;
        }
        ChangeFileExtension(parts[0], parts[1]);
    }

    private void ChangeFileExtension(string fileName, string newExt)
    {
        var log = LogWindowManager.Instance;
        var fw  = FileWindow.Instance;

        if (fw == null) { log?.Log("FileWindow가 없습니다."); return; }

        File target = FindFileByName(fw.GetRootFolder(), fileName);
        if (target == null) { log?.Log($"'{fileName}' 파일을 찾을 수 없습니다."); return; }

        string oldExt = target.extension;
        target.extension = newExt;

        FilePopupManager.Instance?.ClosePopup(fileName);
        log?.Log($"'{target.name}' 확장자 변경: {oldExt} → {newExt}");
        fw.RefreshWindow();
    }

    private static File FindFileByName(Folder folder, string name)
    {
        foreach (var file in folder.files)
            if (file.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return file;

        foreach (var child in folder.children)
        {
            var found = FindFileByName(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
