using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Slider ���

/// <summary>
/// ScanCommandManager
/// - �α� â���� �Էµ� '��ĵ ���'�� �����ϰ� �����ϴ� �Ŵ���
/// - ���� ������ ��ȸ�ϸ� �̻� ����/������ Ž���ϰ� ������� ǥ����
/// </summary>
public class ScanCommandManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static ScanCommandManager Instance;

    // ���� ������ UI�� �����ϴ� FileWindow
    public FileWindow fileWindow;

    // �α� �� ��ɾ� �Է��� ó���ϴ� LogWindowManager
    public LogWindowManager logWindow;

    // ��ĵ �˾� ������ (�����̴� ���Ե� UI)
    public GameObject scanPopupPrefab;

    // �˾��� ���� Canvas
    public Canvas parentCanvas;

    // ��ĵ ������ ���θ� ��Ÿ���� �÷���
    private bool isScanning = false;

    // ���� ������ ��ĵ �˾� �ν��Ͻ�
    private GameObject scanPopupInstance;

    // ������ �˾� ���� �����̴�
    private Slider scanProgressSlider;

    private void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // LogWindowManager�� Ȱ��ȭ�� ��, ��ĵ ��� �̺�Ʈ ����
        if (logWindow != null)
            logWindow.OnScanCommandEntered += HandleScanCommand;
    }

    private void OnDisable()
    {
        // ��Ȱ��ȭ �� �̺�Ʈ ���� ���� (�޸� ���� ����)
        if (logWindow != null)
            logWindow.OnScanCommandEntered -= HandleScanCommand;
    }

    /// <summary>
    /// �α�â���� '��ĵ' ����� �ԷµǾ��� �� ȣ��Ǵ� �޼���
    /// </summary>
    /// <param name="folderName">����ڰ� �Է��� ���� �̸�</param>
    private void HandleScanCommand(string folderName)
    {
        // �̹� ��ĵ ���̸� �ߺ� ���� ����
        if (isScanning)
        {
            logWindow.Log("��ĵ ���Դϴ�...");
            return;
        }

        // FileWindow���� ��Ʈ ���� ��������
        Folder root = fileWindow.GetRootFolder();

        // �̸����� ��� ���� Ž��
        Folder target = FindFolderByName(root, folderName);

        if (target == null)
        {
            logWindow.Log("��� ������ ã�� �� �����ϴ�.");
            return;
        }

        // �ڷ�ƾ���� ��ĵ ����
        StartCoroutine(ScanFolderCoroutine(target));
    }

    /// <summary>
    /// ������ ������ �񵿱�� ��ĵ�ϸ� ���� ��Ȳ�� UI �����̴��� ǥ��
    /// </summary>
    private IEnumerator ScanFolderCoroutine(Folder folder)
    {
        isScanning = true;
        logWindow.DisableInput();

        // �˾� ����
        scanPopupInstance = Instantiate(scanPopupPrefab, parentCanvas.transform);
        scanProgressSlider = scanPopupInstance.GetComponentInChildren<Slider>();
        scanProgressSlider.value = 0f;

        int totalItems = CountAllFilesAndFolders(folder);
        float totalScanTime = totalItems * 3f;
        int abnormalCount = CountAbnormal(folder);

        logWindow.Log("�̻� ��ĵ��...");

        
        // --- ���۸� ���� ���� ---
        int bufferCount = Random.Range(4, 5); // 1~3��
        float[] bufferTimes = new float[bufferCount];
        float totalBufferTime = 0f;
        for (int i = 0; i < bufferCount; i++)
        {
            bufferTimes[i] = Random.Range(2f, 3.5f);
            totalBufferTime += bufferTimes[i];
        }

        // ���� ���� �ð� = �� �ð� - ���۸� ��
        float progressTime = Mathf.Max(0f, totalScanTime - totalBufferTime);

        // ���۸� ��ġ ���� (0~1 ����) + ����
        float[] bufferPositions = new float[bufferCount];
        for (int i = 0; i < bufferCount; i++)
            bufferPositions[i] = Random.Range(0f, 1f);
        System.Array.Sort(bufferPositions);  //  �� ����

        float elapsed = 0f;
        int bufferIndex = 0;

        while (elapsed < progressTime)
        {
            float delta = Time.deltaTime;
            elapsed += delta;
            float progress = Mathf.Clamp01(elapsed / progressTime);

            // ���۸� ó��
            while (bufferIndex < bufferCount && progress >= bufferPositions[bufferIndex])
            {
                yield return new WaitForSeconds(bufferTimes[bufferIndex]);
                bufferIndex++;
            }

            scanProgressSlider.value = progress;
            yield return null;
        }

        scanProgressSlider.value = 1f;

        logWindow.ReplaceLastScanLog("��ĵ �Ϸ�");

        // �˾� ����
        Destroy(scanPopupInstance);

        logWindow.EnableInput();
        isScanning = false;

        // ��ĵ ��� �α�
        logWindow.Log("�̻� �߰� ����: " + abnormalCount);
    }


    /// <summary>
    /// ���� �̸����� Folder ��ü ã�� (��� Ž��)
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
    /// ���� �� ��� ���� ����/���� ���� ��������� ���
    /// </summary>
    private int CountAllFilesAndFolders(Folder folder)
    {
        int count = 1 + folder.files.Count;
        foreach (var child in folder.children)
            count += CountAllFilesAndFolders(child);
        return count;
    }

    /// <summary>
    /// ���� ������ �̻�(abnormal) ���� �� ���� ���� ���
    /// </summary>
    private int CountAbnormal(Folder folder)
    {
        int count = folder.isAbnormal ? 1 : 0;
        foreach (var child in folder.children)
            count += CountAbnormal(child);
        foreach (var file in folder.files)
            if (file.isAbnormal) count++;
        return count;
    }
}

/// <summary>
/// FileWindow Ŭ������ ����� rootFolder �ʵ忡 �����ϱ� ���� Ȯ�� �޼���
/// - ���÷����� �̿��� ����� �ʵ� "rootFolder" ���� ������
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
/// �̻� ����/���� ���� ��� ���� ����
/// </summary>
public static class AbnormalDetector
{
    public static int GetAbnormalCount(Folder folder)
    {
        if (folder == null) return 0;

        int count = folder.isAbnormal ? 1 : 0;

        foreach (var child in folder.children)
            count += GetAbnormalCount(child);

        foreach (var file in folder.files)
            if (file.isAbnormal) count++;

        return count;
    }
}
