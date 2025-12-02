using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SubmitWindow : MonoBehaviour
{
    public Button submitButton;
    public TMP_Text statusText;

    private MessageScheduler scheduler;
    private OSTimeManager timeManager;

    private static GameDateTime? lastSubmitTime = null;

    public SubmitProgress progress;
    private SubmissionChecker submissionChecker;

    void Awake()
    {
        scheduler = FindObjectOfType<MessageScheduler>();
        timeManager = FindObjectOfType<OSTimeManager>();
        submissionChecker = FindObjectOfType<SubmissionChecker>(); // ★ 반드시 추가
    }

    void Start()
    {
        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitClicked);
    }

    private void OnSubmitClicked()
    {
        GameDateTime now = timeManager.GetCurrentGameTime();

        // ==== 하루 1회 제한 ====
        if (lastSubmitTime.HasValue)
        {
            GameDateTime last = lastSubmitTime.Value;

            if (last.year == now.year &&
                last.month == now.month &&
                last.day == now.day)
            {
                if (statusText != null)
                    statusText.text = "오늘은 이미 제출했습니다.";
                return;
            }
        }

        // ProgressBar 시작
        if (progress != null)
            progress.StartProgressBar();

        // ★ 20초 후 실제 제출 처리
        StartCoroutine(SubmitAfterDelay());
    }

    private IEnumerator SubmitAfterDelay()
    {
        yield return new WaitForSeconds(20f);

        GameDateTime now = timeManager.GetCurrentGameTime();

        // 제출 시간 기록 → 하루 1회 제한 활성화
        lastSubmitTime = now;
        submissionChecker.SaveTodayScore();

        // 완료 표시
        if (statusText != null)
            statusText.text = "전송 완료!";
    }
}
