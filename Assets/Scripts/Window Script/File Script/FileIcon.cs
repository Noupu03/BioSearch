using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ���� �������� ���� ���� (�� Ŭ������ ���� ������� �ʰ� ����ؼ� TxtIcon, PngIcon�� ����)
/// </summary>
public abstract class FileIcon : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text fileNameText;

    protected FileWindow fileWindow;
    protected File file;

    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow;

    public virtual void Setup(File file, FileWindow window)
    {
        this.file = file;
        this.fileWindow = window;

        if (fileNameText != null)
            fileNameText.text = $"{file.name}.{file.extension}";

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
            OnDoubleClick();
    }

    // ���� Ŭ�� �� ���� (Ȯ���ں��� ����)
    protected abstract void OnDoubleClick();
}
