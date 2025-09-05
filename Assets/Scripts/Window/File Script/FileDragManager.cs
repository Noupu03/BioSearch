using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class FileDragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static FileDragManager Instance;

    private FileIcon draggingIcon;
    private GameObject ghostIcon;
    private Canvas mainCanvas;

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
        Debug.Log("�巡�� ����");
        draggingIcon = eventData.pointerDrag?.GetComponent<FileIcon>();
        if (draggingIcon == null) return;

        CreateGhost(eventData);
    }

    // �巡�� ��
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("�巡����");
        if (ghostIcon != null)
            UpdateGhostPosition(eventData);
    }

    // �巡�� ����
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("�������");
        EndDrag();
    }

    // ���� EndDrag�� ���������� �����ϰ� ȣ�� �����ϵ��� public
    public void ForceEndDrag()
    {
        if (ghostIcon != null)
        {
            Debug.Log("����");
            Destroy(ghostIcon);
            ghostIcon = null;
        }
        draggingIcon = null;
    }

    private void CreateGhost(PointerEventData eventData)
    {
        // GhostIcon ����
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

        // �ؽ�Ʈ �߰�
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

    // �巡�� ���� �� ��� Ghost ����
    public void EndDrag()
    {
        if (ghostIcon != null)
        {
            Debug.Log("����");
            Destroy(ghostIcon);
            ghostIcon = null;
        }
        draggingIcon = null;
    }

    void Update()
    {
        // draggingIcon�� null�ε� ghostIcon�� ���������� �����ϰ� ����
        if (ghostIcon != null && draggingIcon == null)
        {
            Destroy(ghostIcon);
            ghostIcon = null;
        }
    }

    public FileIcon GetDraggingIcon() => draggingIcon;
}
