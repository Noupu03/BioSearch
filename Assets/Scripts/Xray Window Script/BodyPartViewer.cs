using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class BodyPart
{
    public string partName;       // 부위 이름 (예: "Head")
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

        // FileWindow 내부의 폴더 구조에서 해당 부위 폴더 찾기
        Folder targetFolder = FindFolderByName(fileWindow.GetRootFolder(), partName);

        if (targetFolder == null)
        {
            Debug.LogWarning("해당 부위 폴더를 찾을 수 없습니다: " + partName);
            return;
        }

        // BodyPartViewer의 bodyParts 목록에서 대응되는 BodyPart 찾기
        foreach (var part in bodyParts)
        {
            if (part.partName == partName)
            {
                // Folder.cs의 버튼 색상 조건과 동일한 기준 적용
                bool shouldShowAbnormal = targetFolder.isAbnormal || HasAbnormalInChildren(targetFolder);

                if (shouldShowAbnormal && part.abnormalSprite != null)
                {
                    cameraPanel.sprite = part.abnormalSprite;
                }
                else
                {
                    cameraPanel.sprite = part.normalSprite;
                }
                return;
            }
        }

        Debug.LogWarning("Unknown part: " + partName);
    }

    /// <summary>
    /// 폴더 이름으로 Folder 검색 (재귀)
    /// </summary>
    private Folder FindFolderByName(Folder folder, string name)
    {
        if (folder == null) return null;
        if (folder.name == name) return folder;

        foreach (var child in folder.children)
        {
            Folder found = FindFolderByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    /// <summary>
    /// Folder.cs의 HasAbnormalInChildren과 동일한 기준으로
    /// 자식 폴더 + 파일 이상 여부 확인
    /// </summary>
    private bool HasAbnormalInChildren(Folder folder)
    {
        if (folder == null) return false;

        // 파일 이상 여부 먼저 검사
        foreach (var file in folder.files)
        {
            if (file.isAbnormal)
                return true;
        }

        // 자식 폴더 재귀 검사
        foreach (var child in folder.children)
        {
            if (child.isAbnormal || HasAbnormalInChildren(child))
                return true;
        }

        return false;
    }

}
