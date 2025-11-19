using UnityEngine;

public class FaxViewer : MonoBehaviour
{
    public Camera viewCamera;
    public Transform faxVisual;
    public float minimizedY = 0.05f;
    public float expandedY = 0.45f;
    public float distance = 1.5f;
    public float moveSpeed = 8f;

    private bool isExpanded = false;
    private float currentY;

    void Start()
    {
        currentY = minimizedY;

        if (viewCamera != null)
            UpdateFaxPosition(true);
    }

    void Update()
    {
        if (viewCamera == null) return;

        float targetY = isExpanded ? expandedY : minimizedY;
        currentY = Mathf.Lerp(currentY, targetY, Time.deltaTime * moveSpeed);

        UpdateFaxPosition(false);
    }

   
    public void TriggerExpand()
    {
        isExpanded = !isExpanded;
    }

    void UpdateFaxPosition(bool instant)
    {
        Vector3 vp = new Vector3(0.5f, currentY, distance);
        Vector3 worldPos = viewCamera.ViewportToWorldPoint(vp);

        faxVisual.position = worldPos;
        faxVisual.rotation = Quaternion.LookRotation(viewCamera.transform.forward);
    }

    public bool IsExpanded()
    {
        return isExpanded;
    }
}
