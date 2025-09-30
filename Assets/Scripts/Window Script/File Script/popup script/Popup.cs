using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Popup : MonoBehaviour
{
    [Header("UI References")]
    public Button closeButton;          // ������ X ��ư
    public RectTransform topBar;        // �巡�׿� TopBar
    public TMP_Text topBarText;         // TopBar �ؽ�Ʈ

    private RectTransform popupRect;
    private Canvas parentCanvas;
    private Vector2 offset;

    /// <summary>
    /// �˾� ���� �� ���� ������ ����
    /// </summary>
    public void Initialize(string fileName, string fileExtension, Canvas canvas)
    {
        parentCanvas = canvas;
        popupRect = GetComponent<RectTransform>();

        // TopBar �ؽ�Ʈ ����
        if (topBarText != null)
            topBarText.text = $"{fileName}.{fileExtension}";

        // X ��ư Ŭ�� �̺�Ʈ
        if (closeButton != null)
            closeButton.onClick.AddListener(() => Destroy(gameObject));
    }

    #region �巡�� ����
    public void OnTopBarPointerDown(BaseEventData eventData)
    {
        PointerEventData pointerData = eventData as PointerEventData;
        if (popupRect == null) return;

        // Ŭ�� ��ġ�� �˾� ��ġ ���� ����
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupRect, pointerData.position, pointerData.pressEventCamera, out offset);

        // �巡�� ���� �� �ֻ������
        if (popupRect != null)
            popupRect.SetAsLastSibling();
    }

    public void OnTopBarDrag(BaseEventData eventData)
    {
        PointerEventData pointerData = eventData as PointerEventData;
        if (popupRect == null || parentCanvas == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, pointerData.position, pointerData.pressEventCamera, out localPoint))
        {
            popupRect.localPosition = localPoint - offset;
        }
    }
    #endregion
}
