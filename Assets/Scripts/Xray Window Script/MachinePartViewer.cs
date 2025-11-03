using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class BodyPart
{
    public string partName;       // 부위 이름
    public Button button;         // 해당 부위 버튼
    public Sprite normalSprite;   // 정상 스프라이트
    public Sprite errorSprite;    // 고장 스프라이트
    public bool isError;          // 고장 여부
}

public class MachinePartViewer : MonoBehaviour
{
    [Header("왼쪽 카메라창")]
    [SerializeField] private Image cameraPanel;

    // 내부에서 관리하는 신체 부위들
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
        // 버튼 클릭 이벤트 등록
        foreach (var kvp in partDict)
        {
            var part = kvp.Value;
            if (part.button != null)
            {
                string nameCopy = part.partName;
                part.button.onClick.AddListener(() => ShowPart(nameCopy));
            }
        }
    }

    /// <summary>
    /// BodyPart 이름-객체 매핑 초기화
    /// </summary>
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

        // 각 BodyPart의 이름 자동 지정
        foreach (var kvp in partDict)
        {
            kvp.Value.partName = kvp.Key;
        }
    }

    /// <summary>
    /// 씬 내 버튼 이름과 BodyPart 이름을 자동 매핑
    /// </summary>
    private void AutoAssignBodyParts()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true); // 비활성 포함
        foreach (var button in allButtons)
        {
            if (partDict.TryGetValue(button.name, out var bodyPart))
            {
                bodyPart.button = button;
            }
        }
    }

    /// <summary>
    /// 클릭 시 해당 부위 스프라이트 표시
    /// </summary>
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

        // 고장 여부에 따라 스프라이트 선택
        cameraPanel.sprite = part.isError && part.errorSprite != null
            ? part.errorSprite
            : part.normalSprite;
    }
}
