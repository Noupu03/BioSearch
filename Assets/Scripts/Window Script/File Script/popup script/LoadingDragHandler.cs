using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ��ĵ �˾��� TopBar�� �巡���ؼ� ��ġ �̵�
/// </summary>
public class LoadingDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform topBar;          // �巡�׿� TopBar
    private RectTransform popupRect;
    private Canvas parentCanvas;
    private Vector2 offset;

    private void Awake()
    {
        popupRect = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Ŭ�� ��ġ�� �˾� ��ġ ���� ����
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        offset = localPoint - (Vector2)popupRect.localPosition;

        // �ֻ������
        popupRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (popupRect == null || parentCanvas == null) return;

        // ���� ���콺 ��ġ local ��ǥ�� ��ȯ
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            // offset ���� �����ؼ� �ε巴�� �̵�
            popupRect.localPosition = localPoint - offset;
        }
    }
}
