using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 드래그 앤 드롭 고스트 아이콘을 관리하는 싱글톤.
///
/// 원본 아이콘을 복제해 고스트로 사용하므로 쉐이더/머티리얼이 자동으로 일치한다.
/// CanvasGroup.blocksRaycasts = false 로 드롭 이벤트를 방해하지 않는다.
/// OnDisable / OnApplicationFocus / Update 에서 잔상을 강제 정리한다.
/// </summary>
public class FolderDragManager : MonoBehaviour
{
    public static FolderDragManager Instance { get; private set; }

    public FolderIcon CurrentDraggedFolderIcon { get; private set; }
    public FileIcon   CurrentDraggedFileIcon   { get; private set; }

    // Inspector에서 직접 할당 가능; null이면 부모 계층에서 자동 탐색
    [SerializeField] private Canvas mainCanvas;

    private GameObject ghostIcon;

    // ──────────────────────────────────────────────
    //  생명주기
    // ──────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCanvas == null)
            mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
            Debug.LogError("[FolderDragManager] Canvas를 찾을 수 없습니다.");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnDisable() => DestroyGhost();

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) DestroyGhost();
    }

    void Update()
    {
        // 드래그 중 마우스 버튼이 이미 떼어진 경우(포커스 복귀 등) 잔상 정리
        if (ghostIcon != null && !Input.GetMouseButton(0))
            DestroyGhost();
    }

    // ──────────────────────────────────────────────
    //  공개 API
    // ──────────────────────────────────────────────

    public void BeginDrag(FolderIcon folderIcon, PointerEventData eventData)
    {
        CurrentDraggedFolderIcon = folderIcon;
        CurrentDraggedFileIcon   = null;
        SpawnGhost(folderIcon.gameObject, eventData);
    }

    public void BeginDrag(FileIcon fileIcon, PointerEventData eventData)
    {
        CurrentDraggedFolderIcon = null;
        CurrentDraggedFileIcon   = fileIcon;
        SpawnGhost(fileIcon.gameObject, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null) UpdateGhostPosition(eventData);
    }

    public void EndDrag()      => DestroyGhost();
    public void ForceEndDrag() => DestroyGhost();

    // ──────────────────────────────────────────────
    //  내부 구현
    // ──────────────────────────────────────────────

    private void SpawnGhost(GameObject source, PointerEventData eventData)
    {
        if (mainCanvas == null) return;
        DestroyGhost(); // 중복 생성 방지

        // 원본 아이콘 복제 → 쉐이더·머티리얼 자동 일치
        ghostIcon      = Instantiate(source, mainCanvas.transform);
        ghostIcon.name = "GhostIcon";
        ghostIcon.transform.SetAsLastSibling();

        // 게임 로직 스크립트 비활성화 (Start 등록 및 클릭 이벤트 차단)
        foreach (var fi in ghostIcon.GetComponentsInChildren<FolderIcon>(true))
            fi.enabled = false;
        foreach (var fi in ghostIcon.GetComponentsInChildren<FileIcon>(true))
            fi.enabled = false;

        // 이벤트 수신 완전 차단 + 반투명 처리
        var cg              = ghostIcon.AddComponent<CanvasGroup>();
        cg.alpha            = 0.6f;
        cg.blocksRaycasts   = false;
        cg.interactable     = false;

        UpdateGhostPosition(eventData);
    }

    private void DestroyGhost()
    {
        if (ghostIcon != null) Destroy(ghostIcon);
        ghostIcon                = null;
        CurrentDraggedFolderIcon = null;
        CurrentDraggedFileIcon   = null;
    }

    private void UpdateGhostPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            mainCanvas.transform as RectTransform,
            eventData.position,
            mainCanvas.worldCamera,
            out Vector3 worldPos);

        ghostIcon.GetComponent<RectTransform>().position = worldPos;
    }
}
