using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Haare.Client.Routine;

public class FolderIcon : MonoRoutine, IPointerClickHandler, IDropHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text fileNameText;

    private FileWindow fileWindow;
    private Folder folder;

    private Color normalColor   = Color.white;
    private Color selectedColor = Color.yellow;

    public void Setup(Folder folder, FileWindow window, bool parentAbnormal = false)
    {
        this.folder     = folder;
        this.fileWindow = window;

        bool isAbnormal = parentAbnormal || folder.isAbnormal;

        if (fileNameText != null)
            fileNameText.text = folder.name;

        SetSelected(false);

        if (fileNameText != null && isAbnormal)
            fileNameText.color = Color.red;
    }

    public Folder GetFolder() => folder;

    public void SetSelected(bool selected)
    {
        if (fileNameText == null) return;
        if (folder != null && folder.isAbnormal) return;
        fileNameText.color = selected ? selectedColor : normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        fileWindow.SetSelectedIcon(this);

        if (eventData.clickCount == 2)
            fileWindow.OpenFolder(folder);
    }

    #region 드래그 처리

    public void OnBeginDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.EndDrag();
    }

    #endregion

    public void OnDrop(PointerEventData eventData)
    {
        // 폴더 드롭
        FolderIcon draggedFolderIcon = eventData.pointerDrag?.GetComponent<FolderIcon>();
        if (draggedFolderIcon != null)
        {
            Folder source = draggedFolderIcon.GetFolder();
            Folder target = folder;

            if (!FolderDepthUtility.CanMove(source, target, out string warning))
            {
                LogWindowManager.Instance.Log(warning);
                return;
            }

            source.parent?.RemoveChild(source);
            target.AddChild(source);

            LogWindowManager.Instance.Log($"'{source.name}' 폴더를 '{target.name}' 폴더로 이동");

            FolderDragManager.Instance.ForceEndDrag();
            fileWindow.StartCoroutine(OpenFolderNextFrame(target));
            return;
        }

        // 파일 드롭
        FileIcon draggedFileIcon = eventData.pointerDrag?.GetComponent<FileIcon>();
        if (draggedFileIcon != null)
        {
            File   file   = draggedFileIcon.GetFile();
            Folder target = folder;

            file.parent?.RemoveFile(file);
            target.AddFile(file);

            LogWindowManager.Instance.Log($"'{file.name}.{file.extension}' 파일을 '{target.name}' 폴더로 이동");

            FolderDragManager.Instance.ForceEndDrag();
            fileWindow.RefreshWindow();
        }
    }

    private IEnumerator OpenFolderNextFrame(Folder target)
    {
        yield return null;
        fileWindow.OpenFolder(target, false);
    }
}
