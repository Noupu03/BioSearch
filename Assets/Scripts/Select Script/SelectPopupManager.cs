using UnityEngine;
using UnityEngine.UI;

public class SelectPopupManager : MonoBehaviour
{
    [Header("��ư ����")]
    public Button acceptButton;   // ���� ��ư
    public Button releaseButton;  // ���� ��ư

    [Header("�˾� ������")]
    public GameObject acceptPopupPrefab;   // "�����Ͻðڽ��ϱ�?" �˾� ������
    public GameObject releasePopupPrefab;  // "�����Ͻðڽ��ϱ�?" �˾� ������

    [Header("�θ� ������Ʈ")]
    public Transform popupParent; // �˾��� ��� �θ� (��: Canvas)

    private GameObject currentPopup; // ���� ���ִ� �˾� (1���� ����)

    private void Start()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
        if (releaseButton != null)
            releaseButton.onClick.AddListener(OnReleaseButtonClicked);
    }

    // ���� ��ư Ŭ�� ��
    private void OnAcceptButtonClicked()
    {
        ShowPopup(acceptPopupPrefab);
    }

    // ���� ��ư Ŭ�� ��
    private void OnReleaseButtonClicked()
    {
        ShowPopup(releasePopupPrefab);
    }

    // �˾� ���� (�ߺ� ����)
    private void ShowPopup(GameObject popupPrefab)
    {
        if (currentPopup != null)
            return; // �̹� �� ������ ����

        if (popupPrefab != null && popupParent != null)
        {
            currentPopup = Instantiate(popupPrefab, popupParent);

            // X ��ư ã��
            Button xButton = currentPopup.transform.Find("XButton")?.GetComponent<Button>();
            if (xButton != null)
                xButton.onClick.AddListener(ClosePopup);

            // Yes / No ��ư ã��
            Button yesButton = currentPopup.transform.Find("YesButton")?.GetComponent<Button>();
            Button noButton = currentPopup.transform.Find("NoButton")?.GetComponent<Button>();

            if (yesButton != null)
                yesButton.onClick.AddListener(ClosePopup);
            if (noButton != null)
                noButton.onClick.AddListener(ClosePopup);
        }
    }

    // �˾� �ݱ�
    private void ClosePopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }
    }
}
