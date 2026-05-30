using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// FolderIcon Ŭ����
/// - ���� �������� �ð��� ǥ��, ���� ����, �巡�� & ���, Ŭ�� �̺�Ʈ ���� ������
/// - FileWindow �� Folder ��ü�� �����Ǿ� �۵���
/// </summary>
public class FolderIcon : MonoBehaviour, IPointerClickHandler, IDropHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text fileNameText;

    // ������ ���� FileWindow ����
    private FileWindow fileWindow;

    // ���� ���� ������(Folder Ŭ����)
    private Folder folder;

    // ���õ��� �ʾ��� ���� ����
    private Color normalColor = Color.white;

    // ���õǾ��� ���� ����
    private Color selectedColor = Color.yellow;

    /// <summary>
    /// FolderIcon �ʱ� ����
    /// </summary>
    /// <param name="folder">������ Folder ��ü</param>
    /// <param name="window">������ FileWindow ��ü</param>
    /// <param name="parentAbnormal">���� ������ ������ �������� ����</param>
    public void Setup(Folder folder, FileWindow window, bool parentAbnormal = false)
    {
        this.folder = folder;
        this.fileWindow = window;

        // ���� ���� �Ǵ� �θ� ���� �� �ϳ��� ������(abnormal)�� ���
        bool isAbnormal = parentAbnormal || folder.isAbnormal;

        // ���� �̸� ǥ��
        if (fileNameText != null)
            fileNameText.text = folder.name;

        // �ʱ⿡�� ���õ��� ���� ����
        SetSelected(false);

        // ������ ������ ��� ��Ʈ ������ ���������� ǥ��
        if (fileNameText != null && isAbnormal)
            fileNameText.color = Color.red;
    }

    /// <summary>
    /// ���� �������� �����ϴ� Folder ��ü ��ȯ
    /// </summary>
    public Folder GetFolder() => folder;

    /// <summary>
    /// ���� ���¸� ǥ�� �������� �ݿ�
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (fileNameText == null) return;
        if (folder != null && folder.isAbnormal) return; // ������ ������ ���� ���� X
        fileNameText.color = selected ? selectedColor : normalColor;
    }

    /// <summary>
    /// ���� ������ Ŭ�� �� ����
    /// - �� �� Ŭ��: ����
    /// - �� �� Ŭ��: ���� ����
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        fileWindow.SetSelectedIcon(this); // ���� ó��

        // ���� Ŭ�� �� ���� ����
        if (eventData.clickCount == 2)
            fileWindow.OpenFolder(folder);
    }

    #region �巡�� ����

    /// <summary>
    /// �巡�� ���� �� ȣ��
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.BeginDrag(this, eventData);
    }

    /// <summary>
    /// �巡�� �� ���콺 �̵� ó��
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.OnDrag(eventData);
    }

    /// <summary>
    /// �巡�� ���� �� ó��
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        FolderDragManager.Instance.EndDrag();
    }

    #endregion

    /// <summary>
    /// ���� ���� �ٸ� �������� ��ӵ� �� ó��
    /// - ������ ����� ���: ���� �̵�
    /// - ������ ����� ���: ���� �̵�
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // -----------------------------
        // 1. ���� ��� ó��
        // -----------------------------
        FolderIcon draggedFolderIcon = eventData.pointerDrag?.GetComponent<FolderIcon>();
        if (draggedFolderIcon != null)
        {
            Folder source = draggedFolderIcon.GetFolder(); // �̵��Ǵ� ����
            Folder target = folder; // ��� ��� ����

            string warning;
            // ���� �̵� ���� ���� �˻� (��ȯ ���� �� ����)
            if (!FolderDepthUtility.CanMove(source, target, out warning))
            {
                LogWindowManager.Instance.Log(warning);
                return;
            }

            source.parent?.RemoveChild(source);
            target.AddChild(source);

            // �α� ���
            LogWindowManager.Instance.Log($"���� '{source.name}' �� '{target.name}' �̵���");

            // �巡�� ���� ���� �� UI ������Ʈ
            FolderDragManager.Instance.ForceEndDrag();
            fileWindow.StartCoroutine(OpenFolderNextFrame(target));
            return;
        }

        // -----------------------------
        // 2. ���� ��� ó��
        // -----------------------------
        FileIcon draggedFileIcon = eventData.pointerDrag?.GetComponent<FileIcon>();
        if (draggedFileIcon != null)
        {
            File file = draggedFileIcon.GetFile(); // �̵��Ǵ� ����
            Folder target = folder; // ��� ��� ����

            file.parent?.RemoveFile(file);
            target.AddFile(file);

            // �α� ���
            LogWindowManager.Instance.Log($"���� '{file.name}.{file.extension}' �� '{target.name}' �̵���");

            // �巡�� ���� ���� �� UI ����
            FolderDragManager.Instance.ForceEndDrag();
            fileWindow.RefreshWindow(); // ���� �̵� �� UI ����
        }
    }

    /// <summary>
    /// ���� �����ӿ� ������ ���� �ڷ�ƾ
    /// - ��� ���� �巡�� �̺�Ʈ ó���� �浹 ���ɼ��� ����
    /// </summary>
    private IEnumerator OpenFolderNextFrame(Folder target)
    {
        yield return null; // �� ������ ���
        fileWindow.OpenFolder(target, false);
    }
    // FolderIcon.cs
    public void SetupDummy(string dummyName)
    {
        if (fileNameText != null)
            fileNameText.text = dummyName;

        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = false;
    }


}
