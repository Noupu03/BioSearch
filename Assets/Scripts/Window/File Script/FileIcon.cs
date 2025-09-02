using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class FileIcon : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text fileNameText;

    private FileWindow fileWindow;
    private Folder folder;
    private float lastClickTime;
    private float doubleClickThreshold = 0.3f;

    private GlobalColorManager gcm;

    private bool isSelected = false;

    public void Setup(Folder folder, FileWindow window)
    {
        this.folder = folder;
        this.fileWindow = window;
        this.gcm = FindObjectOfType<GlobalColorManager>();

        fileNameText.text = folder.name;
        ApplyFolderColor();
    }

    public Folder GetFolder() => folder;

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        ApplyFolderColor();
    }

    private void ApplyFolderColor()
    {
        if (folder == null || fileNameText == null || gcm == null) return;

        if (folder.isAbnormal)
        {
            // �̻� ������ ������ ������
            fileNameText.color = gcm.abnormalFolderTextColor;
        }
        else if (isSelected)
        {
            // ���� ���� ���� (���ϸ� GlobalColorManager���� ������ ���� ����)
            fileNameText.color = gcm.fileTextColor; // ���� ������ ���ϸ� ���� �߰� ����
        }
        else
        {
            // �Ϲ� ���� ����
            fileNameText.color = gcm.fileTextColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        fileWindow.SetSelectedIcon(this);

        if (Time.time - lastClickTime < doubleClickThreshold)
            fileWindow.OpenSelected();

        lastClickTime = Time.time;
    }
}
