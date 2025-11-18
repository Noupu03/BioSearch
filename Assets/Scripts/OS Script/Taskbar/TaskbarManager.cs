// TaskbarManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TaskbarManager : MonoBehaviour
{
    public static TaskbarManager Instance;

    [Header("Taskbar Content Area")]
    public Transform taskbarContentArea; // 버튼이 생성될 부모

    [Header("Taskbar Button Prefab")]
    public GameObject taskbarButtonPrefab; // 버튼 프리팹

    private Dictionary<GameObject, GameObject> programToButton = new Dictionary<GameObject, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void AddTaskbarButton(string programName, GameObject programInstance)
    {
        if (programToButton.ContainsKey(programInstance))
            return;

        GameObject btnObj = Instantiate(taskbarButtonPrefab, taskbarContentArea);
        btnObj.name =  programName;

        // TMP Text 변경
        TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = programName;

        TaskbarIcon iconScript = btnObj.GetComponent<TaskbarIcon>();
        if (iconScript == null)
            iconScript = btnObj.AddComponent<TaskbarIcon>();

        iconScript.Setup(programInstance);

        programToButton.Add(programInstance, btnObj);
    }


    public void RemoveTaskbarButton(GameObject programInstance)
    {
        if (!programToButton.ContainsKey(programInstance)) return;

        Destroy(programToButton[programInstance]);
        programToButton.Remove(programInstance);
    }
}
