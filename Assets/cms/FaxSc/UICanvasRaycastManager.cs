using UnityEngine;
using UnityEngine.UI;

public class UICanvasRaycastManager : MonoBehaviour
{
    public Camera currentCamera;

    private GraphicRaycaster[] raycasters;

    void Awake()
    {
        // 씬 안의 모든 Canvas Raycaster 자동 수집
        raycasters = FindObjectsOfType<GraphicRaycaster>(true);
    }

    public void SetActiveCamera(Camera cam)
    {
        currentCamera = cam;

        foreach (var ray in raycasters)
        {
            Canvas canvas = ray.GetComponent<Canvas>();

            // World Space Canvas라면 camera가 같을 때만 활성화
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                ray.enabled = (canvas.worldCamera == currentCamera);
            }
            else
            {
                // Screen Space Canvas면, 원하는 카메라일 때만 켜기
                ray.enabled = (canvas.worldCamera == null || canvas.worldCamera == currentCamera);
            }
        }

        Debug.Log($"[UI Raycast] {cam.name} 카메라 UI만 클릭 가능하도록 설정됨.");
    }
}
