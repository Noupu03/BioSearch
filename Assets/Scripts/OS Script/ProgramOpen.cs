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

    [Header("프로그램 프리팹")]
    public GameObject cmdProgramPrefab;
    public GameObject fileExplorerProgramPrefab;
    public GameObject displayProgramPrefab;

    [Header("X 버튼 프리팹")]
    public GameObject xButtonPrefab;

    private List<GameObject> icons = new List<GameObject>();

    private float iconSpacing = 40f; // 아이콘 간격
    void Start()
    {
        CreateDesktopIcons();
    }

    void CreateDesktopIcons()
    {
        // 아이콘 목록
        var iconInfos = new List<(GameObject iconPrefab, GameObject programPrefab)>
    {
        (cmdIconPrefab, cmdProgramPrefab),
        (fileExplorerIconPrefab, fileExplorerProgramPrefab),
        (displayIconPrefab, displayProgramPrefab)
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

    void OpenProgram(GameObject programPrefab)
    {
        if (programPrefab == null) return;

        // 프로그램 활성화
        programPrefab.SetActive(true);

        // 이미 X버튼이 있으면 다시 만들지 않음
        Transform existingX = programPrefab.transform.Find("X_Button");
        if (existingX != null) return;

        // X 버튼 생성
        GameObject xBtnObj = Instantiate(xButtonPrefab, programPrefab.transform);
        xBtnObj.name = "X_Button";
        RectTransform rt = xBtnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(0, 0); // 우측 상단 위치

        Button xBtn = xBtnObj.GetComponent<Button>();
        xBtn.onClick.AddListener(() =>
        {
            programPrefab.SetActive(false);
        });
    }


}
