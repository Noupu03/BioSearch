using UnityEngine;
using UnityEngine.UI;

public class SelectPopupManager : MonoBehaviour
{
    [Header("버튼 연결")]
    public Button acceptButton;
    public Button releaseButton;

    [Header("팝업 프리팹")]
    public GameObject acceptPopupPrefab;
    public GameObject releasePopupPrefab;

    [Header("팝업 부모")]
    public Transform popupParent;

    private GameObject currentPopup;

    // 샌티티/로그 연동
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


    //시간 초과로 게임오버시 값 초기화


	private void ShowPopup(GameObject popupPrefab, bool isAccept)
    {
        if (currentPopup != null) return;
        if (popupPrefab == null || popupParent == null) return;

        currentPopup = Instantiate(popupPrefab, popupParent);

        // PopupPanel 안 X 버튼 찾기
        Button xButton = currentPopup.transform.Find("PopupPanel/XButton")?.GetComponent<Button>();

        // contentPanel 안 Yes/No 버튼 찾기
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

                // Yes 클릭 시 처리
                popupComp.onYes += () =>
                {
                    HandleYes(isAccept);
                    ClosePopup();
                };

                // No 클릭은 그냥 닫기
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

                //  게임오버 확인
                if (sanityManager.GetCurrentSanity() <= 0f)
                {
                    // 게임오버 처리
                    GameOverManager gameOver = FindObjectOfType<GameOverManager>();
                    if (gameOver != null)
                    {
                        gameOver.TriggerGameOver("정신력 0으로 인한 게임오버");
                    }

                    // 씬 Reload나 stage 증가 중단
                    return;
                }
            }
        }

        // 게임오버가 아니면 스테이지 진행
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
