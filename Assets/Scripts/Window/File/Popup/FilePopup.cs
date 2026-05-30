using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 파일 팝업 창. 드래그 이동은 topBar에 붙은 DragHandler 컴포넌트가 담당한다.
/// </summary>
public class FilePopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button        closeButton;
    [SerializeField] public  RectTransform topBar;      // FilePopupManager에서 DragHandler 추가에 사용
    [SerializeField] private TMP_Text      topBarText;

    private string fileKey;

    public void Initialize(string fileName, string fileExtension, Canvas canvas)
    {
        if (topBarText != null)
            topBarText.text = $"{fileName}.{fileExtension}";

        if (closeButton != null)
            closeButton.onClick.AddListener(() => Destroy(gameObject));

        // topBar에 DragHandler가 없으면 자동 추가 (팝업 자체를 target으로)
        if (topBar != null && topBar.GetComponent<DragHandler>() == null)
        {
            var dh = topBar.gameObject.AddComponent<DragHandler>();
            // target 미지정 시 팝업 RectTransform 자체로 이동
        }
    }

    public void SetFileKey(string key) => fileKey = key;

    // FilePopupManager에서 EventTrigger를 연결할 수 있도록 접근자 제공
    // (FilePopupManager가 SetupDragTrigger에서 사용 - 이제는 DragHandler가 처리하므로 경량화)
    public void OnTopBarPointerDown(BaseEventData _) { }
    public void OnTopBarDrag(BaseEventData _)        { }

    void OnDestroy()
    {
        if (FilePopupManager.Instance != null && !string.IsNullOrEmpty(fileKey))
            FilePopupManager.Instance.OnPopupDestroyed(fileKey);
    }
}
