using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;

    public Canvas targetCanvas;
    public TMP_InputField targetInputField;

    private Camera activeCamera;
    private bool isSwitching = false;

    private float wPressedTime = 0f;
    private bool wPressed = false;
    public float switchDelay = 1.5f; // W ���� �� ��� �ð�

    void Start()
    {
        SetCameraState(camera1, true);
        SetCameraState(camera2, false);
        activeCamera = camera1;
        UpdateCanvasRaycast();
    }

    void Update()
    {
        if (isSwitching) return;

        if (targetInputField != null && targetInputField.isFocused)
            return;

        // W Ű �� camera1 �� camera2 ��ȯ
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

            // �ٸ� Ű �Է� �� �ʱ�ȭ
            if (wPressed && (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.W)))
            {
                wPressed = false;
            }
        }

        // S Ű �� camera2 �� camera1 ��ȯ
        if (activeCamera == camera2 && Input.GetKeyDown(KeyCode.S))
        {
            SwitchFrom2To1();
        }
    }

    IEnumerator SwitchFrom1To2()
    {
        isSwitching = true;

        yield return null; // ���� ���� �ٷ� ��ȯ ����

        SetCameraState(camera1, false);
        SetCameraState(camera2, true);
        activeCamera = camera2;
        UpdateCanvasRaycast();

        isSwitching = false;
    }

    void SwitchFrom2To1()
    {
        SetCameraState(camera2, false);
        SetCameraState(camera1, true);
        activeCamera = camera1;
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

    private void UpdateCanvasRaycast()
    {
        if (targetCanvas != null)
        {
            GraphicRaycaster gr = targetCanvas.GetComponent<GraphicRaycaster>();
            if (gr != null)
                gr.enabled = (activeCamera == camera2);
        }
    }
}
