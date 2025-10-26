using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class BodyPart
{
    public string partName;       // 부위 이름 (예: "LeftUpperArm")
    public Button button;         // 해당 부위 버튼
    public Sprite normalSprite;   // 정상 상태 스프라이트
    public Sprite abnormalSprite; // 이상 상태 스프라이트
}

public class BodyPartViewer : MonoBehaviour
{
    [Header("왼쪽 카메라창")]
    public Image cameraPanel;  // 스프라이트를 표시할 Image

    [Header("부위 & 버튼 매핑")]
    public BodyPart[] bodyParts;  // Inspector에서 부위, 버튼, 스프라이트 쌍 등록

    [Header("File Window 연결")]
    public FileWindow fileWindow; // 폴더 이상 여부 정보를 가져오기 위한 참조

    // 부위명 → Folder 경로 매핑
    private Dictionary<string, string[]> partNameToFolderPath = new Dictionary<string, string[]>
    {
        { "Head", new string[] { "Head" } },
        { "Chest", new string[] { "Body", "Chest" } },
        { "LeftUpperArm", new string[] { "LeftArm", "UpperArm" } },
        { "LeftForeArm", new string[] { "LeftArm", "ForeArm" } },
        { "LeftHand", new string[]  { "LeftArm", "Hand" } },
        { "RightUpperArm", new string[] { "RightArm", "UpperAarm" } },
        { "RightForeArm", new string[] { "RightArm", "ForeArm" } },
        { "RightHand", new string[] { "RightArm", "Hand" } },
        { "Stomach", new string[] { "Organ", "Stomach" } },
        { "Pelvis", new string[] { "Body", "Pelvis" } },
        { "LeftThigh", new string[] { "LeftLeg", "Thigh" } },
        { "LeftCalf", new string[] { "LeftLeg", "Calf" } },
        { "LeftFoot", new string[] { "LeftLeg", "Foot" } },
        { "RightThigh", new string[] { "RightLeg", "Thigh" } },
        { "RightCalf", new string[] { "RightLeg", "Calf" } },
        { "RightFoot", new string[] { "RightLeg", "Foot" } }
    };

    void Start()
    {
        // 각 버튼 클릭 이벤트 등록
        foreach (var part in bodyParts)
        {
            string partName = part.partName; // 지역 변수 복사 (람다 캡처 방지)
            if (part.button != null)
            {
                part.button.onClick.AddListener(() => ShowPart(partName));
            }
        }
    }

    /// <summary>
    /// 버튼 클릭 시 해당 부위의 스프라이트 표시
    /// </summary>
    public void ShowPart(string partName)
    {
        if (cameraPanel == null)
        {
            Debug.LogWarning("cameraPanel이 설정되지 않았습니다.");
            return;
        }

        if (fileWindow == null)
        {
            Debug.LogWarning("fileWindow가 연결되지 않았습니다.");
            return;
        }

        // 부위명 → Folder 경로 매핑 확인
        if (!partNameToFolderPath.TryGetValue(partName, out var path))
        {
            Debug.LogWarning("부위 경로를 찾을 수 없습니다: " + partName);
            return;
        }

        // 루트 폴더에서 경로 따라 Folder 탐색
        Folder targetFolder = fileWindow.GetRootFolder();
        foreach (var node in path)
        {
            targetFolder = targetFolder.children.Find(f => f.name == node);
            if (targetFolder == null)
            {
                Debug.LogWarning("폴더 경로 중 일부를 찾을 수 없습니다: " + string.Join("/", path));
                return;
            }
        }

        // BodyPartViewer의 bodyParts 목록에서 대응되는 BodyPart 찾기
        var partObj = System.Array.Find(bodyParts, p => p.partName == partName);
        if (partObj == null) return;

        // Folder 이상 여부 기준
        bool shouldShowAbnormal = targetFolder.isAbnormal || HasAbnormalInChildren(targetFolder);

        // 스프라이트 갱신
        cameraPanel.sprite = (shouldShowAbnormal && partObj.abnormalSprite != null) ? partObj.abnormalSprite : partObj.normalSprite;
    }

    /// <summary>
    /// 자식 폴더 + 파일 이상 여부 확인 (재귀)
    /// </summary>
    private bool HasAbnormalInChildren(Folder folder)
    {
        if (folder == null) return false;

        foreach (var file in folder.files)
        {
            if (file.isAbnormal) return true;
        }

        foreach (var child in folder.children)
        {
            if (child.isAbnormal || HasAbnormalInChildren(child)) return true;
        }

        return false;
    }
}
