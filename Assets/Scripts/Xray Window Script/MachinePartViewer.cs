using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class BodyPart
{
    public string partName;
    public Button button;
    public Sprite normalSprite;
    public Sprite errorSprite;
    public bool isError;
}

public class MachinePartViewer : MonoBehaviour
{
    [Header("왼쪽 카메라창")]
    [SerializeField] private Image cameraPanel;

    [Header("버튼 색상 관리 매니저")]
    [SerializeField] private ButtonColorManager buttonManager;

    [SerializeField] private BodyPart Head = new BodyPart();
    [SerializeField] private BodyPart Chest = new BodyPart();
    [SerializeField] private BodyPart LeftUpperArm = new BodyPart();
    [SerializeField] private BodyPart LeftForeArm = new BodyPart();
    [SerializeField] private BodyPart LeftHand = new BodyPart();
    [SerializeField] private BodyPart RightUpperArm = new BodyPart();
    [SerializeField] private BodyPart RightForeArm = new BodyPart();
    [SerializeField] private BodyPart RightHand = new BodyPart();
    [SerializeField] private BodyPart Stomach = new BodyPart();
    [SerializeField] private BodyPart Pelvis = new BodyPart();
    [SerializeField] private BodyPart LeftThigh = new BodyPart();
    [SerializeField] private BodyPart LeftCalf = new BodyPart();
    [SerializeField] private BodyPart LeftFoot = new BodyPart();
    [SerializeField] private BodyPart RightThigh = new BodyPart();
    [SerializeField] private BodyPart RightCalf = new BodyPart();
    [SerializeField] private BodyPart RightFoot = new BodyPart();

    private Dictionary<string, BodyPart> partDict;

    void Awake()
    {
        InitializeDictionary();
        AutoAssignBodyParts();
    }

    void Start()
    {
        foreach (var kvp in partDict)
        {
            var part = kvp.Value;
            if (part.button != null)
            {
                string nameCopy = part.partName;
                part.button.onClick.AddListener(() => ShowPart(nameCopy));
            }
        }
        UpdateErrorButtonColors();
        UpdateButtonVisuals();
    }



    private void InitializeDictionary()
    {
        partDict = new Dictionary<string, BodyPart>
        {
            { "Head", Head },
            { "Chest", Chest },
            { "LeftUpperArm", LeftUpperArm },
            { "LeftForeArm", LeftForeArm },
            { "LeftHand", LeftHand },
            { "RightUpperArm", RightUpperArm },
            { "RightForeArm", RightForeArm },
            { "RightHand", RightHand },
            { "Stomach", Stomach },
            { "Pelvis", Pelvis },
            { "LeftThigh", LeftThigh },
            { "LeftCalf", LeftCalf },
            { "LeftFoot", LeftFoot },
            { "RightThigh", RightThigh },
            { "RightCalf", RightCalf },
            { "RightFoot", RightFoot }
        };

        foreach (var kvp in partDict)
            kvp.Value.partName = kvp.Key;
    }

    private void AutoAssignBodyParts()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (var button in allButtons)
        {
            if (partDict.TryGetValue(button.name, out var bodyPart))
            {
                bodyPart.button = button;
            }
        }
    }

    private void ShowPart(string partName)
    {
        if (!partDict.TryGetValue(partName, out var part))
        {
            Debug.LogWarning("해당 부위를 찾을 수 없습니다: " + partName);
            return;
        }

        if (cameraPanel == null)
        {
            Debug.LogWarning("cameraPanel이 설정되지 않았습니다.");
            return;
        }

        cameraPanel.sprite = part.isError && part.errorSprite != null
            ? part.errorSprite
            : part.normalSprite;

        UpdateButtonVisuals();
    }

    /// <summary>
    /// 외부에서 고장 상태 변경 시 호출
    /// </summary>
    public void SetPartError(string partName, bool isError)
    {
        if (partDict.TryGetValue(partName, out var part))
        {
            part.isError = isError;
            UpdateButtonVisuals();
        }
    }

    /// <summary>
    /// BodyButtonManager와 색상 동기화
    /// </summary>
    private void UpdateButtonVisuals()
    {
        if (buttonManager == null) return;

        Dictionary<string, bool> errorStates = new Dictionary<string, bool>();
        foreach (var kvp in partDict)
        {
            errorStates[kvp.Key] = kvp.Value.isError;
        }

        buttonManager.UpdateErrorButtonColors(errorStates);
    }

    private void UpdateErrorButtonColors()
    {
        foreach (var kvp in partDict)
        {
            var part = kvp.Value;
            if (part.button == null) continue;

            ColorBlock colors = part.button.colors;
            if (part.isError)
            {
                colors.normalColor = Color.red;
                colors.highlightedColor = Color.red;
                colors.pressedColor = Color.red;
                colors.selectedColor = Color.red;
            }
            part.button.colors = colors;
        }
    }
    /// <summary>
    /// 프로그램 종료 시 초기화용 함수
    /// 모든 버튼 색상을 기본 상태로 재설정하고 cameraPanel을 null로 설정
    /// </summary>
    public void DisplayInitialize()
    {
        // cameraPanel 초기화
        cameraPanel = null;

        // 버튼 색상 초기화
        foreach (var kvp in partDict)
        {
            var part = kvp.Value;
            if (part.button == null) continue;

            ColorBlock colors = part.button.colors;

            // 빨간색 상태는 유지
            if (!part.isError)
            {
                // 기본 색상으로 초기화
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
            }

            part.button.colors = colors;
        }

        Debug.Log("[MachinePartViewer] DisplayInitialize() 실행됨");
    }

}
