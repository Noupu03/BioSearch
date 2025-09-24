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
            int index = i; // Ŭ���� ���� ����
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
        File draggedFile = null;

        // 1. FolderDragManager���� ���� �巡�� ���� Folder Ȯ��
        if (FolderDragManager.Instance.CurrentDraggedFolderIcon != null)
        {
            draggedFolder = FolderDragManager.Instance.CurrentDraggedFolderIcon.GetFolder();
        }
        // 2. pointerDrag�� FolderIcon�̸� ��������
        else if (eventData.pointerDrag != null)
        {
            FolderIcon folderIcon = eventData.pointerDrag.GetComponent<FolderIcon>();
            if (folderIcon != null)
                draggedFolder = folderIcon.GetFolder();
            else
            {
                // pointerDrag�� FileIcon�̸� ��������
                FileIcon fileIcon = eventData.pointerDrag.GetComponent<FileIcon>();
                if (fileIcon != null)
                    draggedFile = fileIcon.GetFile();
            }
        }

        List<Folder> pathList = fileWindow.GetCurrentPathList();
        if (index < 0 || index >= pathList.Count) return;

        Folder targetFolder = pathList[index];

        // ���� �̵� ó��
        if (draggedFolder != null)
        {
            // �ڱ� �ڽ��̳� ���� ������ ��� ����
            Folder temp = targetFolder;
            while (temp != null)
            {
                if (temp == draggedFolder)
                {
                    LogWindowManager.Instance.Log("�ڽ� �Ǵ� ���� �������� ����� �� �����ϴ�.");
                    return;
                }
                temp = temp.parent;
            }

            string warning;
            if (!FolderDepthUtility.CanMove(draggedFolder, targetFolder, out warning))
            {
                LogWindowManager.Instance.Log(warning);
                return;
            }

            // ���� �θ𿡼� ����
            if (draggedFolder.parent != null)
                draggedFolder.parent.children.Remove(draggedFolder);

            // �� �θ� �߰�
            targetFolder.children.Add(draggedFolder);
            draggedFolder.parent = targetFolder;

            FolderDragManager.Instance.EndDrag();
            LogWindowManager.Instance.Log($"���� '{draggedFolder.name}' �� '{targetFolder.name}' �̵���");
        }
        // ���� �̵� ó��
        else if (draggedFile != null)
        {
            if (draggedFile.parent != null)
                draggedFile.parent.files.Remove(draggedFile); // ���� �θ� �������� ����

            draggedFile.parent = targetFolder;
            targetFolder.files.Add(draggedFile);

            FolderDragManager.Instance.EndDrag();
            LogWindowManager.Instance.Log($"���� '{draggedFile.name}' �� '{targetFolder.name}' �̵���");
        }

        // UI ����
        fileWindow.StartCoroutine(OpenFolderNextFrame(targetFolder));
    }

    private IEnumerator OpenFolderNextFrame(Folder folder)
    {
        yield return null; // �� ������ ���
        fileWindow.OpenFolder(folder, false);
    }
}
