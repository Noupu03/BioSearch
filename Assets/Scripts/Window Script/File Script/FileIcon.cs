using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ���յ� ���� ������
/// - Ȯ���ں� ������ �̹����� ExtensionManager���� ������
/// - ����Ŭ�� �� PopupManager ���� ���� ����
/// - �巡�� �� ��� ����
/// </summary>
public class FileIcon : MonoBehaviour, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    public Image iconImage;          // ������ �̹���
    public TMP_Text fileNameText;    // ���� �̸� ǥ��

    private FileWindow fileWindow;
    private File file;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow;

    /// <summary>
    /// ������ �ʱ�ȭ
    /// </summary>
    public void Setup(File fileData, FileWindow window)
    {
        file = fileData;
        fileWindow = window;

        if (fileNameText != null)
            fileNameText.text = $"{file.name}.{file.extension}";

        if (iconImage != null && ExtensionManager.Instance != null)
            iconImage.sprite = ExtensionManager.Instance.GetIconForExtension(file.extension);

        SetSelected(false);
    }

    public File GetFile() => file;

    public void SetSelected(bool selected)
    {
        if (fileNameText == null) return;
        fileNameText.color = selected ? selectedColor : normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        fileWindow.SetSelectedFileIcon(this);

        if (eventData.clickCount == 2)
        {
            // ����Ŭ�� �� PopupManager ���� ���� ����
            if (PopupManager.Instance != null && file != null)
                PopupManager.Instance.OpenFile(file);
        }
    }

    #region �巡�� ����

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (FolderDragManager.Instance != null)
            FolderDragManager.Instance.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (FolderDragManager.Instance != null)
            FolderDragManager.Instance.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (FolderDragManager.Instance != null)
            FolderDragManager.Instance.EndDrag();
    }

    #endregion
}
