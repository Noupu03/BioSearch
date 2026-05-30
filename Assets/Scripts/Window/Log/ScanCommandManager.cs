using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로그 창에 입력된 'scan 명령'을 처리한다.
/// FileWindow와 LogWindowManager는 Instance로 접근하므로
/// 인스펙터 크로스 참조가 없다.
/// </summary>
public class ScanCommandManager : MonoBehaviour
{
    public static ScanCommandManager Instance { get; private set; }

    [Header("팝업 프리팹")]
    public GameObject scanPopupPrefab;

    private bool   isScanning;
    private Slider scanProgressSlider;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void OnEnable()
    {
        if (LogWindowManager.Instance != null)
            LogWindowManager.Instance.OnScanCommandEntered += HandleScanCommand;
    }

    void OnDisable()
    {
        if (LogWindowManager.Instance != null)
            LogWindowManager.Instance.OnScanCommandEntered -= HandleScanCommand;
    }

    private void HandleScanCommand(string folderName)
    {
        var log = LogWindowManager.Instance;
        if (isScanning) { log?.Log("스캔 중입니다..."); return; }

        var fw = FileWindow.Instance;
        if (fw == null) return;

        Folder target = FindFolderByName(fw.GetRootFolder(), folderName);
        if (target == null) { log?.Log("해당 폴더를 찾을 수 없습니다."); return; }

        StartCoroutine(ScanFolderCoroutine(target));
    }

    private IEnumerator ScanFolderCoroutine(Folder folder)
    {
        var log    = LogWindowManager.Instance;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null || scanPopupPrefab == null) yield break;

        isScanning = true;
        log?.DisableInput();

        var popupGo = Instantiate(scanPopupPrefab, canvas.transform);
        scanProgressSlider = popupGo.GetComponentInChildren<Slider>();
        if (scanProgressSlider) scanProgressSlider.value = 0f;

        int   totalItems    = CountItems(folder);
        float totalScanTime = totalItems * 3f;
        int   abnormalCount = AbnormalDetector.GetAbnormalCount(folder);

        log?.Log("이상 스캔 중...");

        int     bufCount   = Random.Range(4, 6);
        float[] bufTimes   = new float[bufCount];
        float[] bufPos     = new float[bufCount];
        float   bufTotal   = 0f;

        for (int i = 0; i < bufCount; i++)
        {
            bufTimes[i]  = totalScanTime * Random.Range(0.1f, 0.15f);
            bufPos[i]    = Random.Range(0f, 1f);
            bufTotal    += bufTimes[i];
        }
        System.Array.Sort(bufPos);

        float progressTime = Mathf.Max(0f, totalScanTime - bufTotal);
        float elapsed      = 0f;
        int   bufIdx       = 0;

        while (elapsed < progressTime)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / progressTime);

            while (bufIdx < bufCount && progress >= bufPos[bufIdx])
            {
                yield return new WaitForSeconds(bufTimes[bufIdx]);
                bufIdx++;
            }

            if (scanProgressSlider) scanProgressSlider.value = progress;
            yield return null;
        }

        if (scanProgressSlider) scanProgressSlider.value = 1f;
        log?.ReplaceLastScanLog("스캔 완료");

        Destroy(popupGo);
        log?.EnableInput();
        isScanning = false;
        log?.Log($"이상 감지 수: {abnormalCount}");
    }

    private static Folder FindFolderByName(Folder folder, string name)
    {
        if (folder.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return folder;
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
        foreach (var child in folder.children) count += CountItems(child);
        return count;
    }
}
