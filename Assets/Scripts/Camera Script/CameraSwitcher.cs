using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public float switchDelay = 1.5f;

    private Camera activeCamera;
    private bool isSwitching = false;
    private Coroutine switchCoroutine;

    private enum ViewMode { Front, Left, Right }
    private ViewMode currentView = ViewMode.Front;

    void Start()
    {
        SetCameraState(camera1, true);
        SetCameraState(camera2, false);
        activeCamera = camera1;
        currentView = ViewMode.Front;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnWPressed += OnWPressed;
            InputManager.Instance.OnSPressed += OnSPressed;
            InputManager.Instance.OnADChanged += OnADChanged;
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnWPressed -= OnWPressed;
            InputManager.Instance.OnSPressed -= OnSPressed;
            InputManager.Instance.OnADChanged -= OnADChanged;
        }
    }

    private void OnADChanged()
    {
        if (activeCamera != camera1) return;

        if (InputManager.Instance.APressed) currentView = ViewMode.Left;
        else if (InputManager.Instance.DPressed) currentView = ViewMode.Right;
        else currentView = ViewMode.Front;
    }

    private void OnWPressed()
    {
        // 반드시 S 상태(camera1/front)에서만 W 허용
        if (activeCamera != camera1 || currentView != ViewMode.Front) return;
        if (InputManager.Instance.APressed || InputManager.Instance.DPressed) return;
        if (isSwitching) return;

        if (switchCoroutine != null) StopCoroutine(switchCoroutine);
        switchCoroutine = StartCoroutine(Switch1To2Routine());
    }

    private void OnSPressed()
    {
        if (activeCamera != camera2 || isSwitching) return;

        if (switchCoroutine != null) StopCoroutine(switchCoroutine);
        switchCoroutine = StartCoroutine(Switch2To1Routine());
    }

    IEnumerator Switch1To2Routine()
    {
        isSwitching = true;
        yield return new WaitForSeconds(switchDelay);

        SetCameraState(camera1, false);
        SetCameraState(camera2, true);
        activeCamera = camera2;
        currentView = ViewMode.Front;

        isSwitching = false;
    }

    IEnumerator Switch2To1Routine()
    {
        isSwitching = true;

        SetCameraState(camera2, false);
        SetCameraState(camera1, true);
        activeCamera = camera1;
        currentView = ViewMode.Front;

        isSwitching = false;
        yield return null;
    }

    private void SetCameraState(Camera cam, bool state)
    {
        if (cam == null) return;

        cam.enabled = state;
        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = state;
    }
}
