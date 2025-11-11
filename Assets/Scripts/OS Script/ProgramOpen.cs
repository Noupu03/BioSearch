using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProgramOpen : MonoBehaviour
{
    [Header("Desktop Area")]
    public Transform desktopArea; // 바탕화면 아이콘 부모

    [Header("아이콘 프리팹")]
    public GameObject cmdIconPrefab;
    public GameObject fileExplorerIconPrefab;
    public GameObject displayIconPrefab;
    public GameObject messengerIconPrefab; // 메신저 아이콘 프리팹 추가

    [Header("프로그램 프리팹")]
    public GameObject cmdProgramPrefab;
    public GameObject fileExplorerProgramPrefab;
    public GameObject displayProgramPrefab;
    public GameObject messengerProgramPrefab; // 메신저 프로그램 프리팹 추가
    public GameObject messengerWindowprefab;

    [Header("X 버튼 프리팹")]
    public GameObject xButtonPrefab;

    [Header("최소화 버튼 프리팹")]
    public GameObject minimizeButtonPrefab; // ★ 새로 추가

    [Header("하단바 영역")]
    public Transform taskbarArea; // 하단바 아이콘 부모

    [Header("하단바 아이콘 프리팹")]
    public GameObject taskbarCmdIconPrefab;
    public GameObject taskbarFileExplorerIconPrefab;
    public GameObject taskbarDisplayIconPrefab;
    public GameObject taskbarMessengerIconPrefab; // 메신저 하단바 아이콘 프리팹 추가

    private List<GameObject> icons = new List<GameObject>();
    private List<GameObject> taskbarIcons = new List<GameObject>();

    private float iconSpacing = 20f; // 아이콘 간격

    void Start()
    {
        CreateDesktopIcons();
        CreateTaskbarIcons();
    }

    void CreateDesktopIcons()
    {
        // 아이콘 목록
        var iconInfos = new List<(GameObject iconPrefab, GameObject programPrefab)>
        {
            (cmdIconPrefab, cmdProgramPrefab),
            (fileExplorerIconPrefab, fileExplorerProgramPrefab),
            (displayIconPrefab, displayProgramPrefab),
            (messengerIconPrefab, messengerProgramPrefab) // 메신저 추가
        };

        float startY = 0f;

        foreach (var info in iconInfos)
        {
            GameObject icon = Instantiate(info.iconPrefab, desktopArea);
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, startY);
            startY -= iconSpacing;

            icons.Add(icon);

            // 더블클릭 처리
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
        // 하단바 아이콘 목록
        var taskbarIconInfos = new List<(GameObject iconPrefab, GameObject programPrefab)>
        {
            (taskbarCmdIconPrefab, cmdProgramPrefab),
            (taskbarFileExplorerIconPrefab, fileExplorerProgramPrefab),
            (taskbarDisplayIconPrefab, displayProgramPrefab),
            (taskbarMessengerIconPrefab, messengerProgramPrefab) // 메신저 추가
        };

        float startX = 0f;
        float spacing = 5f; // 하단바 아이콘 간격

        foreach (var info in taskbarIconInfos)
        {
            GameObject icon = Instantiate(info.iconPrefab, taskbarArea);
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX, 0);
            startX += spacing;

            taskbarIcons.Add(icon);

            // 클릭 시 프로그램 활성화
            Button btn = icon.GetComponent<Button>();
            if (btn == null) btn = icon.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                OpenProgram(info.programPrefab);
            });
        }
    }

    void OpenProgram(GameObject programPrefab)
    {
        if (programPrefab == null) return;

        // 프로그램 활성화
        programPrefab.SetActive(true);

        // X버튼 처리
        Transform existingX = programPrefab.transform.Find("X_Button");
        Button xBtn;
        RectTransform xRt;

        if (existingX == null)
        {
            GameObject xBtnObj = Instantiate(xButtonPrefab, programPrefab.transform);
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
            xBtn.onClick.RemoveAllListeners(); // 기존 리스너 초기화
        }

        xBtn.onClick.AddListener(() =>
        {
            programPrefab.SetActive(false);


            // 재활성화될 때까지 대기 후 초기화
            StartCoroutine(InvokeAfterActivation(programPrefab));
        });

        // 최소화 버튼 처리
        if (minimizeButtonPrefab != null)
        {
            Transform existingMin = programPrefab.transform.Find("Minimize_Button");
            Button minBtn;
            if (existingMin == null)
            {
                GameObject minBtnObj = Instantiate(minimizeButtonPrefab, programPrefab.transform);
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
                programPrefab.SetActive(false);
            });
        }
        else
        {
            Debug.LogWarning("minimizeButtonPrefab이 연결되지 않았습니다.");
        }
    }


    private System.Collections.IEnumerator InvokeAfterActivation(GameObject programPrefab)
    {
        // 다음 프레임까지 대기 (SetActive가 적용된 후)
        yield return null;

        // programPrefab이 활성화될 때까지 대기
        while (!programPrefab.activeInHierarchy)
            yield return null;

        // 활성화되면 초기화 호출

        var CMD = programPrefab.GetComponent<LogWindowManager>();
        if (CMD != null)
            CMD.CMDInitialize();

        var display = programPrefab.GetComponent<MachinePartViewer>();
        if (display != null)
            display.DisplayInitialize();

        var fileExploer = programPrefab.GetComponent<FileWindow>();
        if (fileExploer != null)
            fileExploer.FileExplorerInitialize();
    }



    public void MessangerWindowOpen()
    {
        Debug.Log("MessangerWindowOpen() 호출됨!");
        OpenProgram(messengerWindowprefab);
    }
}
