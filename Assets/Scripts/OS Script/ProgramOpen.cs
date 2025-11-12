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

    private List<GameObject> icons = new List<GameObject>();
    private List<GameObject> taskbarIcons = new List<GameObject>();

    // 기존 private Dictionary<GameObject, GameObject> activeInstances;
    private Dictionary<GameObject, GameObject> activeInstances = new Dictionary<GameObject, GameObject>();

    // 외부에서 읽기 전용으로 접근 가능하게 프로퍼티 추가
    public GameObject GetActiveInstance(GameObject prefab)
    {
        if (activeInstances.ContainsKey(prefab))
            return activeInstances[prefab];
        return null;
    }

    private float iconSpacing = 20f;

    void Start()
    {
        CreateDesktopIcons();
        CreateTaskbarIcons();
    }

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
            if (clickHandler == null) clickHandler = icon.AddComponent<IconClickHandler>();
            clickHandler.onDoubleClick = () =>
            {
                OpenProgram(info.programPrefab);
            };
        }
    }

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
            if (btn == null) btn = icon.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                OpenProgram(info.programPrefab);
            });
        }
    }

    GameObject OpenProgram(GameObject programPrefab)
    {
        if (programPrefab == null) return null;

        if (activeInstances.ContainsKey(programPrefab))
        {
            var existing = activeInstances[programPrefab];
            if (existing == null)
            {
                existing = InstantiateProgram(programPrefab);
                activeInstances[programPrefab] = existing;
            }
            else if (!existing.activeSelf)
            {
                existing.SetActive(true);
            }
            return existing;
        }

        // 처음 실행되는 경우
        GameObject instance = InstantiateProgram(programPrefab);
        activeInstances[programPrefab] = instance;
        return instance;
    }


    GameObject InstantiateProgram(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, transform.parent);
        instance.name = prefab.name;

        // X 버튼 생성
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

        // X 버튼 → 완전 삭제
        xBtn.onClick.AddListener(() =>
        {
            Destroy(instance);
            RemoveInstance(prefab: prefab);
        });

        // 최소화 버튼 처리
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
        else
        {
            Debug.LogWarning("minimizeButtonPrefab이 연결되지 않았습니다.");
        }

        return instance;
    }

    void RemoveInstance(GameObject prefab)
    {
        if (activeInstances.ContainsKey(prefab))
        {
            activeInstances.Remove(prefab);
        }
    }

    public void MessangerWindowOpen()
    {
        // messengerWindowprefab 인스턴스 열기
        GameObject instance = OpenProgram(messengerWindowprefab);

        // Notifier에 실제 인스턴스 전달
        MessengerNotifier notifier = FindObjectOfType<MessengerNotifier>();
        if (notifier != null && instance != null)
        {
            notifier.SetMessengerProgramInstance(instance);
        }
    }



}
