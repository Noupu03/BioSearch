using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProgramOpen : MonoBehaviour
{
    [Header("Desktop Area")]
    public Transform desktopArea;

    [Header("아이콘 프리팹")]
    public GameObject cmdIconPrefab;
    public GameObject fileExplorerIconPrefab;
    public GameObject displayIconPrefab;
    public GameObject messengerIconPrefab;

    [Header("프로그램 프리팹")]
    public GameObject cmdProgramPrefab;
    public GameObject fileExplorerProgramPrefab;
    public GameObject displayProgramPrefab;
    public GameObject messengerProgramPrefab;
    public GameObject messengerWindowprefab;

    [Header("X 버튼 프리팹")]
    public GameObject xButtonPrefab;

    [Header("최소화 버튼 프리팹")]
    public GameObject minimizeButtonPrefab;

    [Header("하단바 영역")]
    public Transform taskbarArea;

    [Header("하단바 아이콘 프리팹")]
    public GameObject taskbarCmdIconPrefab;
    public GameObject taskbarFileExplorerIconPrefab;
    public GameObject taskbarDisplayIconPrefab;
    public GameObject taskbarMessengerIconPrefab;

    [Header("제출창 프리팹")]
    public GameObject submitWindowPrefab;

    private List<GameObject> icons = new List<GameObject>();
    private List<GameObject> taskbarIcons = new List<GameObject>();

    // 프로그램 이름(key) → 인스턴스
    private Dictionary<string, GameObject> activeInstances = new Dictionary<string, GameObject>();

    private float iconSpacing = 8f;

    void Awake()
    {
        // TaskbarManager 연결
        //if (TaskbarManager.Instance == null)
        //{
        //    Debug.LogError("TaskbarManager가 씬에 존재하지 않습니다!");
        //}
    }

    void Start()
    {
        CreateDesktopIcons();
        CreateTaskbarIcons();
    }

    // =========================================
    // DESKTOP 아이콘 생성
    // =========================================
    void CreateDesktopIcons()
    {
        var iconInfos = new List<(GameObject iconPrefab, GameObject programPrefab)>
        {
            (cmdIconPrefab, cmdProgramPrefab),
            (fileExplorerIconPrefab, fileExplorerProgramPrefab),
            (displayIconPrefab, displayProgramPrefab),
            (messengerIconPrefab, messengerProgramPrefab)
        };

        float startY = 0f;
        foreach (var info in iconInfos)
        {
            GameObject icon = Instantiate(info.iconPrefab, desktopArea);
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, startY);
            startY -= iconSpacing;

            icons.Add(icon);

            IconClickHandler clickHandler = icon.GetComponent<IconClickHandler>();
            if (clickHandler == null)
                clickHandler = icon.AddComponent<IconClickHandler>();

            GameObject capturedPrefab = info.programPrefab;
            clickHandler.onDoubleClick = () =>
            {
                OpenProgram(capturedPrefab);
            };
        }
    }

    // =========================================
    // TASKBAR 아이콘 생성
    // =========================================
    void CreateTaskbarIcons()
    {
        var taskbarIconInfos = new List<(GameObject iconPrefab, GameObject programPrefab)>
        {
            (taskbarCmdIconPrefab, cmdProgramPrefab),
            (taskbarFileExplorerIconPrefab, fileExplorerProgramPrefab),
            (taskbarDisplayIconPrefab, displayProgramPrefab),
            (taskbarMessengerIconPrefab, messengerProgramPrefab)
        };

        float startX = 0f;
        float spacing = 5f;

        foreach (var info in taskbarIconInfos)
        {
            GameObject icon = Instantiate(info.iconPrefab, taskbarArea);
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX, 0);
            startX += spacing;

            taskbarIcons.Add(icon);

            Button btn = icon.GetComponent<Button>();
            if (btn == null)
                btn = icon.AddComponent<Button>();

            GameObject capturedPrefab = info.programPrefab;
            btn.onClick.AddListener(() =>
            {
                OpenProgram(capturedPrefab);
            });
        }
    }

    // =========================================
    // PROGRAM OPEN
    // =========================================
    public GameObject OpenProgram(GameObject prefab)
    {
        if (prefab == null) return null;

        string key = prefab.name;

        // 이미 열려있으면 활성화
        if (activeInstances.ContainsKey(key))
        {
            GameObject existing = activeInstances[key];
            if (existing != null)
            {
                existing.SetActive(true);
                existing.transform.SetAsLastSibling();
            }
            return existing;
        }

        // 새 인스턴스 생성
        GameObject instance = InstantiateProgram(prefab);
        activeInstances[key] = instance;

        // FileExplorer 추가 기능
        if (prefab == fileExplorerProgramPrefab && instance != null)
        {
            FileWindow fileWindowPrefab = Resources.Load<FileWindow>("FileWindow");
            if (fileWindowPrefab != null)
            {
                FileWindow fileWindowInstance = Instantiate(fileWindowPrefab);
                fileWindowInstance.InitializeFileProgram();
            }
        }

        // OpenProgram 내부
        if (TaskbarManager.Instance != null)
        {
            TaskbarManager.Instance.AddTaskbarButton(key, instance);
        }


        return instance;
    }

    // =========================================
    // PROGRAM INSTANTIATE
    // =========================================
    GameObject InstantiateProgram(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, transform.parent);
        instance.name = prefab.name;
        instance.SetActive(true);
        instance.transform.SetAsLastSibling();

        // X 버튼
        Transform existingX = instance.transform.Find("X_Button");
        Button xBtn;
        RectTransform xRt;

        if (existingX == null)
        {
            GameObject xBtnObj = Instantiate(xButtonPrefab, instance.transform);
            xBtnObj.name = "X_Button";
            xRt = xBtnObj.GetComponent<RectTransform>();
            xRt.anchorMin = new Vector2(1, 1);
            xRt.anchorMax = new Vector2(1, 1);
            xRt.pivot = new Vector2(1, 1);
            xRt.anchoredPosition = new Vector2(0, 0);
            xBtn = xBtnObj.GetComponent<Button>();
        }
        else
        {
            xBtn = existingX.GetComponent<Button>();
            xRt = existingX.GetComponent<RectTransform>();
            xBtn.onClick.RemoveAllListeners();
        }

        string key = prefab.name;

        xBtn.onClick.AddListener(() =>
        {
            Destroy(instance);
            if (TaskbarManager.Instance != null)
                TaskbarManager.Instance.RemoveTaskbarButton(instance);
            RemoveInstance(key);
        });

        // Minimize 버튼
        if (minimizeButtonPrefab != null)
        {
            Transform existingMin = instance.transform.Find("Minimize_Button");
            Button minBtn;

            if (existingMin == null)
            {
                GameObject minBtnObj = Instantiate(minimizeButtonPrefab, instance.transform);
                minBtnObj.name = "Minimize_Button";
                RectTransform minRt = minBtnObj.GetComponent<RectTransform>();
                minRt.anchorMin = new Vector2(1, 1);
                minRt.anchorMax = new Vector2(1, 1);
                minRt.pivot = new Vector2(1, 1);
                minRt.anchoredPosition = new Vector2(-xRt.sizeDelta.x - 15f, 0);
                minBtn = minBtnObj.GetComponent<Button>();
            }
            else
            {
                minBtn = existingMin.GetComponent<Button>();
                minBtn.onClick.RemoveAllListeners();
            }

            minBtn.onClick.AddListener(() =>
            {
                instance.SetActive(false);
            });
        }

        return instance;
    }

    // =========================================
    // INSTANCE REMOVE
    // =========================================
    void RemoveInstance(string key)
    {
        if (activeInstances.ContainsKey(key))
            activeInstances.Remove(key);
    }

    // =========================================
    // SHOW PROGRAM BY KEY
    // =========================================
    public void ShowProgram(string key)
    {
        if (activeInstances.ContainsKey(key))
        {
            GameObject instance = activeInstances[key];
            if (instance != null)
            {
                instance.SetActive(true);
                instance.transform.SetAsLastSibling();
            }
        }
    }

    // =========================================
    // 기존 GetActiveInstance 유지
    // =========================================
    public GameObject GetActiveInstance(GameObject prefab)
    {
        string key = prefab.name;
        if (activeInstances.ContainsKey(key))
            return activeInstances[key];
        return null;
    }

    // =========================================
    // Messenger 전용
    // =========================================
    public void MessangerWindowOpen()
    {
        GameObject instance = OpenProgram(messengerWindowprefab);

        MessengerNotifier notifier = FindObjectOfType<MessengerNotifier>();
        if (notifier != null && instance != null)
        {
            notifier.SetMessengerProgramInstance(instance);
        }
    }
    public void OpenSubmitWindow()
    {
        GameObject instance = OpenProgram(submitWindowPrefab);

        // 필요한 초기화가 있으면 여기 추가 예정
    }
}
