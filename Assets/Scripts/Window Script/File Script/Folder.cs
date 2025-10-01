using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Folder
{
    public string name;
    public List<Folder> children = new List<Folder>();
    public List<File> files = new List<File>();
    public Folder parent;

    public bool isAbnormal = false;
    public float abnormalParameter = 0f;

    // 연결된 신체 버튼
    public Button linkedBodyButton;

    public Folder(string name, Folder parent = null, Button linkedButton = null)
    {
        this.name = name;
        this.parent = parent;
        this.linkedBodyButton = linkedButton;
    }

    // 확률에 따라 이상 여부 결정 (자식 폴더 포함)
    public void AssignAbnormalByParameter()
    {
        isAbnormal = Random.value < abnormalParameter;

        // 자식 폴더도 재귀적으로 이상 여부 결정
        foreach (var child in children)
        {
            child.AssignAbnormalByParameter();
        }

        // 버튼 색상 갱신 (자신과 부모까지)
        UpdateLinkedButtonColor();
        UpdateParentButtonColor();
    }

    // 연결된 버튼 색상 업데이트 (자식 폴더까지 포함)
    public void UpdateLinkedButtonColor()
    {
        if (linkedBodyButton != null)
        {
            var colors = linkedBodyButton.colors;

            // 자신 또는 자식 중 하나라도 이상이면 빨간색
            bool shouldBeRed = isAbnormal || HasAbnormalInChildren();
            Color targetColor = shouldBeRed ? Color.red : Color.white;

            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.pressedColor = targetColor;
            colors.selectedColor = targetColor;

            linkedBodyButton.colors = colors;
        }
    }

    // 부모 버튼 색상도 갱신
    private void UpdateParentButtonColor()
    {
        parent?.UpdateLinkedButtonColor();
        parent?.UpdateParentButtonColor();
    }

    // 재귀적으로 자식 폴더 이상 여부 확인
    private bool HasAbnormalInChildren()
    {
        foreach (var child in children)
        {
            if (child.isAbnormal || child.HasAbnormalInChildren())
                return true;
        }
        return false;
    }
}
