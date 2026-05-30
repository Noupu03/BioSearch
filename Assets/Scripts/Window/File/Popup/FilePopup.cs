using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 개별 팝업 동작을 제어하는 클래스
/// - 드래그 이동
/// - 닫기 버튼 처리
/// - 파괴 시 PopupManager와 동기화
/// </summary>
public class FilePopup : MonoBehaviour
{
    [Header("UI References")]
    public Button closeButton;          // 프리팹 X 버튼
    public RectTransform topBar;        // 드래그용 TopBar
    public TMP_Text topBarText;         // TopBar 텍스트

    private RectTransform popupRect;
    private Canvas parentCanvas;
    private Vector2 offset;

    // PopupManager에서 관리할 키 (파일 이름)
    private string fileKey;

    /// <summary>
    /// 팝업 생성 시 파일 정보를 세팅
    /// </summary>
    public void Initialize(string fileName, string fileExtension, Canvas canvas)
    {
        parentCanvas = canvas;
        popupRect = GetComponent<RectTransform>();

        // TopBar 텍스트 설정
        if (topBarText != null)
            topBarText.text = $"{fileName}.{fileExtension}";

        // X 버튼 클릭 이벤트
        if (closeButton != null)
            closeButton.onClick.AddListener(() => Destroy(gameObject));
    }

    /// <summary>
    /// PopupManager에서 관리용 키 설정
    /// </summary>
    public void SetFileKey(string key)
    {
        fileKey = key;
    }

    private void OnDestroy()
    {
        // 파괴 시 PopupManager에 알림
        if (FilePopupManager.Instance != null && !string.IsNullOrEmpty(fileKey))
            FilePopupManager.Instance.OnPopupDestroyed(fileKey);
    }

    #region 드래그 구현
    public void OnTopBarPointerDown(BaseEventData eventData)
    {
        PointerEventData pointerData = eventData as PointerEventData;
        if (popupRect == null) return;

        // 클릭 위치와 팝업 위치 차이 저장
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            popupRect, pointerData.position, pointerData.pressEventCamera, out offset);

        // 드래그 시작 시 최상단으로
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
