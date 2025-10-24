using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraSwitch : MonoBehaviour
{
    [Header("ī�޶� ����")]
    public Camera targetCamera;
    public float defaultFOV = 60f;
    public float zoomFOV = 40f;
    public float transitionSpeed = 5f;

    [Header("���� ����")]
    public Transform view1;     // ù ��° ���� (W)
    public Transform view2;     // �� ��° ���� (S, �⺻ ������)
    public Transform leftView;  // S ������ �� ���� (A)
    public Transform rightView; // S ������ �� ������ (D)

    private Transform currentView;
    private bool inView2 = false;   // ���� S ��������

    [Header("URP ���̴� ����")]
    public UniversalRendererData rendererData;  // ��: PC_Renderer
    public string featureName = "FullScreenPassRendererFeature"; // Renderer Feature �̸�
    public float offDelay = 1.5f; // W Ű�� �� �� ���� �ð�

    private ScriptableRendererFeature targetFeature;
    private Coroutine offCoroutine;
    private float targetFOV;

    void Start()
    {
        // ���� �� S �������� ����
        currentView = view2;
        transform.position = view2.position;
        transform.rotation = view2.rotation;
        inView2 = true;
        targetFOV = defaultFOV;

        // ī�޶� �ڵ� ����
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        // Renderer Feature Ž��
        if (rendererData != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature != null && feature.name == featureName)
                {
                    targetFeature = feature;
                    break;
                }
            }

            if (targetFeature == null)
                Debug.LogWarning("'" + featureName + "' Renderer Feature�� ã�� �� �����ϴ�!");
        }
        else
        {
            Debug.LogWarning("Renderer Data�� �������� �ʾҽ��ϴ�!");
        }
    }

    void Update()
    {
        // W �� view1 ��ȯ + ���̴� OFF (1.5�� ��)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (inView2 && currentView == view2)
            {
                currentView = view1;
                inView2 = false;
                targetFOV = zoomFOV;

                if (targetFeature != null)
                {
                    if (offCoroutine != null)
                        StopCoroutine(offCoroutine);
                    offCoroutine = StartCoroutine(DelayedShaderOff());
                }
            }
        }

        // S �� view2 ��ȯ + ���̴� ON (���)
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentView != view2)
            {
                currentView = view2;
                inView2 = true;
                targetFOV = defaultFOV;

                if (targetFeature != null)
                {
                    if (offCoroutine != null)
                    {
                        StopCoroutine(offCoroutine);
                        offCoroutine = null;
                    }
                    targetFeature.SetActive(true);
                    Debug.Log(" ���̴� ON (S ����)");
                }
            }
        }

        // S ������ ���� A/D �̵� ����
        if (inView2)
        {
            if (Input.GetKeyDown(KeyCode.A) && currentView != leftView)
            {
                currentView = leftView;
            }

            if (Input.GetKeyDown(KeyCode.D) && currentView != rightView)
            {
                currentView = rightView;
            }
        }

        // ī�޶� �ε巴�� �̵�/ȸ��
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);

        // FOV ����
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
    }

    // ���̴� OFF (������ ����)
    IEnumerator DelayedShaderOff()
    {
        Debug.Log(" " + offDelay + "�� �� ���̴� OFF ����...");
        yield return new WaitForSeconds(offDelay);

        targetFeature.SetActive(false);
        Debug.Log(" ���̴� OFF (W ����)");

        offCoroutine = null;
    }
}
