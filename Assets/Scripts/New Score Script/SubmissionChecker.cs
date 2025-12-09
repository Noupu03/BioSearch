using UnityEngine;
public class SubmissionChecker : MonoBehaviour
{
    [Header("BodyPartsData 참조")]
    public BodyPartsData bodyPartsData;

    [Header("날짜별 점수 기록")]
    public ScoreData[] dailyScores;
    public OSTimeManager timeManager;

    private const int baseYear = 0;
    private const int daysPerMonth = 31;
    private const int monthsPerYear = 12;

    void Awake()
    {
        // 안전하게 최소 배열 길이 확보
        if (dailyScores == null || dailyScores.Length < 4000)
            dailyScores = new ScoreData[4000];
    }

    // 기준일 00/01/01 기준으로 배열 인덱스 계산
    public int GetDateIndex(GameDateTime dt)
    {
        int yearDiff = dt.year - baseYear;
        int monthDiff = dt.month - 1; // 1월 = 0
        int dayDiff = dt.day - 1;     // 1일 = 0

        int index = yearDiff * monthsPerYear * daysPerMonth + monthDiff * daysPerMonth + dayDiff;

        if (index < 0) index = 0;

        return index;
    }

    // ------------------------------
    // 파일 체크 개수 계산
    // ------------------------------
    public (int must, int correct) GetFileStats()
    {
        int mustSubmit = 0;
        int correctChecked = 0;

        if (FileWindow.Instance == null)
            return (0, 0);

        foreach (var file in FileWindow.Instance.allFiles)
        {
            if (file.isImportant)
            {
                mustSubmit++;
                if (file.isChecked)
                    correctChecked++;
            }
        }

        return (mustSubmit, correctChecked);
    }

    // ------------------------------
    // 부품 체크 개수 계산
    // ------------------------------
    public (int errorCount, int correctChecked) GetPartStats()
    {
        int errorCount = 0;
        int correctChecked = 0;

        if (bodyPartsData == null)
            return (0, 0);

        foreach (var part in bodyPartsData.GetAllParts())
        {
            if (part == null) continue;

            if (part.isError)
            {
                errorCount++;
                if (part.isChecked)
                    correctChecked++;
            }
        }

        return (errorCount, correctChecked);
    }

    // ------------------------------
    // 최종 날짜별 점수 저장
    // ------------------------------
    // ------------------------------
    // 최종 날짜별 점수 저장
    // ------------------------------
    public void SaveTodayScore()
    {
        if (timeManager == null)
        {
            Debug.LogError("OSTimeManager 미설정!");
            return;
        }

        GameDateTime now = timeManager.GetCurrentGameTime();
        int idx = GetDateIndex(now);

        // 배열 부족 시 자동 확장
        if (idx >= dailyScores.Length)
        {
            System.Array.Resize(ref dailyScores, idx + 1);
        }

        // 파일 통계
        var fileStat = GetFileStats();
        int fileMust = fileStat.must;
        int fileCorrect = fileStat.correct;
        int fileWrong = fileMust - fileCorrect;

        // 부품 통계
        var partStat = GetPartStats();
        int partError = partStat.errorCount;
        int partCorrect = partStat.correctChecked;
        int partWrong = partError - partCorrect;

        // ★ 등급 계산
        char grade = CalculateGrade(fileMust, fileCorrect, fileWrong);

        // 저장
        dailyScores[idx] = new ScoreData
        {
            fileMust = fileMust,
            fileCorrect = fileCorrect,
            fileWrong = fileWrong,

            partErrorCount = partError,
            partCorrect = partCorrect,
            partWrong = partWrong,

            grade = grade // ★ 등급 저장
        };

        Debug.Log($"[{idx}] 점수 저장 완료: 파일({fileCorrect}/{fileMust}/{fileWrong}), 부품({partCorrect}/{partError}/{partWrong}), 등급: {grade}");
    }

    private char CalculateGrade(int must, int correct, int wrong)
    {
        if (must > 0)
        {
            float ratio = (float)correct / must;

            // A 등급
            if (ratio == 1f && wrong == 0)
                return 'A';

            // B 등급
            if (ratio >= 0.5f && wrong >= 1 && wrong <= 3)
                return 'B';
        }

        // 그 외 모두 C
        return 'C';
    }
}
