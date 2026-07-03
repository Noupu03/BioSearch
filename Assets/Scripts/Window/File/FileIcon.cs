using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Haare.Client.Routine;

/// <summary>
/// ���յ� ���� ������
/// - Ȯ���ں� ������ �̹����� ExtensionManager���� ������
/// - ����Ŭ�� �� PopupManager ���� ���� ����
/// - �巡�� �� ��� ����
/// - isAbnormal ���ο� ���� �ؽ�Ʈ ���� ����
/// </summary>
public class FileIcon : MonoRoutine, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    public Image iconImage;          // ������ �̹���
    public TMP_Text fileNameText;    // ���� �̸� ǥ��

    private FileWindow fileWindow;
    private File file;

    // �⺻ ����
    private Color normalColor = Color.white;   // ���� ����: �Ͼ�
    private Color abnormalColor = Color.red;   // �̻� ����: ����
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

        //  ���� �̻� ���� �ݿ�
        if (fileNameText != null)
            fileNameText.color = file.isAbnormal ? abnormalColor : normalColor;

        SetSelected(false);
    }

    public File GetFile() => file;

    public void SetSelected(bool selected)
    {
        if (fileNameText == null) return;

        if (selected)
        {
            fileNameText.color = selectedColor;
        }
        else
        {
            //  ���� ���� �� �ٽ� �̻� ���� �ݿ�
            fileNameText.color = file.isAbnormal ? abnormalColor : normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        fileWindow.SetSelectedFileIcon(this);

        if (eventData.clickCount == 2)
        {
            // ����Ŭ�� �� PopupManager ���� ���� ����
            if (FilePopupManager.Instance != null && file != null)
                FilePopupManager.Instance.OpenFile(file);
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
