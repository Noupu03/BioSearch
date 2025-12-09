using UnityEngine;
using TMPro;

public class ScoreDisplayOnFax : MonoBehaviour
{
    [Header("3D TextMeshPro (World Space TMP)")]
    [SerializeField] private TextMeshPro prevDayScoreTMP;   // 전날 점수 TMP
    [SerializeField] private TextMeshPro currentDateTMP;    // 현재 날짜 TMP


    // 점수 가져올 대상
    public SubmissionChecker submissionChecker;

    // 표시할 날짜
    private GameDateTime targetDate;

    private void Awake()
    {
        // submissionChecker 자동 연결
        if (submissionChecker == null)
        {
            submissionChecker = GetComponent<SubmissionChecker>();

            if (submissionChecker == null)
                submissionChecker = GetComponentInParent<SubmissionChecker>();

            if (submissionChecker == null)
                submissionChecker = FindAnyObjectByType<SubmissionChecker>();
        }
    }

    // 외부에서 날짜 설정
    public void SetDate(GameDateTime date)
    {
        targetDate = date;
        UpdateDisplay();
    }


    // --------------------------------------------------
    // UI 업데이트
    // --------------------------------------------------
    private void UpdateDisplay()
    {
        if (submissionChecker == null)
        {
            Debug.LogError("SubmissionChecker 없음!");
            return;
        }
        // 전날 계산
        GameDateTime prev = GetPreviousDate(targetDate);
        // 현재 날짜 출력
        if (currentDateTMP != null)
        {
            currentDateTMP.text = string.Format(
                "{0}년 {1}월 {2}일",
                prev.year, prev.month, prev.day
            );
        }

        

        // 전날의 인덱스 계산
        int idx = submissionChecker.GetDateIndex(prev);

        // dailyScores 범위 체크
        if (submissionChecker.dailyScores == null ||
            idx < 0 ||
            idx >= submissionChecker.dailyScores.Length)
        {
            if (prevDayScoreTMP != null)
                prevDayScoreTMP.text = "미제출";
            return;
        }

        ScoreData data = submissionChecker.dailyScores[idx];

        // ScoreData 구조체라 null될 일은 없지만 초기 default일 수도 있음
        bool noData =
            data.fileMust == 0 &&
            data.fileCorrect == 0 &&
            data.partErrorCount == 0 &&
            data.partCorrect == 0;

        if (noData)
        {
            if (prevDayScoreTMP != null)
                prevDayScoreTMP.text = "미제출";
        }
        else
        {
            if (prevDayScoreTMP != null)
            {
                prevDayScoreTMP.text = string.Format(
                    "파일: {0}/{1}/{2}\n부품: {3}/{4}/{5}\n총점: {6}",
                    data.fileCorrect,
                    data.fileMust,
                    data.fileWrong,

                    data.partCorrect,
                    data.partErrorCount,
                    data.partWrong,

                    data.grade
                );
            }

        }
    }


    // --------------------------------------------------
    // 날짜 -1일 계산 (hour/minute 무시)
    // --------------------------------------------------
    private GameDateTime GetPreviousDate(GameDateTime dt)
    {
        int y = dt.year;
        int m = dt.month;
        int d = dt.day - 1;

        if (d <= 0)
        {
            m -= 1;
            if (m <= 0)
            {
                m = 12;
                y -= 1;
            }
            d = 31; // 게임 로직상 31일 고정
        }

        return new GameDateTime(y, m, d, 0, 0);
    }
}
