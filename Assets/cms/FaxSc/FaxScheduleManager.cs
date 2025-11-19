using UnityEngine;
using System.Collections.Generic;

public class FaxScheduleManager : MonoBehaviour
{
    [Header("게임 시간 관리자")]
    public OSTimeManager timeManager;

    [Header("팩스 프리팹")]
    public GameObject faxPrefab;

    [Header("팩스를 보여줄 카메라")]
    public Camera roomCamera;   // 여기다가 Room Camera 넣기

    [Header("팩스 생성 위치")]
    public Vector3 spawnPosition = Vector3.zero;

    public List<GameDateTime> faxSchedule = new List<GameDateTime>();
    private HashSet<string> spawnedFax = new HashSet<string>();

    void Update()
    {
        GameDateTime now = timeManager.GetCurrentGameTime();

        foreach (var targetTime in faxSchedule)
        {
            string key = targetTime.ToString();
            if (spawnedFax.Contains(key))
                continue;

            if (now >= targetTime)
            {
                SpawnFax();
                spawnedFax.Add(key);
            }
        }
    }

    void SpawnFax()
    {
        GameObject fax = Instantiate(faxPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("[FAX] 특정 시간에 팩스가 생성되었습니다!");

       
        FaxViewer viewer = fax.GetComponent<FaxViewer>();
        if (viewer != null)
        {
            viewer.viewCamera = roomCamera;          //  카메라 주입
        }

        FaxDeleteButton deleteBtn = fax.GetComponentInChildren<FaxDeleteButton>();
        if (deleteBtn != null)
        {
            deleteBtn.targetCamera = roomCamera;     //  버튼용 카메라도 주입
            deleteBtn.faxObject = fax;
        }
     
    }
}
