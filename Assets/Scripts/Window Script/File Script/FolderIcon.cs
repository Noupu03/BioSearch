using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// FolderIcon 클래스
/// - 폴더 아이콘의 시각적 표시, 선택 상태, 드래그 & 드롭, 클릭 이벤트 등을 관리함
/// - FileWindow 및 Folder 객체와 연동되어 작동함
/// </summary>
public class FolderIcon : MonoBehaviour, IPointerClickHandler, IDropHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 폴더 이름을 표시하는 TMP 텍스트
    public TMP_Text fileNameText;

    // 폴더가 속한 FileWindow 참조
    private FileWindow fileWindow;

    // 실제 폴더 데이터(Folder 클래스)
    private Folder folder;

    // 선택되지 않았을 때의 색상
    private Color normalColor = Color.white;

    // 선택되었을 때의 색상
    private Color selectedColor = Color.yellow;

    /// <summary>
    /// FolderIcon 초기 설정
    /// </summary>
    /// <param name="folder">연결할 Folder 객체</param>
    /// <param name="window">연결할 FileWindow 객체</param>
    /// <param name="parentAbnormal">상위 폴더가 비정상 상태인지 여부</param>
    public void Setup(Folder folder, FileWindow window, bool parentAbnormal = false)
    {
        this.folder = folder;
        this.fileWindow = window;

        // 현재 폴더 또는 부모 폴더 중 하나라도 비정상(abnormal)일 경우
        bool isAbnormal = parentAbnormal || folder.isAbnormal;

        // 폴더 이름 표시
        if (fileNameText != null)
            fileNameText.text = folder.name;

        // 초기에는 선택되지 않은 상태
        SetSelected(false);

        // 비정상 폴더의 경우 폰트 색상을 빨간색으로 표시
        if (fileNameText != null && isAbnormal)
            fileNameText.color = Color.red;
    }

    /// <summary>
    /// 현재 아이콘이 참조하는 Folder 객체 반환
    /// </summary>
    public Folder GetFolder() => folder;

    /// <summary>
    /// 선택 상태를 표시 색상으로 반영
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (fileNameText == null) return;
        if (folder != null && folder.isAbnormal) return; // 비정상 폴더는 색상 변경 X
        fileNameText.color = selected ? selectedColor : normalColor;
    }

    /// <summary>
    /// 폴더 아이콘 클릭 시 동작
    /// - 한 번 클릭: 선택
    /// - 두 번 클릭: 폴더 열기
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        fileWindow.SetSelectedIcon(this); // 선택 처리

        // 더블 클릭 시 폴더 열기
        if (eventData.clickCount == 2)
            fileWindow.OpenFolder(folder);
    }

    #region 드래그 구현

    /// <summary>
    /// 드래그 시작 시 호출
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.BeginDrag(this, eventData);
    }

    /// <summary>
    /// 드래그 중 마우스 이동 처리
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.OnDrag(eventData);
    }

    /// <summary>
    /// 드래그 종료 시 처리
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.EndDrag();
    }

    #endregion

    /// <summary>
    /// 폴더 위로 다른 아이콘이 드롭될 때 처리
    /// - 폴더를 드롭한 경우: 폴더 이동
    /// - 파일을 드롭한 경우: 파일 이동
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // -----------------------------
        // 1. 폴더 드롭 처리
        // -----------------------------
        FolderIcon draggedFolderIcon = eventData.pointerDrag?.GetComponent<FolderIcon>();
        if (draggedFolderIcon != null)
        {
            Folder source = draggedFolderIcon.GetFolder(); // 이동되는 폴더
            Folder target = folder; // 드롭 대상 폴더

            string warning;
            // 폴더 이동 가능 여부 검사 (순환 참조 등 방지)
            if (!FolderDepthUtility.CanMove(source, target, out warning))
            {
                LogWindowManager.Instance.Log(warning);
                return;
            }

            // 기존 부모 폴더에서 제거
            if (source.parent != null)
                source.parent.children.Remove(source);

            // 새 부모 폴더에 추가
            target.children.Add(source);
            source.parent = target;

            // 로그 출력
            LogWindowManager.Instance.Log($"폴더 '{source.name}' → '{target.name}' 이동됨");

            // 드래그 강제 종료 및 UI 업데이트
            FolderDragManager.Instance.ForceEndDrag();
            fileWindow.StartCoroutine(OpenFolderNextFrame(target));
            return;
        }

        // -----------------------------
        // 2. 파일 드롭 처리
        // -----------------------------
        FileIcon draggedFileIcon = eventData.pointerDrag?.GetComponent<FileIcon>();
        if (draggedFileIcon != null)
        {
            File file = draggedFileIcon.GetFile(); // 이동되는 파일
            Folder target = folder; // 드롭 대상 폴더

            // 기존 부모 폴더에서 제거
            if (file.parent != null)
                file.parent.files.Remove(file);

            // 새 부모 폴더에 추가
            target.files.Add(file);
            file.parent = target;

            // 로그 출력
            LogWindowManager.Instance.Log($"파일 '{file.name}.{file.extension}' → '{target.name}' 이동됨");

            // 드래그 강제 종료 및 UI 갱신
            FolderDragManager.Instance.ForceEndDrag();
            fileWindow.RefreshWindow(); // 파일 이동 후 UI 갱신
        }
    }

    /// <summary>
    /// 다음 프레임에 폴더를 여는 코루틴
    /// - 즉시 열면 드래그 이벤트 처리와 충돌 가능성이 있음
    /// </summary>
    private IEnumerator OpenFolderNextFrame(Folder target)
    {
        yield return null; // 한 프레임 대기
        fileWindow.OpenFolder(target, false);
    }
    // FolderIcon.cs
    public void SetupDummy(string dummyName)
    {
        if (fileNameText != null)
            fileNameText.text = dummyName;

        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = false;
    }


}
