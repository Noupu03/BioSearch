using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 스캔 팝업의 TopBar를 드래그해서 위치 이동
/// </summary>
public class LoadingDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform topBar;          // 드래그용 TopBar
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
        // 클릭 위치와 팝업 위치 차이 저장
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        offset = localPoint - (Vector2)popupRect.localPosition;

        // 최상단으로
        popupRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (popupRect == null || parentCanvas == null) return;

        // 현재 마우스 위치 local 좌표로 변환
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            // offset 차이 적용해서 부드럽게 이동
            popupRect.localPosition = localPoint - offset;
        }
    }
}
