using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// FolderDragManager
/// - 폴더 및 파일 드래그 동작을 총괄 관리하는 매니저
/// - 드래그 시작/이동/종료를 통제하고, 화면에 "유령 아이콘(Ghost)"을 표시함
/// - 싱글톤으로 동작하여 전역에서 접근 가능
/// </summary>
public class FolderDragManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static FolderDragManager Instance;

    // 드래그 시 따라다니는 유령 아이콘 오브젝트
    private GameObject ghostIcon;

    // UI의 루트 캔버스 (UI 좌표 변환용)
    private Canvas mainCanvas;

    // 현재 드래그 중인 폴더 아이콘
    public FolderIcon CurrentDraggedFolderIcon { get; private set; }

    // 현재 드래그 중인 파일 아이콘
    public FileIcon CurrentDraggedFileIcon { get; private set; }

    void Awake()
    {
        // 싱글톤 초기화
        Instance = this;

        // 메인 캔버스 자동 탐색
        mainCanvas = FindObjectOfType<Canvas>();

        // 캔버스가 존재하지 않으면 경고 출력
        if (mainCanvas == null)
            Debug.LogError("씬에 Canvas 필요!");
    }

    #region 드래그 통제

    /// <summary>
    /// 폴더 드래그 시작 시 호출됨
    /// </summary>
    public void BeginDrag(FolderIcon folderIcon, PointerEventData eventData)
    {
        CurrentDraggedFolderIcon = folderIcon;
        CurrentDraggedFileIcon = null;
        CreateGhost(folderIcon.GetFolder().name, eventData);
    }

    /// <summary>
    /// 파일 드래그 시작 시 호출됨
    /// </summary>
    public void BeginDrag(FileIcon fileIcon, PointerEventData eventData)
    {
        CurrentDraggedFolderIcon = null;
        CurrentDraggedFileIcon = fileIcon;
        CreateGhost($"{fileIcon.GetFile().name}.{fileIcon.GetFile().extension}", eventData);
    }

    /// <summary>
    /// 드래그 중 마우스 위치가 이동할 때 호출됨
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
            UpdateGhostPosition(eventData);
    }

    /// <summary>
    /// 드래그 종료 시 호출됨
    /// </summary>
    public void EndDrag()
    {
        DestroyGhost();
    }

    /// <summary>
    /// 외부에서 드래그를 강제 종료해야 할 때 호출됨
    /// </summary>
    public void ForceEndDrag()
    {
        DestroyGhost();
    }

    /// <summary>
    /// 유령 아이콘 제거 및 상태 초기화
    /// </summary>
    private void DestroyGhost()
    {
        if (ghostIcon != null)
            Destroy(ghostIcon);

        ghostIcon = null;
        CurrentDraggedFolderIcon = null;
        CurrentDraggedFileIcon = null;
    }

    #endregion

    #region Ghost 생성/위치

    /// <summary>
    /// 드래그 시 마우스를 따라다니는 유령 아이콘 생성
    /// </summary>
    /// <param name="name">표시할 폴더/파일 이름</param>
    /// <param name="eventData">마우스 이벤트 정보</param>
    private void CreateGhost(string name, PointerEventData eventData)
    {
        // Ghost 아이콘 오브젝트 생성 (Image 포함)
        ghostIcon = new GameObject("GhostIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ghostIcon.transform.SetParent(mainCanvas.transform, false);
        ghostIcon.transform.SetAsLastSibling(); // 항상 맨 위에 표시

        // 반투명 배경 이미지 설정
        Image img = ghostIcon.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.5f); // 흰색 반투명
        img.raycastTarget = false; // 클릭 불가로 설정

        // RectTransform 크기 설정
        RectTransform rt = ghostIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 40);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // 텍스트 오브젝트 생성 (폴더/파일 이름 표시)
        TextMeshProUGUI txt = new GameObject("GhostText", typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        txt.text = name;
        txt.fontSize = 24;
        txt.color = Color.yellow; // 노란색 글씨
        txt.alignment = TextAlignmentOptions.Center;
        txt.raycastTarget = false;
        txt.transform.SetParent(ghostIcon.transform, false);

        // 텍스트를 Ghost 아이콘 전체에 맞게 확장
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;

        // 초기 위치 업데이트
        UpdateGhostPosition(eventData);
    }

    /// <summary>
    /// 유령 아이콘을 마우스 위치에 따라 이동
    /// </summary>
    private void UpdateGhostPosition(PointerEventData eventData)
    {
        Camera cam = mainCanvas.worldCamera; // 캔버스에 설정된 카메라 참조
        Vector3 worldPos;

        // 스크린 좌표 → 월드 좌표 변환
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            mainCanvas.transform as RectTransform,
            eventData.position,
            cam,
            out worldPos
        );

        // Ghost 아이콘의 위치 갱신
        ghostIcon.GetComponent<RectTransform>().position = worldPos;
    }

    #endregion
}
