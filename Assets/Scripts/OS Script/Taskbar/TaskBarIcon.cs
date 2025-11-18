using UnityEngine;
using UnityEngine.UI;

public class TaskbarIcon : MonoBehaviour
{
    public GameObject linkedInstance;
    private Button btn;

    public void Setup(GameObject instance)
    {
        linkedInstance = instance;

        if (btn == null)
            btn = GetComponent<Button>();
        if (btn == null)
            btn = gameObject.AddComponent<Button>();

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (linkedInstance != null)
        {
            linkedInstance.SetActive(true);
            linkedInstance.transform.SetAsLastSibling();
            Debug.Log("[TaskbarIcon] Activated " + linkedInstance.name);
        }
        else
        {
            Debug.LogWarning("[TaskbarIcon] linkedInstance is null!");
        }
    }
}
