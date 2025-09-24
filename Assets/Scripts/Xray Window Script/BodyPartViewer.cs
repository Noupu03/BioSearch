using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BodyPart
{
    public string partName;   // ���� �̸� (��: "head")
    public Button button;     // �ش� ��ư
    public Sprite sprite;     // ������ ��������Ʈ
}

public class BodyPartViewer : MonoBehaviour
{
    [Header("���� ī�޶�â")]
    public Image cameraPanel;  // Image ������Ʈ

    [Header("���� & ��ư ����")]
    public BodyPart[] bodyParts;  // Inspector���� ��ư�� ��������Ʈ�� �� ������ ���

    void Start()
    {
        // �� ��ư�� Ŭ�� �̺�Ʈ ����
        foreach (var part in bodyParts)
        {
            string partName = part.partName;  // ���� ������ ���� (���ٿ��� �����ϰ� ���)
            if (part.button != null)
            {
                part.button.onClick.AddListener(() => ShowPart(partName));
            }
        }
    }

    // ��ư Ŭ�� �� ȣ��
    public void ShowPart(string partName)
    {
        foreach (var part in bodyParts)
        {
            if (part.partName == partName)
            {
                if (cameraPanel != null)
                    cameraPanel.sprite = part.sprite;
                return;
            }
        }
        Debug.LogWarning("Unknown part: " + partName);
    }
}
