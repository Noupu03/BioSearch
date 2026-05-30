using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 드래그 앤 드롭 고스트 아이콘을 관리하는 싱글톤.
/// Canvas는 컴포넌트 계층에서 자동으로 찾으므로 인스펙터 참조 불필요.
/// </summary>
public class FolderDragManager : MonoBehaviour
{
    public static FolderDragManager Instance { get; private set; }

    public FolderIcon CurrentDraggedFolderIcon { get; private set; }
    public FileIcon   CurrentDraggedFileIcon   { get; private set; }

    private GameObject ghostIcon;
    private Canvas     mainCanvas;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        // 자신의 계층에서 Canvas 탐색 (Canvas 하위에 배치돼 있다고 가정)
        mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
            Debug.LogError("[FolderDragManager] 부모 계층에 Canvas가 없습니다.");
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

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

    private void DestroyGhost()
    {
        if (ghostIcon != null) Destroy(ghostIcon);
        ghostIcon                = null;
        CurrentDraggedFolderIcon = null;
        CurrentDraggedFileIcon   = null;
    }

    private void CreateGhost(string label, PointerEventData eventData)
    {
        if (mainCanvas == null) return;

        ghostIcon = new GameObject("GhostIcon",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ghostIcon.transform.SetParent(mainCanvas.transform, false);
        ghostIcon.transform.SetAsLastSibling();

        var img = ghostIcon.GetComponent<Image>();
        img.color         = new Color(1f, 1f, 1f, 0.5f);
        img.raycastTarget = false;

        var rt = ghostIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 40f);
        rt.pivot     = new Vector2(0.5f, 0.5f);

        var txt = new GameObject("GhostText",
            typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        txt.text                       = label;
        txt.fontSize                   = 24;
        txt.color                      = Color.yellow;
        txt.alignment                  = TextAlignmentOptions.Center;
        txt.raycastTarget              = false;
        txt.transform.SetParent(ghostIcon.transform, false);
        txt.rectTransform.anchorMin    = Vector2.zero;
        txt.rectTransform.anchorMax    = Vector2.one;
        txt.rectTransform.offsetMin    = Vector2.zero;
        txt.rectTransform.offsetMax    = Vector2.zero;

        UpdateGhostPosition(eventData);
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
