using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로그 창에 입력된 'scan 명령'을 처리하는 매니저.
/// 폴더를 탐색해 이상 파일/폴더를 찾아 진행바로 표시한다.
/// </summary>
public class ScanCommandManager : MonoBehaviour
{
    public static ScanCommandManager Instance { get; private set; }

    [Header("참조")]
    public FileWindow       fileWindow;
    public LogWindowManager logWindow;
    public GameObject       scanPopupPrefab;
    public Canvas           parentCanvas;

    private bool        isScanning;
    private Slider      scanProgressSlider;
    private GameObject  scanPopupInstance;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (logWindow != null)
            logWindow.OnScanCommandEntered += HandleScanCommand;
    }

    void OnDisable()
    {
        if (logWindow != null)
            logWindow.OnScanCommandEntered -= HandleScanCommand;
    }

    private void HandleScanCommand(string folderName)
    {
        if (isScanning) { logWindow.Log("스캔 중입니다..."); return; }

        Folder root   = fileWindow.GetRootFolder();
        Folder target = FindFolderByName(root, folderName);

        if (target == null) { logWindow.Log("해당 폴더를 찾을 수 없습니다."); return; }

        StartCoroutine(ScanFolderCoroutine(target));
    }

    private IEnumerator ScanFolderCoroutine(Folder folder)
    {
        isScanning = true;
        logWindow.DisableInput();

        scanPopupInstance  = Instantiate(scanPopupPrefab, parentCanvas.transform);
        scanProgressSlider = scanPopupInstance.GetComponentInChildren<Slider>();
        scanProgressSlider.value = 0f;

        int   totalItems    = CountItems(folder);
        float totalScanTime = totalItems * 3f;
        int   abnormalCount = AbnormalDetector.GetAbnormalCount(folder);

        logWindow.Log("이상 스캔 중...");

        // 버퍼 구간 생성 (진행이 잠시 멈추는 구간)
        int     bufferCount     = Random.Range(4, 6);
        float[] bufferTimes     = new float[bufferCount];
        float[] bufferPositions = new float[bufferCount];
        float   totalBufferTime = 0f;

        for (int i = 0; i < bufferCount; i++)
        {
            bufferTimes[i]      = totalScanTime * Random.Range(0.1f, 0.15f);
            bufferPositions[i]  = Random.Range(0f, 1f);
            totalBufferTime    += bufferTimes[i];
        }
        System.Array.Sort(bufferPositions);

        float progressTime = Mathf.Max(0f, totalScanTime - totalBufferTime);
        float elapsed      = 0f;
        int   bufferIndex  = 0;

        while (elapsed < progressTime)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / progressTime);

            while (bufferIndex < bufferCount && progress >= bufferPositions[bufferIndex])
            {
                yield return new WaitForSeconds(bufferTimes[bufferIndex]);
                bufferIndex++;
            }

            scanProgressSlider.value = progress;
            yield return null;
        }

        scanProgressSlider.value = 1f;
        logWindow.ReplaceLastScanLog("스캔 완료");

        Destroy(scanPopupInstance);
        logWindow.EnableInput();
        isScanning = false;

        logWindow.Log($"이상 감지 수: {abnormalCount}");
    }

    // ── 유틸리티 ─────────────────────────────────

    private static Folder FindFolderByName(Folder folder, string name)
    {
        if (folder.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            return folder;

        foreach (var child in folder.children)
        {
            var found = FindFolderByName(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private static int CountItems(Folder folder)
    {
        int count = 1 + folder.files.Count;
        foreach (var child in folder.children)
            count += CountItems(child);
        return count;
    }
}
