using UnityEngine;
using UnityEngine.Rendering.Universal;
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

    [Header("URP 셰이더 설정")]
    [SerializeField, Tooltip("인스펙터에서 2개의 렌더러 데이터를 드래그하세요.")]
    private UniversalRendererData[] rendererDatas = new UniversalRendererData[2]; // 2개 고정
    public string featureName = "FullScreenPassRendererFeature";
    public float offDelay = 1.5f;

    private ScriptableRendererFeature[] targetFeatures = new ScriptableRendererFeature[2];
    private Coroutine offCoroutine;
    private float targetFOV;

    void Start()
    {
        // 시작 시 S 시점
        currentView = view2;
        transform.position = view2.position;
        transform.rotation = view2.rotation;
        inView2 = true;
        targetFOV = defaultFOV;

        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        // 각 렌더러 데이터에서 Feature 탐색
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
                    Debug.LogWarning($"Renderer {i}에서 '{featureName}'를 찾을 수 없습니다!");
            }
            else
            {
                Debug.LogWarning($"Renderer {i}가 할당되지 않았습니다!");
            }
        }

        // 시작 시 기본 상태: view2 → 1번 켜고 2번 끄기
        if (targetFeatures[0] != null) targetFeatures[0].SetActive(true);
        if (targetFeatures[1] != null) targetFeatures[1].SetActive(false);
    }

    void Update()
    {
        // W → view1 전환 + 1번 OFF, 2번 ON (딜레이 가능)
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

        // S → view2 전환 + 1번 ON, 2번 OFF
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

        // S 상태일 때만 A/D 이동
        if (inView2)
        {
            if (Input.GetKeyDown(KeyCode.A) && currentView != leftView)
                currentView = leftView;

            if (Input.GetKeyDown(KeyCode.D) && currentView != rightView)
                currentView = rightView;
        }

        // 카메라 이동/회전 보간
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
    }

    // W 누를 때 딜레이 포함
    IEnumerator DelayedSwitchShader(bool isW)
    {
        yield return new WaitForSeconds(offDelay);
        SwitchShader(isW);
        offCoroutine = null;
    }

    // Shader 상태 전환 함수
    private void SwitchShader(bool wPressed)
    {
        if (targetFeatures.Length < 2) return;

        if (wPressed)
        {
            // W → 1번 OFF, 2번 ON
            if (targetFeatures[0] != null) targetFeatures[0].SetActive(false);
            if (targetFeatures[1] != null) targetFeatures[1].SetActive(true);
            Debug.Log("W 눌림 → 1번 OFF, 2번 ON");
        }
        else
        {
            // S → 1번 ON, 2번 OFF
            if (targetFeatures[0] != null) targetFeatures[0].SetActive(true);
            if (targetFeatures[1] != null) targetFeatures[1].SetActive(false);
            Debug.Log("S 눌림 → 1번 ON, 2번 OFF");
        }
    }
}
