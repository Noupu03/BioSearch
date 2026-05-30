using UnityEngine;
using UnityEngine.UI;

public class SelectPopupManager : MonoBehaviour
{
    [Header("버튼 참조")]
    public Button acceptButton;
    public Button releaseButton;

    [Header("팝업 프리팹")]
    public GameObject acceptPopupPrefab;
    public GameObject releasePopupPrefab;

    [Header("팝업 부모")]
    public Transform popupParent;

    private GameObject currentPopup;

    [Header("매니저 참조")]
    public SanityManager sanityManager;
    public LogWindowManager logWindow;
    public FileWindow fileWindow;
    [SerializeField] private GameOverManager gameOverManager;

    void Start()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(() => ShowPopup(acceptPopupPrefab, true));

        if (releaseButton != null)
            releaseButton.onClick.AddListener(() => ShowPopup(releasePopupPrefab, false));
    }

    private void ShowPopup(GameObject popupPrefab, bool isAccept)
    {
        if (currentPopup != null) return;
        if (popupPrefab == null || popupParent == null) return;

        currentPopup = Instantiate(popupPrefab, popupParent);

        Button xButton = currentPopup.transform.Find("PopupPanel/XButton")?.GetComponent<Button>();

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

                popupComp.onYes += () =>
                {
                    HandleYes(isAccept);
                    ClosePopup();
                };
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
            logWindow.Log("성공!");
            ScoreCount.successCount++;
        }
        else
        {
            logWindow.Log("실패!");
            ScoreCount.failCount++;

            if (sanityManager != null)
            {
                sanityManager.DecreaseSanity(40f);
                if (sanityManager.GetCurrentSanity() <= 0f)
                    return; // SanityManager가 이미 GameOver 처리
            }
        }

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
