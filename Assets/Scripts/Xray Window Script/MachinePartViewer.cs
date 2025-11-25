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

    // 추가됨: 클릭 토글 여부
    public bool isChecked = false;
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
                part.button.onClick.AddListener(() => OnPartClicked(nameCopy));
            }
        }

        // 기존 빨간색 적용 기능 제거
        // UpdateErrorButtonColors();  <-- 삭제됨

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

    // ========================= 클릭 이벤트 =========================
    private void OnPartClicked(string partName)
    {
        // 1) 해당 이미지 표시 (UpdateButtonVisuals 호출 X)
        ShowPart(partName);

        // 2) 체크 토글
        if (partDict.TryGetValue(partName, out var part))
        {
            part.isChecked = !part.isChecked;
        }

        // 3) 색상 1번만 업데이트 → 깜빡임 제거
        UpdateButtonVisuals();
    }
    // ===============================================================

    private void ShowPart(string partName)
    {
        if (!partDict.TryGetValue(partName, out var part))
            return;

        if (cameraPanel == null)
            return;

        // 에러면 에러 스프라이트 표시, 아니면 정상 표시
        cameraPanel.sprite = part.isError && part.errorSprite != null
            ? part.errorSprite
            : part.normalSprite;

        // ※ 여기에 UpdateButtonVisuals() 호출했던 것이 깜빡임 원인 → 제거됨
    }

    public void SetPartError(string partName, bool isError)
    {
        if (partDict.TryGetValue(partName, out var part))
        {
            part.isError = isError;
            // 버튼 색과 무관, 갱신 필요 없음
        }
    }

    private void UpdateButtonVisuals()
    {
        foreach (var kvp in partDict)
        {
            var part = kvp.Value;
            if (part.button == null) continue;

            ColorBlock colors = part.button.colors;

            // 에러도 색 바꾸지 않음 → isChecked만 적용
            if (part.isChecked)
            {
                // 초록색
                colors.normalColor = Color.green;
                colors.highlightedColor = Color.green;
                colors.pressedColor = Color.green;
                colors.selectedColor = Color.green;
            }
            else
            {
                // 기본 흰색
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
            }

            part.button.colors = colors;
        }
    }

    public void DisplayInitialize()
    {
        cameraPanel = null;

        foreach (var kvp in partDict)
        {
            var part = kvp.Value;
            if (part.button == null) continue;

            ColorBlock colors = part.button.colors;

            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;

            part.button.colors = colors;
        }
    }
}

