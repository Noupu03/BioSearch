using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PathPanelManager : MonoBehaviour
{
    // 경로 버튼들이 배치될 부모 오브젝트 (예: HorizontalLayoutGroup이 있는 영역)
    public Transform contentArea;

    // 경로 버튼 프리팹 (예: "Root > SubFolder1 > SubFolder2" 버튼 중 하나의 기본 형태)
    public Button pathButtonPrefab;

    // 현재 파일 창 참조 (경로 이동 시 활용)
    private FileWindow fileWindow;

    // 생성된 경로 버튼들을 관리하는 리스트
    private List<Button> pathButtons = new List<Button>();

    // FileWindow 연결 (초기화 시 호출)
    public void Initialize(FileWindow window)
    {
        fileWindow = window;
    }

    // 현재 폴더의 경로 버튼들을 갱신 (예: Root > Documents > Images)
    public void UpdatePathButtons()
    {
        // 기존 버튼 제거
        foreach (var btn in pathButtons)
            Destroy(btn.gameObject);
        pathButtons.Clear();

        // 현재 경로 폴더 리스트 가져오기
        List<Folder> pathList = fileWindow.GetCurrentPathList();

        // 각 폴더 경로마다 버튼 생성
        for (int i = 0; i < pathList.Count; i++)
        {
            int index = i; // 클로저 문제 방지
            Button btn = Instantiate(pathButtonPrefab, contentArea);
            TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
            text.text = pathList[i].name;

            // 텍스트 길이에 따라 버튼 폭 자동 조정
            float width = text.preferredWidth + 20f;
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(width, btn.GetComponent<RectTransform>().sizeDelta.y);

            // 클릭 시 해당 경로로 이동
            btn.onClick.AddListener(() =>
            {
                fileWindow.NavigateToPathIndex(index);
            });

            // 드롭 이벤트 등록 (파일 또는 폴더를 버튼 위에 드롭할 경우)
            EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.Drop };
            entry.callback.AddListener((data) =>
            {
                OnPathDrop(index, (PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            // 생성된 버튼 리스트에 저장
            pathButtons.Add(btn);
        }
    }

    // 경로 버튼 위로 드롭될 때 실행되는 처리
    private void OnPathDrop(int index, PointerEventData eventData)
    {
        Folder draggedFolder = null;
        File draggedFile = null;

        // 1. FolderDragManager에서 현재 드래그 중인 Folder 확인
        if (FolderDragManager.Instance.CurrentDraggedFolderIcon != null)
        {
            draggedFolder = FolderDragManager.Instance.CurrentDraggedFolderIcon.GetFolder();
        }
        // 2. pointerDrag로부터 직접 가져오기
        else if (eventData.pointerDrag != null)
        {
            FolderIcon folderIcon = eventData.pointerDrag.GetComponent<FolderIcon>();
            if (folderIcon != null)
                draggedFolder = folderIcon.GetFolder();
            else
            {
                // pointerDrag가 FileIcon이면 가져오기
                FileIcon fileIcon = eventData.pointerDrag.GetComponent<FileIcon>();
                if (fileIcon != null)
                    draggedFile = fileIcon.GetFile();
            }
        }

        // 유효한 인덱스 범위 확인
        List<Folder> pathList = fileWindow.GetCurrentPathList();
        if (index < 0 || index >= pathList.Count) return;

        Folder targetFolder = pathList[index];

        // 폴더 이동 처리
        if (draggedFolder != null)
        {
            // 자기 자신이나 하위 폴더로 드롭 방지
            Folder temp = targetFolder;
            while (temp != null)
            {
                if (temp == draggedFolder)
                {
                    LogWindowManager.Instance.Log("자신 또는 하위 폴더에는 드롭할 수 없습니다.");
                    return;
                }
                temp = temp.parent;
            }

            // 이동 가능 여부 검사 (깊이 제한 등)
            string warning;
            if (!FolderDepthUtility.CanMove(draggedFolder, targetFolder, out warning))
            {
                LogWindowManager.Instance.Log(warning);
                return;
            }

            // 기존 부모에서 제거
            if (draggedFolder.parent != null)
                draggedFolder.parent.children.Remove(draggedFolder);

            // 새 부모에 추가
            targetFolder.children.Add(draggedFolder);
            draggedFolder.parent = targetFolder;

            // 드래그 종료 및 로그 출력
            FolderDragManager.Instance.EndDrag();
            LogWindowManager.Instance.Log($"폴더 '{draggedFolder.name}' → '{targetFolder.name}' 이동됨");
        }
        // 파일 이동 처리
        else if (draggedFile != null)
        {
            // 기존 부모에서 제거
            if (draggedFile.parent != null)
                draggedFile.parent.files.Remove(draggedFile);

            // 새 부모에 추가
            draggedFile.parent = targetFolder;
            targetFolder.files.Add(draggedFile);

            // 드래그 종료 및 로그 출력
            FolderDragManager.Instance.EndDrag();
            LogWindowManager.Instance.Log($"파일 '{draggedFile.name}' → '{targetFolder.name}' 이동됨");
        }

        // 한 프레임 후 UI 갱신 (즉시 호출 시 UI 충돌 방지)
        fileWindow.StartCoroutine(OpenFolderNextFrame(targetFolder));
    }

    // 한 프레임 뒤 해당 폴더 열기 (UI 안정성 확보용)
    private IEnumerator OpenFolderNextFrame(Folder folder)
    {
        yield return null; // 한 프레임 대기
        fileWindow.OpenFolder(folder, false);
    }
}
