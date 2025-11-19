using UnityEngine;
using System.Collections;

public class FaxDeleteButton : MonoBehaviour
{
    public Camera targetCamera;
    public FaxViewer viewer;
    public GameObject faxObject;

    public float shrinkDuration = 0.4f;
    public float destroyDelay = 0.4f;

    private bool isDeleting = false;

    void Start()
    {
        if (viewer == null)
            viewer = GetComponentInParent<FaxViewer>();

        if (faxObject == null)
            faxObject = viewer.gameObject;
    }

    void Update()
    {
        if (isDeleting) return;
        if (targetCamera == null) return;
        if (!viewer.IsExpanded()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 20))
            {
                if (hit.collider.gameObject == this.gameObject)
                    StartCoroutine(DeleteRoutine());
            }
        }
    }

    IEnumerator DeleteRoutine()
    {
        isDeleting = true;

        float t = 0;
        Vector3 startScale = faxObject.transform.localScale;

        while (t < 1)
        {
            t += Time.deltaTime / shrinkDuration;
            faxObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(faxObject);
    }
}
