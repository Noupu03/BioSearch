using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 파일 탐색기의 폴더 데이터 모델.
/// children/files 컬렉션은 읽기 전용으로만 노출되며,
/// 변경은 반드시 Add/Remove 메서드를 통해 수행해야 한다.
/// </summary>
public class Folder
{
    public string name;
    public Folder parent;

    public bool  isAbnormal      = false;
    public float abnormalParameter = 0f;

    // 인스펙터 연동 버튼 (FileWindow.LinkBodyButtons에서만 설정)
    internal Button linkedBodyButton;

    private readonly List<Folder> _children = new List<Folder>();
    private readonly List<File>   _files    = new List<File>();

    /// <summary>하위 폴더 읽기 전용 뷰</summary>
    public IReadOnlyList<Folder> children => _children;
    /// <summary>포함된 파일 읽기 전용 뷰</summary>
    public IReadOnlyList<File>   files    => _files;

    public Folder(string name, Folder parent = null)
    {
        this.name   = name;
        this.parent = parent;
        parent?._children.Add(this);
    }

    // ── 자식 폴더 조작 ──────────────────────────
    public void AddChild(Folder child)
    {
        if (child == null || _children.Contains(child)) return;
        _children.Add(child);
        child.parent = this;
    }

    public void RemoveChild(Folder child)
    {
        if (child == null || !_children.Contains(child)) return;
        _children.Remove(child);
        child.parent = null;
    }

    // ── 파일 조작 ────────────────────────────────
    public void AddFile(File file)
    {
        if (file == null || _files.Contains(file)) return;
        _files.Add(file);
        file.parent = this;
    }

    public void RemoveFile(File file)
    {
        if (file == null || !_files.Contains(file)) return;
        _files.Remove(file);
        file.parent = null;
    }

    // ── 이상 감지 ────────────────────────────────
    public void AssignAbnormalByParameter()
    {
        isAbnormal = Random.value < abnormalParameter;

        foreach (var child in _children)
            child.AssignAbnormalByParameter();

        UpdateLinkedButtonColor();
        UpdateParentButtonColor();
    }

    public void UpdateLinkedButtonColor()
    {
        if (linkedBodyButton == null) return;

        Color c = (isAbnormal || HasAbnormalInChildren()) ? Color.red : Color.white;
        var cb       = linkedBodyButton.colors;
        cb.normalColor = cb.highlightedColor = cb.pressedColor = cb.selectedColor = c;
        linkedBodyButton.colors = cb;
    }

    public bool HasAbnormalInChildren()
    {
        foreach (var f in _files)
            if (f.isAbnormal) return true;

        foreach (var child in _children)
            if (child.isAbnormal || child.HasAbnormalInChildren()) return true;

        return false;
    }

    private void UpdateParentButtonColor()
    {
        parent?.UpdateLinkedButtonColor();
        parent?.UpdateParentButtonColor();
    }
}
