using UnityEngine;
using System.Collections;

public class FaxDeleteButton : MonoBehaviour
{
    [Tooltip("삭제할 팩스 오브젝트 (보통 부모 오브젝트)")]
    public GameObject faxObject;

    [Tooltip("클릭 감지 카메라 (RoomCamera, FaxCamera 등)")]
    public Camera targetCamera;

    [Header("삭제 애니메이션 설정")]
    public float shrinkDuration = 0.4f;  // 축소 속도
    public float destroyDelay = 0.5f;    // 완전 삭제 딜레이

    [Header("연결된 팩스 상태 스크립트")]
    public FaxViewer faxViewer;          // 팩스의 열림 상태를 확인하기 위함

    private bool isDeleting = false;
    private Renderer[] faxRenderers;

    void Start()
    {
        if (faxObject == null)
            faxObject = transform.parent.gameObject;

        if (faxViewer == null)
            faxViewer = faxObject.GetComponent<FaxViewer>();

        if (targetCamera == null)
            Debug.LogWarning("[FaxDeleteButton] targetCamera가 설정되어 있지 않습니다! 인스펙터에서 지정하세요.");

        faxRenderers = faxObject.GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        // 팩스가 팝업되지 않은 상태라면 클릭 차단
        if (faxViewer != null && !faxViewer.IsExpanded())
            return;

        if (Input.GetMouseButtonDown(0) && targetCamera != null && !isDeleting)
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    Debug.Log("[FAX] 팩스 열림 상태에서 삭제 버튼 클릭됨 → 삭제 애니메이션 시작");
                    StartCoroutine(DeleteFaxRoutine());
                }
            }
        }
    }

    IEnumerator DeleteFaxRoutine()
    {
        isDeleting = true;

        float t = 0f;
        Vector3 startScale = faxObject.transform.localScale;
        Material[] mats = GetAllMaterials();

        while (t < 1f)
        {
            t += Time.deltaTime / shrinkDuration;
            faxObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            float alpha = Mathf.Lerp(1f, 0f, t);
            SetAlpha(mats, alpha);

            yield return null;
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(faxObject);
    }

    Material[] GetAllMaterials()
    {
        var list = new System.Collections.Generic.List<Material>();
        foreach (Renderer r in faxRenderers)
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
