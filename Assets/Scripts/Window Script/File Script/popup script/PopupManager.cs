using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("Prefab & Canvas")]
    public GameObject popupPrefab;  // Popup ������
    public Canvas canvas;           // Inspector���� ���� ����

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenFile(File file)
    {
        if (canvas == null)
        {
            Debug.LogError("PopupManager: Canvas�� �������� �ʾҽ��ϴ�!");
            return;
        }

        GameObject popupInstance = Instantiate(popupPrefab, canvas.transform, false);
        Popup popupScript = popupInstance.GetComponent<Popup>();
        if (popupScript != null)
        {
            popupScript.Initialize(file.name, file.extension, canvas);

            // TopBar �巡�� �̺�Ʈ ����
            EventTrigger trigger = popupScript.topBar.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = popupScript.topBar.gameObject.AddComponent<EventTrigger>();

            // PointerDown
            EventTrigger.Entry entryDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entryDown.callback.AddListener((data) => popupScript.OnTopBarPointerDown(data));
            trigger.triggers.Add(entryDown);

            // Drag
            EventTrigger.Entry entryDrag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag
            };
            entryDrag.callback.AddListener((data) => popupScript.OnTopBarDrag(data));
            trigger.triggers.Add(entryDrag);
        }

        // ���� �� �ֻ������
        popupInstance.transform.SetAsLastSibling();

        // ���� Ȯ���ڿ� ���� ���� ǥ��
        Transform popupImage = FindDeepChild(popupInstance.transform, "PopupImage");
        Transform popupText = FindDeepChild(popupInstance.transform, "PopupText");

        if (popupImage == null || popupText == null)
        {
            Debug.LogError("PopupPrefab �ȿ� 'PopupImage' �Ǵ� 'PopupText' ������Ʈ�� �����ϴ�!");
            return;
        }

        switch (file.extension.ToLower())
        {
            case "png":
                popupImage.gameObject.SetActive(true);
                popupText.gameObject.SetActive(false);
                Image img = popupImage.GetComponent<Image>();
                if (img != null && file.imageContent != null)
                    img.sprite = file.imageContent;
                break;

            case "txt":
                popupImage.gameObject.SetActive(false);
                popupText.gameObject.SetActive(true);
                TMP_Text textComp = popupText.GetComponent<TMP_Text>();
                if (textComp != null)
                    textComp.text = file.textContent ?? $"{file.name}.{file.extension} (���� ����)";
                break;

            default:
                popupImage.gameObject.SetActive(false);
                popupText.gameObject.SetActive(false);
                break;
        }
    }

    // ������ ���� ��� Ž��
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
