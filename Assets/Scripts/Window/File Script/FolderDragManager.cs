using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class FolderDragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static FolderDragManager Instance;

    private FolderIcon draggingIcon;
    private GameObject ghostIcon;
    private Canvas mainCanvas;

    // ���� Drag ���� Folder
    public Folder CurrentDraggedFolder { get; private set; }

    void Awake()
    {
        Instance = this;
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
            Debug.LogError("���� Canvas �ʿ�!");
    }

    // �巡�� ����
    public void OnBeginDrag(PointerEventData eventData)
    {
        draggingIcon = eventData.pointerDrag?.GetComponent<FolderIcon>();
        if (draggingIcon == null) return;

        CurrentDraggedFolder = draggingIcon.GetFolder();
        CreateGhost(eventData);
    }

    // �巡�� ��
    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
            UpdateGhostPosition(eventData);
    }

    // �巡�� ����
    public void OnEndDrag(PointerEventData eventData)
    {
        EndDrag();
    }

    // Ghost ����
    private void CreateGhost(PointerEventData eventData)
    {
        ghostIcon = new GameObject("GhostIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ghostIcon.transform.SetParent(mainCanvas.transform, false);
        ghostIcon.transform.SetAsLastSibling();

        Image img = ghostIcon.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.5f);
        img.raycastTarget = false;

        CanvasGroup group = ghostIcon.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        RectTransform rt = ghostIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120, 40);
        rt.pivot = new Vector2(0.5f, 0.5f);

        TextMeshProUGUI txt = new GameObject("GhostText", typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        txt.text = draggingIcon.GetFolder().name;
        txt.fontSize = 24;
        txt.color = Color.yellow;
        txt.alignment = TextAlignmentOptions.Center;
        txt.raycastTarget = false;
        txt.transform.SetParent(ghostIcon.transform, false);

        txt.rectTransform.anchorMin = Vector2.zero;
        txt.rectTransform.anchorMax = Vector2.one;
        txt.rectTransform.offsetMin = Vector2.zero;
        txt.rectTransform.offsetMax = Vector2.zero;

        UpdateGhostPosition(eventData);
    }

    // Ghost ��ġ ������Ʈ
    private void UpdateGhostPosition(PointerEventData eventData)
    {
        Camera cam = mainCanvas.worldCamera;
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            mainCanvas.transform as RectTransform,
            eventData.position,
            cam,
            out worldPos
        );
        ghostIcon.GetComponent<RectTransform>().position = worldPos;
    }

    // �巡�� ���� �� Ghost ���� �� ���� �ʱ�ȭ
    private void EndDrag()
    {
        if (ghostIcon != null)
        {
            Destroy(ghostIcon);
            ghostIcon = null;
        }
        draggingIcon = null;
        CurrentDraggedFolder = null;
    }

    // ���� �巡�� ���� (Drop �� ȣ��)
    public void ForceEndDrag()
    {
        EndDrag();
    }

    void Update()
    {
        // ���� ��ġ: draggingIcon�� null�ε� ghost�� ���������� ����
        if (ghostIcon != null && draggingIcon == null)
        {
            Destroy(ghostIcon);
            ghostIcon = null;
        }
    }

    public FolderIcon GetDraggingIcon() => draggingIcon;
}
