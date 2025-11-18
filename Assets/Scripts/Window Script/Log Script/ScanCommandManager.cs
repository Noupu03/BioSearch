using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Slider 사용

/// <summary>
/// ScanCommandManager
/// - 로그 창에서 입력된 '스캔 명령'을 감지하고 실행하는 매니저
/// - 폴더 구조를 순회하며 이상 파일/폴더를 탐색하고 진행률을 표시함
/// </summary>
public class ScanCommandManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static ScanCommandManager Instance;

    // 폴더 구조와 UI를 관리하는 FileWindow
    public FileWindow fileWindow;

    // 로그 및 명령어 입력을 처리하는 LogWindowManager
    public LogWindowManager logWindow;

    // 스캔 팝업 프리팹 (슬라이더 포함된 UI)
    public GameObject scanPopupPrefab;

    // 팝업을 붙일 Canvas
    public Canvas parentCanvas;

    // 스캔 중인지 여부를 나타내는 플래그
    private bool isScanning = false;

    // 현재 생성된 스캔 팝업 인스턴스
    private GameObject scanPopupInstance;

    // 생성된 팝업 내부 슬라이더
    private Slider scanProgressSlider;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // LogWindowManager가 활성화될 때, 스캔 명령 이벤트 구독
        if (logWindow != null)
            logWindow.OnScanCommandEntered += HandleScanCommand;
    }

    private void OnDisable()
    {
        // 비활성화 시 이벤트 구독 해제 (메모리 누수 방지)
        if (logWindow != null)
            logWindow.OnScanCommandEntered -= HandleScanCommand;
    }

    /// <summary>
    /// 로그창에서 '스캔' 명령이 입력되었을 때 호출되는 메서드
    /// </summary>
    /// <param name="folderName">사용자가 입력한 폴더 이름</param>
    private void HandleScanCommand(string folderName)
    {
        // 이미 스캔 중이면 중복 실행 방지
        if (isScanning)
        {
            logWindow.Log("스캔 중입니다...");
            return;
        }

        // FileWindow에서 루트 폴더 가져오기
        Folder root = fileWindow.GetRootFolder();

        // 이름으로 대상 폴더 탐색
        Folder target = FindFolderByName(root, folderName);

        if (target == null)
        {
            logWindow.Log("대상 폴더를 찾을 수 없습니다.");
            return;
        }

        // 코루틴으로 스캔 시작
        StartCoroutine(ScanFolderCoroutine(target));
    }

    /// <summary>
    /// 지정한 폴더를 비동기로 스캔하며 진행 상황을 UI 슬라이더로 표시
    /// </summary>
    private IEnumerator ScanFolderCoroutine(Folder folder)
    {
        isScanning = true;
        logWindow.DisableInput();

        // 팝업 생성
        scanPopupInstance = Instantiate(scanPopupPrefab, parentCanvas.transform);
        scanProgressSlider = scanPopupInstance.GetComponentInChildren<Slider>();
        scanProgressSlider.value = 0f;

        int totalItems = CountAllFilesAndFolders(folder);
        float totalScanTime = totalItems * 3f;
        int abnormalCount = CountAbnormal(folder);

        logWindow.Log("이상 스캔중...");


        // --- 버퍼링 구간 생성 ---
        int bufferCount = Random.Range(4, 6); // 1~3개
        float[] bufferTimes = new float[bufferCount];
        float totalBufferTime = 0f;

        for (int i = 0; i < bufferCount; i++)
        {
            // totalScanTime 비례: 예를 들어 5~15% 비율 랜덤
            float ratio = Random.Range(0.1f, 0.15f);
            bufferTimes[i] = totalScanTime * ratio;
            totalBufferTime += bufferTimes[i];
        }

        // 실제 진행 시간 = 총 시간 - 버퍼링 합
        float progressTime = Mathf.Max(0f, totalScanTime - totalBufferTime);

        // 버퍼링 위치 설정 (0~1 구간 랜덤)
        float[] bufferPositions = new float[bufferCount];
        for (int i = 0; i < bufferCount; i++)
            bufferPositions[i] = Random.Range(0f, 1f);
        System.Array.Sort(bufferPositions);

        float elapsed = 0f;
        int bufferIndex = 0;

        while (elapsed < progressTime)
        {
            float delta = Time.deltaTime;
            elapsed += delta;
            float progress = Mathf.Clamp01(elapsed / progressTime);

            // 버퍼링 처리
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

        // 팝업 제거
        Destroy(scanPopupInstance);

        logWindow.EnableInput();
        isScanning = false;

        // 스캔 결과 로그
        logWindow.Log("이상 발견 개수: " + abnormalCount);
    }


    /// <summary>
    /// 폴더 이름으로 Folder 객체 찾기 (재귀 탐색)
    /// </summary>
    private Folder FindFolderByName(Folder folder, string name)
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

    /// <summary>
    /// 폴더 및 모든 하위 파일/폴더 수를 재귀적으로 계산
    /// </summary>
    private int CountAllFilesAndFolders(Folder folder)
    {
        int count = 1 + folder.files.Count;
        foreach (var child in folder.children)
            count += CountAllFilesAndFolders(child);
        return count;
    }

    /// <summary>
    /// 폴더 내부의 이상(abnormal) 파일 및 폴더 개수 계산
    /// </summary>
    private int CountAbnormal(Folder folder)
    {
        int count = folder.isImportant ? 1 : 0;
        foreach (var child in folder.children)
            count += CountAbnormal(child);
        foreach (var file in folder.files)
            if (file.isImportant) count++;
        return count;
    }
}

/// <summary>
/// FileWindow 클래스의 비공개 rootFolder 필드에 접근하기 위한 확장 메서드
/// - 리플렉션을 이용해 비공개 필드 "rootFolder" 값을 가져옴
/// </summary>
public static class FileWindowExtensions
{
    public static Folder GetRootFolder(this FileWindow window)
    {
        var field = typeof(FileWindow).GetField("rootFolder",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field.GetValue(window) as Folder;
    }
}

/// <summary>
/// 이상 파일/폴더 개수 계산 전용 헬퍼
/// </summary>
public static class AbnormalDetector
{
    public static int GetAbnormalCount(Folder folder)
    {
        if (folder == null) return 0;

        int count = folder.isImportant ? 1 : 0;

        foreach (var child in folder.children)
            count += GetAbnormalCount(child);

        foreach (var file in folder.files)
            if (file.isImportant) count++;

        return count;
    }
}
