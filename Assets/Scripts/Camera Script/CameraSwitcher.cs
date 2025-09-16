using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1;   // 1�� ī�޶�
    public Camera camera2;   // 2�� ī�޶�

    private Camera activeCamera;
    private bool isSwitching = false;

    void Start()
    {
        // ���� �� 1�� ī�޶� Ȱ��ȭ, 2���� ��Ȱ��ȭ
        SetCameraState(camera1, true);
        SetCameraState(camera2, false);
        activeCamera = camera1;
    }

    void Update()
    {
        if (isSwitching) return;

        // 1�� ī�޶󿡼� W �� 3�� �� ��ȯ
        if (activeCamera == camera1 && Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(SwitchFrom1To2());
        }
        // 2�� ī�޶󿡼� S �� ��� ��ȯ
        else if (activeCamera == camera2 && Input.GetKeyDown(KeyCode.S))
        {
            SwitchFrom2To1();
        }
    }

    IEnumerator SwitchFrom1To2()
    {
        isSwitching = true;

        // 3�� ���
        yield return new WaitForSeconds(3f);

        // ī�޶� ��ȯ
        SetCameraState(camera1, false);
        SetCameraState(camera2, true);
        activeCamera = camera2;

        isSwitching = false;
    }

    void SwitchFrom2To1()
    {
        // ��� ��ȯ
        SetCameraState(camera2, false);
        SetCameraState(camera1, true);
        activeCamera = camera1;
    }

    // ī�޶�� ����� ������ ���¸� �Բ� ����
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
