using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 단순 팝업
/// Yes 클릭 시 이벤트 호출, No 클릭 시 닫기
/// </summary>
public class SelectPopup : MonoBehaviour
{
    [Header("버튼 연결")]
    public Button yesButton;
    public Button noButton;
    public Button closeButton;

    public event Action onClose;
    public event Action onYes; // Yes 클릭 이벤트

    void Start()
    {
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYes);

        if (noButton != null)
            noButton.onClick.AddListener(ClosePopup);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
    }

    private void OnYes()
    {
        onYes?.Invoke();
        ClosePopup();
    }

    private void ClosePopup()
    {
        onClose?.Invoke();
        Destroy(gameObject);
    }
}
