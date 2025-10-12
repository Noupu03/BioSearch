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
        // �̱��� ����
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Canvas �ڵ� Ž�� (Ȥ�� Inspector�� ���� �� ���� ��� ���)
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
    }

    public void OpenFile(File file)
    {
        if (canvas == null)
        {
            Debug.LogError("PopupManager: Canvas�� �������� �ʾҽ��ϴ�!");
            return;
        }

        if (popupPrefab == null)
        {
            Debug.LogError("PopupManager: popupPrefab�� �������� �ʾҽ��ϴ�!");
            return;
        }

        // �˾� ����
        GameObject popupInstance = Instantiate(popupPrefab, canvas.transform, false);
        if (popupInstance == null)
        {
            Debug.LogError("PopupManager: Popup ���� ����!");
            return;
        }

        // Popup ��ũ��Ʈ ��������
        Popup popupScript = popupInstance.GetComponent<Popup>();
        if (popupScript != null)
        {
            popupScript.Initialize(file.name, file.extension, canvas);

            // TopBar �巡�� �̺�Ʈ ����
            EventTrigger trigger = popupScript.topBar.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = popupScript.topBar.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear(); // ���� Ʈ���� �ʱ�ȭ (�ߺ� ����)

            // PointerDown
            EventTrigger.Entry entryDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entryDown.callback.AddListener((data) => popupScript.OnTopBarPointerDown((PointerEventData)data));
            trigger.triggers.Add(entryDown);

            // Drag
            EventTrigger.Entry entryDrag = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag
            };
            entryDrag.callback.AddListener((data) => popupScript.OnTopBarDrag((PointerEventData)data));
            trigger.triggers.Add(entryDrag);
        }
        else
        {
            Debug.LogError("PopupManager: Popup �����տ� Popup ��ũ��Ʈ�� �����ϴ�!");
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

        // Ȯ���ں� ǥ�� ����
        string ext = file.extension.ToLower();
        popupImage.gameObject.SetActive(false);
        popupText.gameObject.SetActive(false);

        switch (ext)
        {
            case "png":
            case "jpg":
            case "jpeg":
                popupImage.gameObject.SetActive(true);
                Image img = popupImage.GetComponent<Image>();
                if (img != null && file.imageContent != null)
                    img.sprite = file.imageContent;
                else
                    Debug.LogWarning($"{file.name}.{file.extension} : �̹��� ���� ����");
                break;

            case "txt":
                popupText.gameObject.SetActive(true);
                TMP_Text textComp = popupText.GetComponent<TMP_Text>();
                if (textComp != null)
                    textComp.text = file.textContent ?? $"{file.name}.{file.extension} (���� ����)";
                else
                    Debug.LogWarning($"{file.name}.{file.extension} : TMP_Text ������Ʈ ����");
                break;

            default:
                Debug.LogWarning($"{file.name}.{file.extension} : �������� �ʴ� ���� ����");
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
