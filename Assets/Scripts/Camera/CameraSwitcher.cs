using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;
    [SerializeField] private float  switchDelay = 1.5f;

    private Camera     activeCamera;
    private bool       isSwitching;
    private Coroutine  switchCoroutine;

    private enum ViewMode { Front, Left, Right }
    private ViewMode currentView = ViewMode.Front;

    void Start()
    {
        CameraUtils.SetActive(camera1, true);
        CameraUtils.SetActive(camera2, false);
        activeCamera = camera1;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnWPressed  += OnWPressed;
            InputManager.Instance.OnSPressed  += OnSPressed;
            InputManager.Instance.OnADChanged += OnADChanged;
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnWPressed  -= OnWPressed;
            InputManager.Instance.OnSPressed  -= OnSPressed;
            InputManager.Instance.OnADChanged -= OnADChanged;
        }
    }

    private void OnADChanged()
    {
        if (activeCamera != camera1) return;
        if      (InputManager.Instance.APressed) currentView = ViewMode.Left;
        else if (InputManager.Instance.DPressed) currentView = ViewMode.Right;
        else                                     currentView = ViewMode.Front;
    }

    private void OnWPressed()
    {
        if (activeCamera != camera1 || currentView != ViewMode.Front) return;
        if (InputManager.Instance.APressed || InputManager.Instance.DPressed) return;
        if (isSwitching) return;

        if (switchCoroutine != null) StopCoroutine(switchCoroutine);
        switchCoroutine = StartCoroutine(Switch1To2Routine());
    }

    private void OnSPressed()
    {
        if (isSwitching || activeCamera != camera2) return;

        if (switchCoroutine != null) StopCoroutine(switchCoroutine);
        switchCoroutine = StartCoroutine(Switch2To1Routine());
    }

    IEnumerator Switch1To2Routine()
    {
        isSwitching = true;
        yield return new WaitForSeconds(switchDelay);

        CameraUtils.SetActive(camera1, false);
        CameraUtils.SetActive(camera2, true);
        activeCamera = camera2;
        currentView  = ViewMode.Front;
        isSwitching  = false;

        InputManager.Instance?.LockSInput(false);
    }

    IEnumerator Switch2To1Routine()
    {
        isSwitching = true;

        CameraUtils.SetActive(camera2, false);
        CameraUtils.SetActive(camera1, true);
        activeCamera = camera1;
        currentView  = ViewMode.Front;
        isSwitching  = false;

        InputManager.Instance?.LockSInput(false);
        yield return null;
    }
}
