using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FolderIcon : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    public TMP_Text fileNameText;

    private FileWindow fileWindow;
    private Folder folder;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow;

    public void Setup(Folder folder, FileWindow window, bool parentAbnormal = false)
    {
        this.folder = folder;
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
    public void OnDrop(PointerEventData eventData)
    {
        FolderIcon dragged = eventData.pointerDrag?.GetComponent<FolderIcon>();
        if (dragged == null) return;

        // 1) FileDragManager�� OnEndDrag�� EventSystem �帧���� ���� ȣ��
        if (FolderDragManager.Instance != null)
        {
            ExecuteEvents.Execute(FolderDragManager.Instance.gameObject, eventData, ExecuteEvents.endDragHandler);
        }

        // 2) ���� �̵� ����
        Folder source = dragged.GetFolder();
        Folder target = folder;

        // ���� ���� Ȯ��
        string warning;
        if (!FolderDepthUtility.CanMove(source, target, out warning))
        {
            LogWindowManager.Instance.Log(warning);
            return;
        }

        if (source.parent != null)
            source.parent.children.Remove(source);

        target.children.Add(source);
        source.parent = target;

        // 3) UI ������ ���� ���������� ���� �̺�Ʈ �ý����� ������ �ð� Ȯ��
        fileWindow.StartCoroutine(OpenFolderNextFrame(target));
    }


    private System.Collections.IEnumerator OpenFolderNextFrame(Folder target)
    {
        yield return null; // �� ������ ���
        fileWindow.OpenFolder(target, false);
    }

}
