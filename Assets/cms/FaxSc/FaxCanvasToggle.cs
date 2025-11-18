using UnityEngine;

public class FaxCanvasToggle : MonoBehaviour
{
    public CanvasGroup faxCanvasGroup;

    void Start()
    {
        if (faxCanvasGroup == null)
            faxCanvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // W 키 → Canvas 클릭 차단
        if (Input.GetKeyDown(KeyCode.W))
        {
            faxCanvasGroup.blocksRaycasts = false;
            faxCanvasGroup.interactable = false;
            Debug.Log("FAX Canvas → 비활성화 (클릭 불가능)");
        }

        // S 키 → Canvas 클릭 허용
        if (Input.GetKeyDown(KeyCode.S))
        {
            faxCanvasGroup.blocksRaycasts = true;
            faxCanvasGroup.interactable = true;
            Debug.Log("FAX Canvas → 활성화 (클릭 가능)");
        }
    }
}
