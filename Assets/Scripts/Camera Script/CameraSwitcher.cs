using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;   // 1�� ī�޶�
    public Camera camera2;   // 2�� ī�޶�

    public Canvas targetCanvas; // Ŭ�� ������ ������ Canvas

    private Camera activeCamera;
    private bool isSwitching = false;

    void Start()
    {
        // ���� �� 1�� ī�޶� Ȱ��ȭ, 2���� ��Ȱ��ȭ
        SetCameraState(camera1, true);
        SetCameraState(camera2, false);
        activeCamera = camera1;

        // 1�� ī�޶󿡼� UI ��Ȱ��ȭ
        UpdateCanvasRaycast();
    }

    void Update()
    {
        if (isSwitching) return;

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

        // 2�� ī�޶󿡼� UI Ȱ��ȭ
        UpdateCanvasRaycast();

        isSwitching = false;
    }

    void SwitchFrom2To1()
    {
        SetCameraState(camera2, false);
        SetCameraState(camera1, true);
        activeCamera = camera1;

        // 1�� ī�޶󿡼� UI ��Ȱ��ȭ
        UpdateCanvasRaycast();
    }

    private void SetCameraState(Camera cam, bool state)
    {
        if (cam != null)
        {
            cam.enabled = state;
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = state;
        }
    }

    // Canvas Ŭ�� ����/�Ұ��� ����
    private void UpdateCanvasRaycast()
    {
        if (targetCanvas != null)
        {
            GraphicRaycaster gr = targetCanvas.GetComponent<GraphicRaycaster>();
            if (gr != null)
                gr.enabled = (activeCamera == camera2); // 2�� ī�޶� Ŭ�� ����
        }
    }
}
