using UnityEngine;
using UnityEngine.UI;

public class SelectPopupManager : MonoBehaviour
{
    [Header("��ư ����")]
    public Button acceptButton;
    public Button releaseButton;

    [Header("�˾� ������")]
    public GameObject acceptPopupPrefab;
    public GameObject releasePopupPrefab;

    [Header("�˾� �θ�")]
    public Transform popupParent;

    private GameObject currentPopup;

    // ��ƼƼ/�α� ����
    public SanityManager sanityManager;
    public LogWindowManager logWindow;
    public FileWindow fileWindow;
    TimerManager timerManager;


    void Start()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(() => ShowPopup(acceptPopupPrefab, true));

        if (releaseButton != null)
            releaseButton.onClick.AddListener(() => ShowPopup(releasePopupPrefab, false));
    }


    //�ð� �ʰ��� ���ӿ����� �� �ʱ�ȭ


	private void ShowPopup(GameObject popupPrefab, bool isAccept)
    {
        if (currentPopup != null) return;
        if (popupPrefab == null || popupParent == null) return;

        currentPopup = Instantiate(popupPrefab, popupParent);

        // PopupPanel �� X ��ư ã��
        Button xButton = currentPopup.transform.Find("PopupPanel/XButton")?.GetComponent<Button>();

        // contentPanel �� Yes/No ��ư ã��
        Transform content = currentPopup.transform.Find("PopupPanel/contentPanel");
        if (content != null)
        {
            Button yesButton = content.Find("Yes")?.GetComponent<Button>();
            Button noButton = content.Find("No")?.GetComponent<Button>();

            SelectPopup popupComp = currentPopup.GetComponent<SelectPopup>();
            if (popupComp != null)
            {
                popupComp.yesButton = yesButton;
                popupComp.noButton = noButton;
                popupComp.closeButton = xButton;

                // Yes Ŭ�� �� ó��
                popupComp.onYes += () =>
                {
                    HandleYes(isAccept);
                    ClosePopup();
                };

                // No Ŭ���� �׳� �ݱ�
            }
        }

        if (xButton != null)
            xButton.onClick.AddListener(ClosePopup);
    }

    private void HandleYes(bool isAccept)
    {
        Folder root = fileWindow.GetRootFolder();
        if (root == null) return;

        int abnormalCount = AbnormalDetector.GetAbnormalCount(root);

        bool success = (isAccept && abnormalCount == 0) || (!isAccept && abnormalCount > 0);

        if (success)
        {
            logWindow.Log("����!");
            ScoreCount.successCount++;
        }
        else
        {
            logWindow.Log("����!");
            ScoreCount.failCount++;

            if (sanityManager != null)
            {
                sanityManager.DecreaseSanity(40f);

                //  ���ӿ��� Ȯ��
                if (sanityManager.GetCurrentSanity() <= 0f)
                {
                    // ���ӿ��� ó��
                    GameOverManager gameOver = FindObjectOfType<GameOverManager>();
                    if (gameOver != null)
                    {
                        gameOver.TriggerGameOver("���ŷ� 0���� ���� ���ӿ���");
                    }

                    // �� Reload�� stage ���� �ߴ�
                    return;
                }
            }
        }

        // ���ӿ����� �ƴϸ� �������� ����
        ScoreCount.stageCount++;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }


    private void ClosePopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }
    }

    
}
