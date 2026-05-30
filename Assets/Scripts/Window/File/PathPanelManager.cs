using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 파일 탐색기 상단 경로 패널 (breadcrumb).
/// 버튼마다 PathDropReceiver 컴포넌트를 붙여 드롭 이벤트를 처리한다.
/// EventTrigger를 직접 AddComponent하지 않으므로 리스너 누적이 없다.
/// </summary>
public class PathPanelManager : MonoBehaviour
{
    [SerializeField] public Transform contentArea;
    [SerializeField] public Button    pathButtonPrefab;

    private FileWindow         fileWindow;
    private readonly List<Button> pathButtons = new List<Button>();

    public void Initialize(FileWindow window) => fileWindow = window;

    public void UpdatePathButtons()
    {
        foreach (var btn in pathButtons) Destroy(btn.gameObject);
        pathButtons.Clear();

        var pathList = fileWindow.GetCurrentPathList();

        for (int i = 0; i < pathList.Count; i++)
        {
            int    idx = i;
            Button btn = Instantiate(pathButtonPrefab, contentArea);

            var text = btn.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = pathList[i].name;
                float w = text.preferredWidth + 20f;
                btn.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(w, btn.GetComponent<RectTransform>().sizeDelta.y);
            }

            btn.onClick.AddListener(() => fileWindow.NavigateToPathIndex(idx));

            // 드롭 수신 컴포넌트를 부착 (EventTrigger AddListener 누적 없음)
            var drop = btn.gameObject.AddComponent<PathDropReceiver>();
            drop.Initialize(idx, fileWindow);

            pathButtons.Add(btn);
        }
    }
}

/// <summary>
/// 경로 버튼 위에 드롭됐을 때 폴더/파일 이동을 처리하는 전용 컴포넌트.
/// 버튼 GameObject와 수명이 같으므로 리스너가 자동 정리된다.
/// </summary>
public class PathDropReceiver : MonoBehaviour, IDropHandler
{
    private int        pathIndex;
    private FileWindow fileWindow;

    public void Initialize(int index, FileWindow fw)
    {
        pathIndex  = index;
        fileWindow = fw;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var pathList = fileWindow.GetCurrentPathList();
        if (pathIndex < 0 || pathIndex >= pathList.Count) return;

        Folder target = pathList[pathIndex];

        // ── 폴더 드롭 ────────────────────────────────
        var draggedFolder = FolderDragManager.Instance?.CurrentDraggedFolderIcon?.GetFolder();
        if (draggedFolder != null)
        {
            // 자기 자신 또는 조상으로 이동 금지
            Folder temp = target;
            while (temp != null)
            {
                if (temp == draggedFolder)
                { LogWindowManager.Instance?.Log("자식 또는 자기 자신으로 이동할 수 없습니다."); return; }
                temp = temp.parent;
            }

            if (!FolderDepthUtility.CanMove(draggedFolder, target, out string warning))
            { LogWindowManager.Instance?.Log(warning); return; }

            draggedFolder.parent?.RemoveChild(draggedFolder);
            target.AddChild(draggedFolder);

            FolderDragManager.Instance.EndDrag();
            LogWindowManager.Instance?.Log($"폴더 '{draggedFolder.name}' → '{target.name}' 이동");
            StartCoroutine(OpenNextFrame(target));
            return;
        }

        // ── 파일 드롭 ────────────────────────────────
        var draggedFile = FolderDragManager.Instance?.CurrentDraggedFileIcon?.GetFile();
        if (draggedFile != null)
        {
            draggedFile.parent?.RemoveFile(draggedFile);
            target.AddFile(draggedFile);

            FolderDragManager.Instance.EndDrag();
            LogWindowManager.Instance?.Log($"파일 '{draggedFile.name}' → '{target.name}' 이동");
            fileWindow.RefreshWindow();
        }
    }

    private IEnumerator OpenNextFrame(Folder folder)
    {
        yield return null;
        fileWindow.OpenFolder(folder, false);
    }
}
