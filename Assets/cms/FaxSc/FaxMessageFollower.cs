using UnityEngine;

public class FaxMessageFollower : MonoBehaviour
{
    public Transform target;                  // 따라갈 오브젝트 (팩스)
    public Camera targetCamera;               // RoomCamera 자동 매핑

    public string roomCameraName = "Room Camera"; // 씬에서 카메라 이름
    public Vector3 screenOffset = new Vector3(0, 60, 0);

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        //  카메라가 비어있을 때만 자동 매핑 실행
        if (targetCamera == null)
        {
            // 1) 동일 씬에서 이름으로 찾기
            GameObject camObj = GameObject.Find(roomCameraName);

            if (camObj != null && camObj.GetComponent<Camera>() != null)
            {
                targetCamera = camObj.GetComponent<Camera>();
                Debug.Log($"[FaxFollower] RoomCamera 자동 연결됨: {targetCamera.name}");
            }
            else
            {
                Debug.LogWarning("[FaxFollower] RoomCamera를 자동으로 찾지 못함. Inspector에서 연결 필요.");
            }
        }
    }

    void LateUpdate()
    {
        if (target == null || targetCamera == null) return;

        Vector3 screenPos = targetCamera.WorldToScreenPoint(target.position);

        if (screenPos.z < 0)
        {
            rect.gameObject.SetActive(false);
            return;
        }

        rect.gameObject.SetActive(true);
        rect.position = screenPos + screenOffset;
    }
}
