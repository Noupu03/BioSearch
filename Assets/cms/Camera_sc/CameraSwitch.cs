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
    public Transform view1;     // 첫 번째 시점 (W)
    public Transform view2;     // 두 번째 시점 (S, 기본 시작점)
    public Transform leftView;  // S 상태일 때 왼쪽 (A)
    public Transform rightView; // S 상태일 때 오른쪽 (D)

    private Transform currentView;
    private bool inView2 = false;   // 현재 S 시점인지

    [Header("URP 셰이더 설정")]
    public UniversalRendererData rendererData;  // 예: PC_Renderer
    public string featureName = "FullScreenPassRendererFeature"; // Renderer Feature 이름
    public float offDelay = 1.5f; // W 키로 끌 때 지연 시간

    private ScriptableRendererFeature targetFeature;
    private Coroutine offCoroutine;
    private float targetFOV;

    void Start()
    {
        // 시작 시 S 시점으로 설정
        currentView = view2;
        transform.position = view2.position;
        transform.rotation = view2.rotation;
        inView2 = true;
        targetFOV = defaultFOV;

        // 카메라 자동 연결
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        // Renderer Feature 탐색
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
                Debug.LogWarning("'" + featureName + "' Renderer Feature를 찾을 수 없습니다!");
        }
        else
        {
            Debug.LogWarning("Renderer Data가 지정되지 않았습니다!");
        }
    }

    void Update()
    {
        // W → view1 전환 + 셰이더 OFF (1.5초 후)
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

        // S → view2 전환 + 셰이더 ON (즉시)
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
                    Debug.Log(" 셰이더 ON (S 눌림)");
                }
            }
        }

        // S 상태일 때만 A/D 이동 가능
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

        // 카메라 부드럽게 이동/회전
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);

        // FOV 보간
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);
    }

    // 셰이더 OFF (딜레이 포함)
    IEnumerator DelayedShaderOff()
    {
        Debug.Log(" " + offDelay + "초 후 셰이더 OFF 예정...");
        yield return new WaitForSeconds(offDelay);

        targetFeature.SetActive(false);
        Debug.Log(" 셰이더 OFF (W 눌림)");

        offCoroutine = null;
    }
}
