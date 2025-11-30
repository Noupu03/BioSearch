using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MachinePartViewer : MonoBehaviour
{
    public static MachinePartViewer Instance;

    [Header("좌측 카메라창")]
    [SerializeField] private Image cameraPanel;

    [Header("BodyPartsData 참조")]
    [SerializeField] private BodyPartsData bodyPartsData;

    [Header("각 BodyPart와 매핑될 버튼들")]
    [SerializeField] private Button HeadBtn;
    [SerializeField] private Button ChestBtn;
    [SerializeField] private Button LeftUpperArmBtn;
    [SerializeField] private Button LeftForeArmBtn;
    [SerializeField] private Button LeftHandBtn;
    [SerializeField] private Button RightUpperArmBtn;
    [SerializeField] private Button RightForeArmBtn;
    [SerializeField] private Button RightHandBtn;
    [SerializeField] private Button StomachBtn;
    [SerializeField] private Button PelvisBtn;
    [SerializeField] private Button LeftThighBtn;
    [SerializeField] private Button LeftCalfBtn;
    [SerializeField] private Button LeftFootBtn;
    [SerializeField] private Button RightThighBtn;
    [SerializeField] private Button RightCalfBtn;
    [SerializeField] private Button RightFootBtn;

    private Dictionary<string, Button> buttonDict;

    private void Awake()
    {
        Instance = this;
        BuildButtonDict();
        RegisterButtonEvents();
    }

    private void BuildButtonDict()
    {
        buttonDict = new Dictionary<string, Button>
        {
            { "Head", HeadBtn },
            { "Chest", ChestBtn },
            { "LeftUpperArm", LeftUpperArmBtn },
            { "LeftForeArm", LeftForeArmBtn },
            { "LeftHand", LeftHandBtn },
            { "RightUpperArm", RightUpperArmBtn },
            { "RightForeArm", RightForeArmBtn },
            { "RightHand", RightHandBtn },
            { "Stomach", StomachBtn },
            { "Pelvis", PelvisBtn },
            { "LeftThigh", LeftThighBtn },
            { "LeftCalf", LeftCalfBtn },
            { "LeftFoot", LeftFootBtn },
            { "RightThigh", RightThighBtn },
            { "RightCalf", RightCalfBtn },
            { "RightFoot", RightFootBtn }
        };
    }

    private void RegisterButtonEvents()
    {
        foreach (var kvp in buttonDict)
        {
            string partName = kvp.Key;
            Button btn = kvp.Value;

            if (btn != null)
                btn.onClick.AddListener(() => OnPartClicked(partName));
        }
    }

    private void OnPartClicked(string partName)
    {
        BodyPartDataObject part = bodyPartsData.GetPart(partName);
        if (part == null) return;

        part.isChecked = !part.isChecked;

        UpdateButtonVisual(partName, part.isChecked);

        cameraPanel.sprite =
            part.isError ? part.errorSprite : part.normalSprite;
    }

    private void UpdateButtonVisual(string name, bool isChecked)
    {
        if (!buttonDict.TryGetValue(name, out Button btn))
            return;

        ColorBlock c = btn.colors;
        if (isChecked)
        {
            c.normalColor = Color.green;
            c.highlightedColor = Color.green;
        }
        else
        {
            c.normalColor = Color.white;
            c.highlightedColor = Color.white;
        }
        btn.colors = c;
    }
}
