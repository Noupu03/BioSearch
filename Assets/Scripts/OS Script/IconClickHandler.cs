using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class IconClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Action onDoubleClick;
    private float lastClickTime;
    private float doubleClickThreshold = 0.3f; // 0.3초 이내 두 번 클릭하면 더블클릭

    public void Initialize(Action doubleClickAction)
    {
        onDoubleClick = doubleClickAction;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            onDoubleClick?.Invoke();
        }
        lastClickTime = Time.time;
    }
}
