using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class BodyPart
{
    public string partName;
    public Button button;
    public Sprite normalSprite;
    public Sprite abnormalSprite;
}

/// <summary>
/// Xray 패널에서 신체 부위 버튼을 클릭하면 해당 폴더의 이상 여부에 따라
/// 카메라 패널 스프라이트를 교체한다.
/// FileWindow는 Instance로 접근하므로 인스펙터 크로스 참조가 없다.
/// </summary>
public class BodyPartViewer : MonoBehaviour
{
    [Header("카메라창")]
    public Image cameraPanel;

    [Header("신체 부위 목록")]
    public BodyPart[] bodyParts;

    private static readonly Dictionary<string, string[]> PartFolderPaths =
        new Dictionary<string, string[]>
        {
            { "Head",         new[] { "Head" } },
            { "Chest",        new[] { "Body",    "Chest" } },
            { "LeftUpperArm", new[] { "LeftArm", "UpperArm" } },
            { "LeftForeArm",  new[] { "LeftArm", "ForeArm" } },
            { "LeftHand",     new[] { "LeftArm", "Hand" } },
            { "RightUpperArm",new[] { "RightArm","UpperArm" } },
            { "RightForeArm", new[] { "RightArm","ForeArm" } },
            { "RightHand",    new[] { "RightArm","Hand" } },
            { "Stomach",      new[] { "Organ",   "Stomach" } },
            { "Pelvis",       new[] { "Body",    "Pelvis" } },
            { "LeftThigh",    new[] { "LeftLeg", "Thigh" } },
            { "LeftCalf",     new[] { "LeftLeg", "Calf" } },
            { "LeftFoot",     new[] { "LeftLeg", "Foot" } },
            { "RightThigh",   new[] { "RightLeg","Thigh" } },
            { "RightCalf",    new[] { "RightLeg","Calf" } },
            { "RightFoot",    new[] { "RightLeg","Foot" } },
        };

    void Start()
    {
        foreach (var part in bodyParts)
        {
            string name = part.partName;
            if (part.button != null)
                part.button.onClick.AddListener(() => ShowPart(name));
        }
    }

    public void ShowPart(string partName)
    {
        if (cameraPanel == null) return;

        var fw = FileWindow.Instance;
        if (fw == null) return;

        if (!PartFolderPaths.TryGetValue(partName, out var path)) return;

        Folder target = fw.GetRootFolder();
        foreach (var node in path)
        {
            target = target?.children.Find(f => f.name == node);
            if (target == null) return;
        }

        var part = System.Array.Find(bodyParts, p => p.partName == partName);
        if (part == null) return;

        bool isAbnormal    = AbnormalDetector.HasAbnormal(target);
        cameraPanel.sprite = (isAbnormal && part.abnormalSprite != null)
            ? part.abnormalSprite
            : part.normalSprite;
    }
}
