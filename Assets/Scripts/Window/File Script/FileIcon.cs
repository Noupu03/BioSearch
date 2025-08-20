using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class FileIcon : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMP_Text fileNameText;

    private string fileName;
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    private FileWindow fileWindow; // ���� FileWindow ����

    private Color normalColor = Color.white;   // �⺻ ��
    private Color selectedColor = Color.yellow; // ���� �� ��

    public void Setup(string name, FileWindow window)
    {
        fileName = name;
        fileWindow = window;
        fileNameText.text = name;
        iconImage.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // ����Ŭ�� �Ǻ�
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            OnDoubleClick();
        }
        else
        {
            OnSingleClick();
        }
        lastClickTime = Time.time;
    }

    private void OnSingleClick()
    {
        // ���� ���� ����
        if (fileWindow.selectedIcon != null && fileWindow.selectedIcon != this)
        {
            fileWindow.selectedIcon.iconImage.color = fileWindow.selectedIcon.normalColor;
            fileWindow.selectedIcon.fileNameText.color = fileWindow.selectedIcon.normalColor;
        }

        // ���� ���� ����
        iconImage.color = selectedColor;
        fileNameText.color = selectedColor;

        // FileWindow�� ���� ������ ���
        fileWindow.selectedIcon = this;

        Debug.Log("���� ����: " + fileName);
    }

    private void OnDoubleClick()
    {
        Debug.Log("���� ����: " + fileName);
        // ���� â ���� ��� �߰� ����
    }
}
