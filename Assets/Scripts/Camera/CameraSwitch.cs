using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraSwitch : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float  defaultFOV      = 60f;
    [SerializeField] private float  zoomFOV         = 40f;
    [SerializeField] private float  transitionSpeed  = 5f;
    [SerializeField] private float  switchDelay      = 1.5f;

    [Header("뷰 포인트")]
    [SerializeField] private Transform view1;
    [SerializeField] private Transform view2;
    [SerializeField] private Transform leftView;
    [SerializeField] private Transform rightView;

    [Header("URP Feature 전환")]
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private string featureName1;
    [SerializeField] private string featureName2;

    private Transform              currentView;
    private bool                   inView2 = true;
    private float                  targetFOV;

    private ScriptableRendererFeature feature1;
    private ScriptableRendererFeature feature2;
    private Coroutine switchCoroutine;

    void Start()
    {
        currentView = view2;
        transform.position = view2.position;
        transform.rotation = view2.rotation;
        targetFOV = defaultFOV;

        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (rendererData != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature != null)
                {
                    if (feature.name == featureName1) feature1 = feature;
                    else if (feature.name == featureName2) feature2 = feature;
                }
            }
        }

        if (feature1 != null) feature1.SetActive(true);
        if (feature2 != null) feature2.SetActive(false);

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnWPressed += OnWPressed;
            InputManager.Instance.OnSPressed += OnSPressed;
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnWPressed -= OnWPressed;
            InputManager.Instance.OnSPressed -= OnSPressed;
        }
    }

    void Update()
    {
        // A/D ���� ��ȯ (S ����(view2)������)
        if (inView2)
        {
            if (InputManager.Instance.APressed && currentView != leftView)
                currentView = leftView;
            else if (InputManager.Instance.DPressed && currentView != rightView)
                currentView = rightView;
        }

        // ī�޶� �̵�/ȸ��/FOV ����
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
    }

    private void OnWPressed()
    {
        // �ݵ�� S ����(view2)������ W ���
        if (!inView2) return;

        // A/D �Է� ���̸� W ����
        if (InputManager.Instance.APressed || InputManager.Instance.DPressed) return;

        currentView = view1;
        inView2 = false;
        targetFOV = zoomFOV;

        if (switchCoroutine != null) StopCoroutine(switchCoroutine);
        switchCoroutine = StartCoroutine(DelayedSwitchFeatures());
    }

    private void OnSPressed()
    {
        currentView = view2;
        inView2 = true;
        targetFOV = defaultFOV;

        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
            switchCoroutine = null;
        }
        SwitchFeatures(false);
    }

    IEnumerator DelayedSwitchFeatures()
    {
        yield return new WaitForSeconds(switchDelay);
        SwitchFeatures(true);
        switchCoroutine = null;
    }

    private void SwitchFeatures(bool wPressed)
    {
        if (wPressed)
        {
            if (feature1 != null) feature1.SetActive(false);
            if (feature2 != null) feature2.SetActive(true);
        }
        else
        {
            if (feature1 != null) feature1.SetActive(true);
            if (feature2 != null) feature2.SetActive(false);
        }
    }
}
