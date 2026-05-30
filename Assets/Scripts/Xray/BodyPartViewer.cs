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
/// Xray 패널에서 신체 부위 버튼을 클릭하면
/// 해당 폴더의 이상 여부에 따라 스프라이트를 교체한다.
/// </summary>
public class BodyPartViewer : MonoBehaviour
{
    [Header("카메라창")]
    public Image cameraPanel;

    [Header("신체 부위 목록")]
    public BodyPart[] bodyParts;

    [Header("참조")]
    public FileWindow fileWindow;

    // 부위명 → 폴더 경로 (FileWindow의 폴더 트리와 동일한 계층)
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
        if (cameraPanel == null || fileWindow == null) return;

        if (!PartFolderPaths.TryGetValue(partName, out var path))
        {
            Debug.LogWarning($"[BodyPartViewer] 알 수 없는 부위: {partName}");
            return;
        }

        Folder target = fileWindow.GetRootFolder();
        foreach (var node in path)
        {
            target = target?.children.Find(f => f.name == node);
            if (target == null)
            {
                Debug.LogWarning($"[BodyPartViewer] 폴더 경로 탐색 실패: {string.Join("/", path)}");
                return;
            }
        }

        var part = System.Array.Find(bodyParts, p => p.partName == partName);
        if (part == null) return;

        bool isAbnormal = AbnormalDetector.HasAbnormal(target);
        cameraPanel.sprite = (isAbnormal && part.abnormalSprite != null)
            ? part.abnormalSprite
            : part.normalSprite;
    }
}
