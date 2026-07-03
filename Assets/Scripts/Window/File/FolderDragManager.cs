using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Haare.Client.Routine;

/// <summary>
/// 드래그 앤 드롭 고스트 아이콘을 관리하는 싱글톤.
///
/// Ghost는 프로그래매틱하게 생성하며, ghostMaterial을 Inspector에서 할당하면
/// 게임에서 쓰는 커스텀 쉐이더가 그대로 적용된다 (미할당 시 기본 UI 쉐이더).
/// OnDisable / OnApplicationFocus / Update 에서 잔상을 강제 정리한다.
/// </summary>
public class FolderDragManager : MonoRoutine
{
    public static FolderDragManager Instance { get; private set; }

    public FolderIcon CurrentDraggedFolderIcon { get; private set; }
    public FileIcon   CurrentDraggedFileIcon   { get; private set; }

    // Ghost Image에 적용할 머티리얼. 미할당 시 기본 UI 쉐이더 사용.
    [SerializeField] private Material ghostMaterial;

    // null이면 부모 계층에서 자동 탐색
    [SerializeField] private Canvas mainCanvas;

    private GameObject ghostIcon;

    // ──────────────────────────────────────────────
    //  생명주기
    // ──────────────────────────────────────────────

    protected override void Constructor()
    {
        base.Constructor();
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCanvas == null)
            mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
            Debug.LogError("[FolderDragManager] Canvas를 찾을 수 없습니다.");
    }

    // MonoRoutine도 private OnDestroy()를 정의하므로(Awake와 같은 문제), Instance 해제는
    // 기존 OnDisable(ghost 정리)에 같이 넣는다.
    void OnDisable()
    {
        DestroyGhost();
        if (Instance == this) Instance = null;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) DestroyGhost();
    }

    protected override void UpdateProcess()
    {
        base.UpdateProcess();
        // 마우스 버튼이 이미 떼어진 상태인데 ghost가 남아있으면 정리
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
        CreateGhost(folderIcon.GetFolder().name, eventData);
    }

    public void BeginDrag(FileIcon fileIcon, PointerEventData eventData)
    {
        CurrentDraggedFolderIcon = null;
        CurrentDraggedFileIcon   = fileIcon;
        CreateGhost($"{fileIcon.GetFile().name}.{fileIcon.GetFile().extension}", eventData);
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

    private void CreateGhost(string label, PointerEventData eventData)
    {
        if (mainCanvas == null) return;
        DestroyGhost(); // 중복 방지

        ghostIcon = new GameObject("GhostIcon",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ghostIcon.transform.SetParent(mainCanvas.transform, false);
        ghostIcon.transform.SetAsLastSibling();

        var rt       = ghostIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 40f);
        rt.pivot     = new Vector2(0.5f, 0.5f);

        var img           = ghostIcon.GetComponent<Image>();
        img.raycastTarget = false;
        if (ghostMaterial != null)
            img.material = ghostMaterial;

        // CanvasGroup으로 반투명 + 이벤트 차단
        var cg              = ghostIcon.AddComponent<CanvasGroup>();
        cg.alpha            = 0.6f;
        cg.blocksRaycasts   = false;
        cg.interactable     = false;

        var txtGo  = new GameObject("GhostText", typeof(RectTransform), typeof(TextMeshProUGUI));
        var txt    = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text                    = label;
        txt.fontSize                = 24;
        txt.color                   = Color.yellow;
        txt.alignment               = TextAlignmentOptions.Center;
        txt.raycastTarget           = false;
        txt.transform.SetParent(ghostIcon.transform, false);
        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;

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
