using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// TopBar를 드래그하여 지정된 부모 창을 이동시키는 스크립트
/// </summary>
public class OSTopbarDrag : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("드래그로 이동시킬 부모 창 (RectTransform)")]
    public RectTransform parentWindow;   // 인스펙터에서 수동 지정

    private Canvas rootCanvas;           // 상위 Canvas
    private Vector2 pointerOffset;       // 클릭 시 마우스 오프셋

    void Start()
    {
        // 자동으로 상위 캔버스 탐색 (한 번만)
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (parentWindow == null || rootCanvas == null)
            return;

        // 클릭 시 TopBar 클릭 위치와 부모창 pivot 사이의 거리 계산
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentWindow, eventData.position, eventData.pressEventCamera, out pointerOffset);

        // 클릭 시 창을 최상단으로 이동
        parentWindow.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentWindow == null || rootCanvas == null)
            return;

        Vector2 localPoint;
        // 캔버스 좌표계 기준으로 마우스 위치 계산
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            parentWindow.anchoredPosition = localPoint - pointerOffset;
        }
    }
}
