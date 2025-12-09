using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FaxScheduleManager : MonoBehaviour
{
    [Header("게임 시간 관리자")]
    public OSTimeManager timeManager;

    [Header("팩스 프리팹")]
    public GameObject faxPrefab;

    [Header("팩스를 보여줄 카메라")]
    public Camera roomCamera;

    [Header("팩스 생성 위치")]
    public Vector3 spawnPosition = Vector3.zero;

    [Header("UI 버튼 (씬에 존재하는 버튼)")]
    public Button faxUIButton;

    public List<GameDateTime> faxSchedule = new List<GameDateTime>();
    private HashSet<string> spawnedFax = new HashSet<string>();

    void Update()
    {
        if (timeManager == null) return;

        GameDateTime now = timeManager.GetCurrentGameTime();

        foreach (var targetTime in faxSchedule)
        {
            string key = targetTime.ToString();

            if (!spawnedFax.Contains(key) && now >= targetTime)
            {
                SpawnFax();
                spawnedFax.Add(key);
            }
        }
    }

    void SpawnFax()
    {
        //  팩스 생성
        GameObject fax = Instantiate(faxPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"[FAX] {fax.name} 생성됨!");
        FaxViewer viewer = fax.GetComponent<FaxViewer>();

        // ------------------------------
        // ScoreDisplayOnFax 연결
        // ------------------------------
        ScoreDisplayOnFax scoreUI = fax.GetComponent<ScoreDisplayOnFax>();
        if (scoreUI != null)
        {
            // SubmissionChecker는 여기서 찾아 연결
            SubmissionChecker checker = FindObjectOfType<SubmissionChecker>();
            scoreUI.submissionChecker = checker;

            // 스케줄된 날짜의 "전날 점수" 가 필요하므로
            // SpawnFax 호출한 targetTime을 전달
            if (faxSchedule.Count > 0)
            {
                GameDateTime target = timeManager.GetCurrentGameTime();
                scoreUI.SetDate(target);
            }
        }

        if (viewer != null)
        {
            viewer.viewCamera = roomCamera;
            Debug.Log("[FAX] Viewer 카메라 연결 완료");
        }
        else
        {
            Debug.LogError("[FAX ERROR] FaxViewer 컴포넌트를 찾지 못함!");
        }

        // 3) 삭제 버튼 처리 (world click delete)
        FaxDeleteButton deleteBtn = fax.GetComponentInChildren<FaxDeleteButton>(true);
        if (deleteBtn != null)
        {
            deleteBtn.targetCamera = roomCamera;
            deleteBtn.faxObject = fax;
            Debug.Log("[FAX] 삭제 버튼 세팅 완료");
        }

        //  UI 버튼 자동 연결 (씬 버튼)
        if (faxUIButton != null)
        {
            faxUIButton.onClick.RemoveAllListeners();
            faxUIButton.onClick.AddListener(() => viewer.TriggerExpand());

            Debug.Log($"[FAX UI] '{faxUIButton.name}' 버튼 → 새 FaxViewer에 자동 연결됨");
        }
        else
        {
            Debug.LogWarning("[FAX WARNING] UI 버튼이 배정되지 않음 → Inspector에 지정 필요");
        }
    }

}
