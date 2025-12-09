using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class FaxScheduleEntry
{
    public GameDateTime time;      // 언제 도착하는지
    [TextArea(3, 5)]
    public string message;         // 이때 보여줄 팩스 내용
}

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

    [Header("씬에 있는 UI 버튼 (선택)")]
    public Button faxUIButton;   // paxClickArea 안의 Button 넣어두면 됨

    [Header("시간별 팩스 스케줄")]
    public List<FaxScheduleEntry> faxSchedule = new List<FaxScheduleEntry>();



    // 어떤 스케줄이 이미 생성됐는지 체크용
    private HashSet<int> spawnedIndexes = new HashSet<int>();

    void Update()
    {
        if (timeManager == null || faxPrefab == null) return;

        GameDateTime now = timeManager.GetCurrentGameTime();

        for (int i = 0; i < faxSchedule.Count; i++)
        {
            if (spawnedIndexes.Contains(i))
                continue;

            FaxScheduleEntry entry = faxSchedule[i];

            if (now >= entry.time)
            {
                SpawnFax(entry);
                spawnedIndexes.Add(i);
            }
        }
    }

    //  여기서 실제 팩스 오브젝트 생성 + 메시지 주입 + 버튼 연결
    void SpawnFax(FaxScheduleEntry entry)
    {
        GameObject fax = Instantiate(faxPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"[FAX] 팩스 생성됨! 메시지: {entry.message}");

        // 1) FaxViewer 기본 세팅
        FaxViewer viewer = fax.GetComponent<FaxViewer>();
        if (viewer != null)
        {
            viewer.viewCamera = roomCamera;
        }

        // 2) 삭제 버튼 세팅 (있으면)
        FaxDeleteButton deleteBtn = fax.GetComponentInChildren<FaxDeleteButton>(true);
        if (deleteBtn != null)
        {
            deleteBtn.targetCamera = roomCamera;
            deleteBtn.faxObject = fax;
        }

        // 3)  메시지 UI에 꽂기
        FaxMessageDisplay msgDisplay = fax.GetComponentInChildren<FaxMessageDisplay>(true);
        if (msgDisplay != null)
        {
            msgDisplay.SetMessage(entry.message);
        }
        else
        {
            Debug.LogWarning("[FAX] FaxMessageDisplay 컴포넌트를 찾을 수 없습니다.");
        }

        // 4) (옵션) 씬 UI 버튼을 이 팩스에 연결
        if (faxUIButton != null && viewer != null)
        {
            faxUIButton.onClick.RemoveAllListeners();
            faxUIButton.onClick.AddListener(viewer.TriggerExpand);
        }
        Canvas canvas = fax.GetComponentInChildren<Canvas>(true);
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = roomCamera;

    }
}
