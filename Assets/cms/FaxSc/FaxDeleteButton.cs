using UnityEngine;
using System.Collections;

public class FaxDeleteButton : MonoBehaviour
{
    public Camera targetCamera;           // Spawner에서 자동 주입
    public FaxViewer viewer;             // 자동 할당
    public GameObject faxObject;         // 자동 할당

    public float shrinkDuration = 0.4f;
    public float destroyDelay = 0.5f;

    private bool isDeleting = false;
    private Renderer[] renderers;

    void Start()
    {
        if (faxObject == null)
            faxObject = transform.root.gameObject;

        if (viewer == null)
            viewer = faxObject.GetComponent<FaxViewer>();

        renderers = faxObject.GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (isDeleting) return;
        if (targetCamera == null) return;
        if (!viewer.IsExpanded()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                {
                    StartCoroutine(DeleteRoutine());
                }
            }
        }
    }

    IEnumerator DeleteRoutine()
    {
        isDeleting = true;

        Vector3 startScale = faxObject.transform.localScale;
        float t = 0;
        Material[] mats = GetAllMaterials();

        while (t < 1f)
        {
            t += Time.deltaTime / shrinkDuration;
            float alpha = 1f - t;

            faxObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            SetAlpha(mats, alpha);

            yield return null;
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(faxObject);
    }

    Material[] GetAllMaterials()
    {
        var list = new System.Collections.Generic.List<Material>();
        foreach (Renderer r in renderers)
            list.AddRange(r.materials);
        return list.ToArray();
    }

    void SetAlpha(Material[] mats, float alpha)
    {
        foreach (Material m in mats)
        {
            if (m.HasProperty("_Color"))
            {
                Color c = m.color;
                c.a = alpha;
                m.color = c;
            }
        }
    }
}
