using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 팝업 창을 드래그로 이동시키는 재사용 가능한 컴포넌트.
/// 이동할 RectTransform(target)과 Canvas를 자동 탐색한다.
/// FilePopup과 LoadingDragHandler 양쪽에서 사용.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    /// <summary>드래그할 때 이동시킬 대상. null이면 이 GameObject의 RectTransform 사용.</summary>
    [SerializeField] private RectTransform target;

    private RectTransform moveTarget;
    private Canvas        canvas;
    private Vector2       offset;

    void Awake()
    {
        moveTarget = target != null ? target : GetComponent<RectTransform>();
        canvas     = GetComponentInParent<Canvas>();
    }

    /// <summary>코드에서 AddComponent 후 즉시 이동 대상을 지정할 때 사용.</summary>
    public void Init(RectTransform dragTarget)
    {
        moveTarget = dragTarget;
        canvas     = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            e.position, e.pressEventCamera,
            out Vector2 local);

        offset = local - (Vector2)moveTarget.localPosition;
        moveTarget.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData e)
    {
        if (canvas == null || moveTarget == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            e.position, e.pressEventCamera,
            out Vector2 local))
        {
            moveTarget.localPosition = local - offset;
        }
    }
}
