using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HybridCameraController : MonoBehaviour
{
    [Header("Cameras")]
    public Camera camera1;
    public Camera camera2;

    [Header("Camera Views (Camera2 Only)")]
    public Transform view2;
    public Transform leftView;
    public Transform rightView;

    [Header("Camera Movement")]
    public float transitionSpeed = 5f;
    public float defaultFOV = 60f;
    public float zoomFOV = 40f;

    [Header("Switch Settings")]
    public float switchDelay = 1.5f; // W 딜레이
    private bool wPressed = false;
    private float wPressedTime = 0f;

    [Header("UI")]
    public Canvas targetCanvas;
    public TMP_InputField targetInputField;

    private Camera activeCamera;
    private Transform currentView;
    private float targetFOV;
    private bool inView2 = false;
    private bool mustPassThroughS = false;
    private bool isSwitching = false;

    void Start()
    {
        SetCameraState(camera1, true);
        SetCameraState(camera2, false);
        activeCamera = camera1;

        if (camera2 != null)
        {
            currentView = view2;
            targetFOV = defaultFOV;
        }

        UpdateCanvasRaycast();
    }

    void Update()
    {
        if (isSwitching) return;

        if (targetInputField != null && targetInputField.isFocused)
            return;

        HandleKeyInput();
        UpdateCameraMovement();
    }

    private void HandleKeyInput()
    {
        // W 키 눌림 (camera1 → camera2)
        if (activeCamera == camera1)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                wPressed = true;
                wPressedTime = Time.time;
            }

            if (wPressed && Time.time - wPressedTime >= switchDelay)
            {
                StartCoroutine(SwitchFrom1To2());
                wPressed = false;
            }

            if (wPressed && (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.W)))
            {
                wPressed = false;
            }
        }

        // S 키 눌림 (camera2 → camera1)
        if (activeCamera == camera2 && Input.GetKeyDown(KeyCode.S))
        {
            SwitchFrom2To1();
        }

        // camera2일 때만 좌/우 이동
        if (activeCamera == camera2 && inView2)
        {
            if (Input.GetKeyDown(KeyCode.A))
                HandleSideView(leftView);
            if (Input.GetKeyDown(KeyCode.D))
                HandleSideView(rightView);
            if (Input.GetKeyDown(KeyCode.W))
                ZoomView(); // W로 zoom FOV 가능
        }
    }

    private void HandleSideView(Transform sideView)
    {
        if (currentView == sideView) return;

        if (!mustPassThroughS)
        {
            currentView = view2;
            mustPassThroughS = true;
        }
        else
        {
            currentView = sideView;
            mustPassThroughS = false;
        }
    }

    private void ZoomView()
    {
        currentView = currentView; // 그대로 위치 유지
        targetFOV = zoomFOV;
    }

    IEnumerator SwitchFrom1To2()
    {
        isSwitching = true;

        SetCameraState(camera1, false);
        SetCameraState(camera2, true);
        activeCamera = camera2;

        currentView = view2;
        inView2 = true;
        targetFOV = defaultFOV;

        UpdateCanvasRaycast();
        isSwitching = false;
        yield return null;
    }

    void SwitchFrom2To1()
    {
        SetCameraState(camera2, false);
        SetCameraState(camera1, true);
        activeCamera = camera1;

        UpdateCanvasRaycast();
        inView2 = false;
    }

    private void UpdateCameraMovement()
    {
        if (activeCamera == camera2 && currentView != null)
        {
            Transform camTransform = camera2.transform;

            camTransform.position = Vector3.Lerp(camTransform.position, currentView.position, Time.deltaTime * transitionSpeed);
            camTransform.rotation = Quaternion.Lerp(camTransform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);
            camera2.fieldOfView = Mathf.Lerp(camera2.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
        }
    }

    private void SetCameraState(Camera cam, bool state)
    {
        if (cam == null) return;

        cam.enabled = state;
        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener != null)
            listener.enabled = state;
    }

    private void UpdateCanvasRaycast()
    {
        if (targetCanvas == null) return;

        GraphicRaycaster gr = targetCanvas.GetComponent<GraphicRaycaster>();
        if (gr != null)
            gr.enabled = (activeCamera == camera2);
    }
}
