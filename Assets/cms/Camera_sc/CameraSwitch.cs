using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro; // TMP_InputField용
using System.Collections;

public class CameraSwitch : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera targetCamera;
    public float defaultFOV = 60f;
    public float zoomFOV = 40f;
    public float transitionSpeed = 5f;

    [Header("시점 설정")]
    public Transform view1;
    public Transform view2;
    public Transform leftView;
    public Transform rightView;

    private Transform currentView;
    private bool inView2 = false;
    private float targetFOV;

    [Header("URP Feature 설정")]
    [SerializeField, Tooltip("인스펙터에서 단일 Renderer Data를 드래그하세요.")]
    private UniversalRendererData rendererData;

    [SerializeField, Tooltip("첫 번째 Feature 이름")]
    private string featureName1;

    [SerializeField, Tooltip("두 번째 Feature 이름")]
    private string featureName2;

    public float switchDelay = 1.5f;

    private ScriptableRendererFeature feature1;
    private ScriptableRendererFeature feature2;
    private Coroutine switchCoroutine;

    [Header("UI")]
    public TMP_InputField inputField; // TMP_InputField

    void Start()
    {
        currentView = view2;
        transform.position = view2.position;
        transform.rotation = view2.rotation;
        inView2 = true;
        targetFOV = defaultFOV;

        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        // RendererData에서 Feature 찾기
        if (rendererData != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature != null)
                {
                    if (feature.name == featureName1)
                        feature1 = feature;
                    else if (feature.name == featureName2)
                        feature2 = feature;
                }
            }
        }

        // 시작 시 기본 상태: view2 → Feature1 ON, Feature2 OFF
        if (feature1 != null) feature1.SetActive(true);
        if (feature2 != null) feature2.SetActive(false);
    }

    void Update()
    {
        // W → view1 전환 + Feature1 OFF, Feature2 ON (딜레이)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!inView2) return; // S/D 상태에서는 W 무시
            if (inView2 && currentView == view2)
            {
                currentView = view1;
                inView2 = false;
                targetFOV = zoomFOV;

                if (switchCoroutine != null)
                    StopCoroutine(switchCoroutine);

                switchCoroutine = StartCoroutine(DelayedSwitchFeatures());
            }
        }

        // S → view2 전환 + Feature1 ON, Feature2 OFF
        // TMP_InputField 활성화시 무시
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (inputField != null && inputField.isFocused)
                return; // 입력 중이면 무시

            if (currentView != view2)
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
        }

        // S 상태에서만 A/D 이동
        if (inView2)
        {
            if (Input.GetKeyDown(KeyCode.A) && currentView != leftView)
                currentView = leftView;

            if (Input.GetKeyDown(KeyCode.D) && currentView != rightView)
                currentView = rightView;
        }

        // 카메라 이동/회전/FOV 보간
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
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
            Debug.Log("W 눌림 → Feature1 OFF, Feature2 ON");
        }
        else
        {
            if (feature1 != null) feature1.SetActive(true);
            if (feature2 != null) feature2.SetActive(false);
            Debug.Log("S 눌림 → Feature1 ON, Feature2 OFF");
        }
    }
}
