using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 실제 폴더 데이터 구조.
/// 자식 폴더 보관, 이상 여부 표시.
/// </summary>

public class Folder
{
    public string name;
    public List<Folder> children = new List<Folder>();
    public List<File> files = new List<File>();   // 파일 리스트 추가
    public Folder parent;

    // 이상 폴더 여부
    public bool isImportant = false;

    // 이상 폴더 탐색용 파라미터 (0~1 범위, 외부에서 설정 가능)
    public float importantParameter = 0f;

    // 연결된 신체 버튼
    public Button linkedBodyButton;

    public Folder(string name, Folder parent = null, Button linkedButton = null)
    {
        this.name = name;
        this.parent = parent;
        this.linkedBodyButton = linkedButton;
    }

    
    // 확률에 따라 이상 여부 결정 (자식 폴더 포함)
    /*public void AssignAbnormalByParameter()
    {
        isImportant = Random.value < importantParameter;

        // 자식 폴더도 재귀적으로 이상 여부 결정
        foreach (var child in children)
        {
            child.AssignAbnormalByParameter();
        }

        // 버튼 색상 갱신 (자신과 부모까지)
        UpdateLinkedButtonColor();
        UpdateParentButtonColor();
    }*/
    // 연결된 버튼 색상 업데이트 (자식 폴더까지 포함)
    /*public void UpdateLinkedButtonColor()
    {
        if (linkedBodyButton != null)
        {
            var colors = linkedBodyButton.colors;

            // 자신 또는 자식 중 하나라도 이상이면 빨간색
            bool shouldBeRed = isImportant || HasAbnormalInChildren();
            Color targetColor = shouldBeRed ? Color.red : Color.white;

            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.pressedColor = targetColor;
            colors.selectedColor = targetColor;

            linkedBodyButton.colors = colors;
        }
    }*/

    // 부모 버튼 색상도 갱신
    /*private void UpdateParentButtonColor()
    {
        parent?.UpdateLinkedButtonColor();
        parent?.UpdateParentButtonColor();
    }*/

    // 재귀적으로 자식 폴더 이상 여부 확인
    // 재귀적으로 자식 폴더 + 파일 이상 여부 확인
    private bool HasAbnormalInChildren()
    {
        // 1) 파일 먼저 검사
        foreach (var file in files)
        {
            if (file.isImportant)    // 파일에 이상이 하나라도 있으면 이상 처리
                return true;
        }

        // 2) 자식 폴더 검사
        foreach (var child in children)
        {
            if (child.isImportant || child.HasAbnormalInChildren())
                return true;
        }

        return false;
    }



    // 자식 추가
    public void AddChild(Folder child)
    {
        if (!children.Contains(child))
        {
            children.Add(child);
            child.parent = this;
        }
    }

    // 자식 제거
    public void RemoveChild(Folder child)
    {
        if (children.Contains(child))
        {
            children.Remove(child);
            child.parent = null;
        }
    }

    // 파일 추가
    public void AddFile(File file)
    {
        if (!files.Contains(file))
        {
            files.Add(file);
            file.parent = this;
        }
    }

    // 파일 제거
    public void RemoveFile(File file)
    {
        if (files.Contains(file))
        {
            files.Remove(file);
            file.parent = null;
        }
    }
    public List<Exe> exes = new List<Exe>();
}
