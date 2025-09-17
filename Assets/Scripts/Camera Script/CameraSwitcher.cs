using UnityEngine;
using System.Collections;
using TMPro; // TMP_InputField 사용 시 필요

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    public TMP_InputField inputField; // 입력 중 체크용

    private Camera activeCamera;
    private bool isSwitching = false;

    void Start()
    {
        SetCameraState(camera1, true);
        SetCameraState(camera2, false);
        activeCamera = camera1;
    }

    void Update()
    {
        if (isSwitching) return;

        // 입력 중이면 카메라 전환 막기
        if (inputField != null && inputField.isFocused) return;

        if (activeCamera == camera1 && Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(SwitchFrom1To2());
        }
        else if (activeCamera == camera2 && Input.GetKeyDown(KeyCode.S))
        {
            SwitchFrom2To1();
        }
    }

    IEnumerator SwitchFrom1To2()
    {
        isSwitching = true;
        yield return new WaitForSeconds(3f);
        SetCameraState(camera1, false);
        SetCameraState(camera2, true);
        activeCamera = camera2;
        isSwitching = false;
    }

    void SwitchFrom2To1()
    {
        SetCameraState(camera2, false);
        SetCameraState(camera1, true);
        activeCamera = camera1;
    }

    private void SetCameraState(Camera cam, bool state)
    {
        if (cam != null)
        {
            cam.enabled = state;
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = state;
            }
        }
    }
}
