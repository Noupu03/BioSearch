using UnityEngine;
using UnityEngine.EventSystems;

public class FaxViewer : MonoBehaviour
{
    public Camera viewCamera;

    [Header("팽스 오브젝트")]
    public Transform faxVisual;
    public BoxCollider bigCollider;
    public BoxCollider smallCollider;

    [Header("페이퍼 이동 위치 (Viewport 기준)")]
    public float minimizedY = 0.05f;
    public float expandedY = 0.45f;
    public float viewX = 0.5f;
    public float distanceFromCamera = 1.5f;

    [Header("스피드")]
    public float slideSpeed = 10f;

    private bool isExpanded = false;
    private Vector3 targetPos;

    void Start()
    {
        if (viewCamera == null)
            viewCamera = Camera.main;

        SetColliderState(false);
        SetTarget(minimizedY, true);
    }

    void Update()
    {
        HandleInput();

        float targetY = isExpanded ? expandedY : minimizedY;
        SetTarget(targetY, false);

        transform.position = Vector3.Lerp(
            transform.position, targetPos, Time.deltaTime * slideSpeed
        );

        //  콜라이더도 오브젝트에 따라 같이 이동 적용
        RepositionCollider(bigCollider);
        RepositionCollider(smallCollider);
    }

    void HandleInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        //  UI 클릭 → 무조건 접기
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            isExpanded = false;
            return;
        }

        //  레이캐스트 판단
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            if (!isExpanded && hit.collider == smallCollider)
                isExpanded = true;
            else if (isExpanded && hit.collider == bigCollider)
                isExpanded = true;
            else
                isExpanded = false;
        }
        else
        {
            isExpanded = false;
        }

        SetColliderState(isExpanded);
    }

    //  각 Collider도 뷰포트 기반으로 위치 보정
    void RepositionCollider(BoxCollider col)
    {
        col.transform.position = faxVisual.position;
        col.transform.rotation = faxVisual.rotation;
    }

    void SetColliderState(bool expanded)
    {
        bigCollider.enabled = expanded;
        smallCollider.enabled = !expanded;
    }

    void SetTarget(float vpY, bool instant)
    {
        Vector3 viewPos = new Vector3(viewX, vpY, distanceFromCamera);
        targetPos = viewCamera.ViewportToWorldPoint(viewPos);

        if (instant)
            transform.position = targetPos;
    }
}
