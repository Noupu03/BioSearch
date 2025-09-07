using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PathPanelManager : MonoBehaviour
{
    public Transform contentArea;
    public Button pathButtonPrefab;

    private FileWindow fileWindow;
    private List<Button> pathButtons = new List<Button>();

    public void Initialize(FileWindow window)
    {
        fileWindow = window;
    }

    public void UpdatePathButtons()
    {
        // ���� ��ư ����
        foreach (var btn in pathButtons)
            Destroy(btn.gameObject);
        pathButtons.Clear();

        List<Folder> pathList = fileWindow.GetCurrentPathList();

        for (int i = 0; i < pathList.Count; i++)
        {
            int index = i;
            Button btn = Instantiate(pathButtonPrefab, contentArea);
            TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
            text.text = pathList[i].name;

            float width = text.preferredWidth + 20f;
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(width, btn.GetComponent<RectTransform>().sizeDelta.y);

            // Ŭ�� �� �̵�
            btn.onClick.AddListener(() =>
            {
                fileWindow.NavigateToPathIndex(index);
            });

            // Drop �̺�Ʈ ���
            EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.Drop };
            entry.callback.AddListener((data) =>
            {
                OnPathDrop(index, (PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            pathButtons.Add(btn);
        }
    }

    private void OnPathDrop(int index, PointerEventData eventData)
    {
        Folder draggedFolder = null;

        // 1. FileDragManager���� ���� �巡�� ���� Folder Ȯ��
        if (FileDragManager.Instance.CurrentDraggedFolder != null)
        {
            draggedFolder = FileDragManager.Instance.CurrentDraggedFolder;
        }
        // 2. pointerDrag�� FileIcon�̸� ��������
        else if (eventData.pointerDrag != null)
        {
            FileIcon icon = eventData.pointerDrag.GetComponent<FileIcon>();
            if (icon != null)
                draggedFolder = icon.GetFolder();
        }

        if (draggedFolder == null)
        {
            Debug.LogWarning("�巡�� ���� ������ ã�� �� �����ϴ�.");
            return;
        }

        List<Folder> pathList = fileWindow.GetCurrentPathList();
        if (index < 0 || index >= pathList.Count) return;

        Folder targetFolder = pathList[index];

        // �ڱ� �ڽ��̳� ���� ������ ��� ����
        Folder temp = targetFolder;
        while (temp != null)
        {
            if (temp == draggedFolder)
            {
                Debug.LogWarning("�ڽ� �Ǵ� ���� �������� ����� �� �����ϴ�.");
                return;
            }
            temp = temp.parent;
        }

        // ���� �θ𿡼� ����
        if (draggedFolder.parent != null)
            draggedFolder.parent.children.Remove(draggedFolder);

        // �� �θ� �߰�
        targetFolder.children.Add(draggedFolder);
        draggedFolder.parent = targetFolder;

        // Ghost ����
        FileDragManager.Instance.ForceEndDrag();

        // UI ����
        fileWindow.StartCoroutine(OpenFolderNextFrame(targetFolder));
    }

    private IEnumerator OpenFolderNextFrame(Folder folder)
    {
        yield return null; // �� ������ ���
        fileWindow.OpenFolder(folder, false);
    }
}
