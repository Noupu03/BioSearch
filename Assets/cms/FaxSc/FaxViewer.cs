using UnityEngine;

public class FaxViewer : MonoBehaviour
{
    public Camera viewCamera;
    public Transform faxVisual;
    public float minimizedY = 0.05f;
    public float expandedY = 0.45f;
    public float distance = 1.5f;
    public float moveSpeed = 8f;

    private bool isExpanded;
    private float currentY;

    void Start()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        currentY = minimizedY;
        UpdateFaxPosition(true);
    }

    void Update()
    {
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

        if (instant)
            faxVisual.position = worldPos;
    }
}
