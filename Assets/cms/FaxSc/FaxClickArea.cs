using UnityEngine;

public class FaxClickArea : MonoBehaviour
{
    public FaxViewer viewer;        // 팝업 담당
    public Camera targetCamera;     // Raycast 카메라

    void Update()
    {
        if (targetCamera == null || viewer == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                {
                    viewer.TriggerExpand();
                }
            }
        }
    }
}
