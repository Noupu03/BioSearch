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
    public Transform view1;
    public Transform view2;
    public Transform leftView;
    public Transform rightView;

    private Transform currentView;
    private bool inView2 = false;

    [Header("URP ���̴� ����")]
    [SerializeField, Tooltip("�ν����Ϳ��� 2���� ������ �����͸� �巡���ϼ���.")]
    private UniversalRendererData[] rendererDatas = new UniversalRendererData[2]; // 2�� ����
    public string featureName = "FullScreenPassRendererFeature";
    public float offDelay = 1.5f;

    private ScriptableRendererFeature[] targetFeatures = new ScriptableRendererFeature[2];
    private Coroutine offCoroutine;
    private float targetFOV;

    void Start()
    {
        // ���� �� S ����
        currentView = view2;
        transform.position = view2.position;
        transform.rotation = view2.rotation;
        inView2 = true;
        targetFOV = defaultFOV;

        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        // �� ������ �����Ϳ��� Feature Ž��
        for (int i = 0; i < rendererDatas.Length; i++)
        {
            var data = rendererDatas[i];
            if (data != null)
            {
                foreach (var feature in data.rendererFeatures)
                {
                    if (feature != null && feature.name == featureName)
                    {
                        targetFeatures[i] = feature;
                        break;
                    }
                }

                if (targetFeatures[i] == null)
                    Debug.LogWarning($"Renderer {i}���� '{featureName}'�� ã�� �� �����ϴ�!");
            }
            else
            {
                Debug.LogWarning($"Renderer {i}�� �Ҵ���� �ʾҽ��ϴ�!");
            }
        }

        // ���� �� �⺻ ����: view2 �� 1�� �Ѱ� 2�� ����
        if (targetFeatures[0] != null) targetFeatures[0].SetActive(true);
        if (targetFeatures[1] != null) targetFeatures[1].SetActive(false);
    }

    void Update()
    {
        // W �� view1 ��ȯ + 1�� OFF, 2�� ON (������ ����)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (inView2 && currentView == view2)
            {
                currentView = view1;
                inView2 = false;
                targetFOV = zoomFOV;

                if (offCoroutine != null)
                    StopCoroutine(offCoroutine);
                offCoroutine = StartCoroutine(DelayedSwitchShader(true));
            }
        }

        // S �� view2 ��ȯ + 1�� ON, 2�� OFF
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentView != view2)
            {
                currentView = view2;
                inView2 = true;
                targetFOV = defaultFOV;

                if (offCoroutine != null)
                {
                    StopCoroutine(offCoroutine);
                    offCoroutine = null;
                }

                SwitchShader(false);
            }
        }

        // S ������ ���� A/D �̵�
        if (inView2)
        {
            if (Input.GetKeyDown(KeyCode.A) && currentView != leftView)
                currentView = leftView;

            if (Input.GetKeyDown(KeyCode.D) && currentView != rightView)
                currentView = rightView;
        }

        // ī�޶� �̵�/ȸ�� ����
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
    }

    // W ���� �� ������ ����
    IEnumerator DelayedSwitchShader(bool isW)
    {
        yield return new WaitForSeconds(offDelay);
        SwitchShader(isW);
        offCoroutine = null;
    }

    // Shader ���� ��ȯ �Լ�
    private void SwitchShader(bool wPressed)
    {
        if (targetFeatures.Length < 2) return;

        if (wPressed)
        {
            // W �� 1�� OFF, 2�� ON
            if (targetFeatures[0] != null) targetFeatures[0].SetActive(false);
            if (targetFeatures[1] != null) targetFeatures[1].SetActive(true);
            Debug.Log("W ���� �� 1�� OFF, 2�� ON");
        }
        else
        {
            // S �� 1�� ON, 2�� OFF
            if (targetFeatures[0] != null) targetFeatures[0].SetActive(true);
            if (targetFeatures[1] != null) targetFeatures[1].SetActive(false);
            Debug.Log("S ���� �� 1�� ON, 2�� OFF");
        }
    }
}
