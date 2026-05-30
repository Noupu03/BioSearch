using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 단순 팝업 뷰. 버튼 이벤트만 외부로 노출하고 자기 자신을 소멸시킨다.
/// </summary>
public class SelectPopup : MonoBehaviour
{
    [Header("버튼")]
    public Button yesButton;
    public Button noButton;
    public Button closeButton;

    public event Action OnYesClicked;
    public event Action OnClosed;

    void Start()
    {
        if (yesButton)   yesButton.onClick.AddListener(HandleYes);
        if (noButton)    noButton.onClick.AddListener(HandleClose);
        if (closeButton) closeButton.onClick.AddListener(HandleClose);
    }

    private void HandleYes()
    {
        OnYesClicked?.Invoke();
        HandleClose();
    }

    private void HandleClose()
    {
        OnClosed?.Invoke();
        Destroy(gameObject);
    }
}
