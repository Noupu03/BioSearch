using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// ���� / ���� �˾�â ���� ��ũ��Ʈ
/// Yes / No / X ��ư ��� �ݱ� ����
/// </summary>
public class SelectPopup : MonoBehaviour
{
    [Header("UI ����")]
    public Button yesButton;      // YES ��ư
    public Button noButton;       // NO ��ư
    public Button closeButton;    // X ��ư

    // �˾��� ���� �� ȣ��Ǵ� �̺�Ʈ (PopupManager�� �̰� ����)
    public event Action onClose;

    void Start()
    {
        // ��ư �̺�Ʈ ���
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYes);

        if (noButton != null)
            noButton.onClick.AddListener(OnNo);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
    }

    /// <summary>
    /// YES ��ư Ŭ�� �� ����
    /// </summary>
    private void OnYes()
    {
        Debug.Log("����ڰ� YES�� �����߽��ϴ�.");
        ClosePopup();
    }

    /// <summary>
    /// NO ��ư Ŭ�� �� ����
    /// </summary>
    private void OnNo()
    {
        Debug.Log("����ڰ� NO�� �����߽��ϴ�.");
        ClosePopup();
    }

    /// <summary>
    /// �˾� �ݱ�
    /// </summary>
    private void ClosePopup()
    {
        onClose?.Invoke();
        Destroy(gameObject);
    }
}
