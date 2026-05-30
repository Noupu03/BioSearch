using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HybridCameraController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;

    [Header("Camera Views (Camera2 Only)")]
    [SerializeField] private Transform view2;
    [SerializeField] private Transform leftView;
    [SerializeField] private Transform rightView;

    [Header("Camera Movement")]
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private float defaultFOV      = 60f;
    [SerializeField] private float zoomFOV         = 40f;

    [Header("Switch Settings")]
    [SerializeField] private float switchDelay = 1.5f;
    private bool  wPressed     = false;
    private float wPressedTime = 0f;

    [Header("UI")]
    [SerializeField] private Canvas          targetCanvas;
    [SerializeField] private TMP_InputField  targetInputField;

    private Camera    activeCamera;
    private Transform currentView;
    private float     targetFOV;
    private bool      inView2          = false;
    private bool      mustPassThroughS = false;

    void Start()
    {
        SetCameraState(camera1, true);
        SetCameraState(camera2, false);
        activeCamera = camera1;

        if (camera2 != null)
        {
            currentView = view2;
            targetFOV   = defaultFOV;
        }

        UpdateCanvasRaycast();
    }

    void Update()
    {
        if (targetInputField != null && targetInputField.isFocused)
            return;

        HandleKeyInput();
        UpdateCameraMovement();
    }

    private void HandleKeyInput()
    {
        // W 키 홀드 → camera1에서 camera2로 전환
        if (activeCamera == camera1)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                wPressed     = true;
                wPressedTime = Time.time;
            }

            if (wPressed && Time.time - wPressedTime >= switchDelay)
            {
                SwitchFrom1To2();
                wPressed = false;
            }

            if (wPressed && Input.anyKeyDown && !Input.GetKeyDown(KeyCode.W))
                wPressed = false;
        }

        // S 키 → camera2에서 camera1으로 복귀
        if (activeCamera == camera2 && Input.GetKeyDown(KeyCode.S))
            SwitchFrom2To1();

        // camera2에서 좌/우/줌 이동
        if (activeCamera == camera2 && inView2)
        {
            if (Input.GetKeyDown(KeyCode.A)) HandleSideView(leftView);
            if (Input.GetKeyDown(KeyCode.D)) HandleSideView(rightView);
            if (Input.GetKeyDown(KeyCode.W)) targetFOV = zoomFOV;
        }
    }

    private void HandleSideView(Transform sideView)
    {
        if (currentView == sideView) return;

        // 다른 사이드 뷰에서 바로 반대쪽으로 가려면 view2를 경유해야 함
        if (!mustPassThroughS)
        {
            currentView      = view2;
            mustPassThroughS = true;
        }
        else
        {
            currentView      = sideView;
            mustPassThroughS = false;
        }
    }

    private void SwitchFrom1To2()
    {
        SetCameraState(camera1, false);
        SetCameraState(camera2, true);
        activeCamera = camera2;
        currentView  = view2;
        inView2      = true;
        targetFOV    = defaultFOV;
        UpdateCanvasRaycast();
    }

    private void SwitchFrom2To1()
    {
        SetCameraState(camera2, false);
        SetCameraState(camera1, true);
        activeCamera = camera1;
        inView2      = false;
        UpdateCanvasRaycast();
    }

    private void UpdateCameraMovement()
    {
        if (activeCamera != camera2 || currentView == null) return;

        Transform camTransform = camera2.transform;
        camTransform.position    = Vector3.Lerp(camTransform.position, currentView.position, Time.deltaTime * transitionSpeed);
        camTransform.rotation    = Quaternion.Lerp(camTransform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);
        camera2.fieldOfView      = Mathf.Lerp(camera2.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
    }

    private static void SetCameraState(Camera cam, bool state) => CameraUtils.SetActive(cam, state);

    private void UpdateCanvasRaycast()
    {
        if (targetCanvas == null) return;
        var gr = targetCanvas.GetComponent<GraphicRaycaster>();
        if (gr != null) gr.enabled = (activeCamera == camera2);
    }
}
