using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 카메라 전환 — 방 모드 / 모니터 모드 분리.
///
/// [방 모드]  camera1(화면) + camera2(renderTexture) 동시 활성.
///           모니터 메시가 camera2 렌더 결과(캔버스 전체)를 표시.
/// [모니터 모드] W 키 → camera1 비활성, camera2.targetTexture=null(화면 직접 렌더).
/// [복귀]     S 키 → camera2.targetTexture 복원, camera1 재활성.
///
/// Canvas가 Screen Space - Camera + worldCamera=camera2 여야 캡처됨.
/// World Space Canvas인 경우 view2 위치에서 전체 캔버스가 보여야 함.
/// </summary>
public class HybridCameraController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;

    [Header("Camera Views (Camera2)")]
    [SerializeField] private Transform view2;
    [SerializeField] private Transform leftView;
    [SerializeField] private Transform rightView;

    [Header("Camera Movement")]
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private float defaultFOV      = 60f;
    [SerializeField] private float zoomFOV         = 40f;

    [Header("Switch Settings")]
    [SerializeField] private float switchDelay = 1.5f;

    [Header("UI")]
    [SerializeField] private Canvas         targetCanvas;
    [SerializeField] private TMP_InputField targetInputField;

    private Camera    activeCamera;
    private Transform currentView;
    private float     targetFOV;
    private bool      inView2;
    private bool      mustPassThroughS;
    private bool      isSwitching;
    private Coroutine switchCoroutine;

    // camera2에서 MonitorSwitch가 설정한 RenderTexture를 저장.
    // 모니터 모드 진입 시 null로 교체, 복귀 시 복원.
    private RenderTexture _monitorRenderTexture;

    IEnumerator Start()
    {
        // camera1 활성, camera2는 일단 비활성 (MonitorSwitch.Start()가 targetTexture 설정하는 것 기다림)
        SetCameraState(camera1, true);
        SetCameraState(camera2, false);
        activeCamera = camera1;

        if (camera2 != null) { currentView = view2; targetFOV = defaultFOV; }

        // Canvas가 Screen Space-Camera + camera2를 worldCamera로 써야 RenderTexture에 캡처됨
        if (targetCanvas != null && camera2 != null)
        {
            if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                targetCanvas.renderMode  = RenderMode.ScreenSpaceCamera;
                targetCanvas.worldCamera = camera2;
                Debug.Log("[HybridCamera] Canvas Overlay → ScreenSpaceCamera(camera2)");
            }
            else if (targetCanvas.renderMode == RenderMode.ScreenSpaceCamera
                     && targetCanvas.worldCamera != camera2)
            {
                targetCanvas.worldCamera = camera2;
                Debug.Log("[HybridCamera] Canvas worldCamera → camera2");
            }
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnWPressed += OnInputWPressed;
            InputManager.Instance.OnSPressed += OnInputSPressed;
        }

        UpdateCanvasRaycast();

        // Frame+1: 모든 Start() 완료 (MonitorSwitch.Start()가 camera2.targetTexture 설정)
        yield return null;
        Canvas.ForceUpdateCanvases();

        // MonitorSwitch가 설정한 RenderTexture 저장 후 camera2 활성화
        // targetTexture가 있으면 화면 대신 텍스처로 렌더 → camera1과 충돌 없음
        if (camera2 != null)
        {
            _monitorRenderTexture = camera2.targetTexture;
            SetCameraState(camera2, true);  // 이제부터 항상 renderTexture에 렌더
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnWPressed -= OnInputWPressed;
            InputManager.Instance.OnSPressed -= OnInputSPressed;
        }
    }

    void Update()
    {
        if (targetInputField != null && targetInputField.isFocused) return;
        UpdateCamera2Input();
        UpdateCameraMovement();
    }

    // ── W/S 이벤트 ───────────────────────────────────────────────────────

    private void OnInputWPressed()
    {
        if (activeCamera != camera1 || isSwitching) return;
        if (InputManager.Instance != null &&
            (InputManager.Instance.APressed || InputManager.Instance.DPressed)) return;

        if (switchCoroutine != null) StopCoroutine(switchCoroutine);
        switchCoroutine = StartCoroutine(Switch1To2Routine());
    }

    private void OnInputSPressed()
    {
        if (isSwitching || activeCamera != camera2) return;
        if (switchCoroutine != null) { StopCoroutine(switchCoroutine); switchCoroutine = null; }
        SwitchFrom2To1();
    }

    private IEnumerator Switch1To2Routine()
    {
        isSwitching = true;
        yield return new WaitForSeconds(switchDelay);
        SwitchFrom1To2();
        isSwitching     = false;
        switchCoroutine = null;
    }

    // ── Camera2 내부 뷰 입력 ─────────────────────────────────────────────

    private void UpdateCamera2Input()
    {
        if (activeCamera != camera2 || !inView2) return;

        if (Input.GetKeyDown(KeyCode.A)) HandleSideView(leftView);
        if (Input.GetKeyDown(KeyCode.D)) HandleSideView(rightView);
        if (Input.GetKeyDown(KeyCode.W)) targetFOV = zoomFOV;
    }

    private void HandleSideView(Transform sideView)
    {
        if (currentView == sideView) return;
        if (!mustPassThroughS) { currentView = view2; mustPassThroughS = true; }
        else { currentView = sideView; mustPassThroughS = false; }
    }

    // ── 카메라 전환 ───────────────────────────────────────────────────────

    private void SwitchFrom1To2()
    {
        SetCameraState(camera1, false);
        // targetTexture 제거 → camera2가 화면으로 직접 렌더 (플레이어가 캔버스를 직접 봄)
        if (camera2 != null) camera2.targetTexture = null;
        SetCameraState(camera2, true);
        activeCamera = camera2;
        currentView  = view2;
        inView2      = true;
        targetFOV    = defaultFOV;
        UpdateCanvasRaycast();
    }

    private void SwitchFrom2To1()
    {
        // RenderTexture 복원 → camera2가 모니터 메시용 텍스처로 렌더
        if (camera2 != null) camera2.targetTexture = _monitorRenderTexture;
        SetCameraState(camera2, true);  // 방 모드에서도 camera2 유지 (항상 renderTexture 렌더)
        SetCameraState(camera1, true);
        activeCamera = camera1;
        inView2      = false;
        UpdateCanvasRaycast();
    }

    // ── 카메라 이동 ───────────────────────────────────────────────────────

    private void UpdateCameraMovement()
    {
        if (activeCamera != camera2 || currentView == null) return;

        Transform ct = camera2.transform;
        ct.position         = Vector3.Lerp(ct.position, currentView.position, Time.deltaTime * transitionSpeed);
        ct.rotation         = Quaternion.Lerp(ct.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);
        camera2.fieldOfView = Mathf.Lerp(camera2.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
    }

    /// <summary>스테이지 전환 시 Camera1(방뷰)으로 즉시 복귀.</summary>
    public void ReturnToRoomView()
    {
        if (switchCoroutine != null) { StopCoroutine(switchCoroutine); switchCoroutine = null; }
        isSwitching = false;

        if (camera2 != null && view2 != null)
        {
            camera2.transform.SetPositionAndRotation(view2.position, view2.rotation);
            camera2.fieldOfView = defaultFOV;
        }

        currentView      = view2;
        targetFOV        = defaultFOV;
        mustPassThroughS = false;

        SwitchFrom2To1();
    }

    private static void SetCameraState(Camera cam, bool state) => CameraUtils.SetActive(cam, state);

    private void UpdateCanvasRaycast()
    {
        if (targetCanvas == null) return;
        var gr = targetCanvas.GetComponent<GraphicRaycaster>();
        if (gr != null) gr.enabled = (activeCamera == camera2);
    }
}
