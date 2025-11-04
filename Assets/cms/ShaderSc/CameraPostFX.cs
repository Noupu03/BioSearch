using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraPostFX : MonoBehaviour
{
    [Header("Global Volume 연결")]
    public Volume globalVolume;

    private Bloom bloom;
    private Vignette vignette;
    private FilmGrain filmGrain;

    [Header("전환 시간 (초)")]
    public float transitionTime = 1f; // 전환 단계 시간
    private Coroutine transitionCoroutine;

    // --- Room (기본 시점, S 상태) ---
    [Header("1 Room (S 시점)")]
    public float bloomIntensity_Room = 3f;
    public float bloomScatter_Room = 0.5f;
    public float vignetteIntensity_Room = 0.33f;
    public float vignetteSmoothness_Room = 1f;
    public float filmGrainIntensity_Room = 1f;

    // --- Transition (이동 중) ---
    [Header("2 Transition (이동 중)")]
    public float bloomIntensity_Transition = 1f;
    public float bloomScatter_Transition = 1f;
    public float vignetteIntensity_Transition = 1f;
    public float vignetteSmoothness_Transition = 1f;
    public float filmGrainIntensity_Transition = 1f;

    // --- Monitor (도착 후, W 상태) ---
    [Header("3 Monitor (W 시점)")]
    public float bloomIntensity_Monitor = 40f;
    public float bloomScatter_Monitor = 0.65f;
    public float vignetteIntensity_Monitor = 0.3f;
    public float vignetteSmoothness_Monitor = 1f;
    public float filmGrainIntensity_Monitor = 1f;

    void Start()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out bloom);
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out filmGrain);
        }
        else
        {
            Debug.LogWarning(" Global Volume이 연결되지 않았습니다!");
        }

        ApplyRoomValues(); // 시작 시 Room 상태

        // --- InputManager 이벤트 연결 ---
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

    // W 누를 때
    private void OnWPressed()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionToMonitor());
    }

    // S 누를 때
    private void OnSPressed()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(LerpToRoom());
    }

    // Room → Transition → Monitor 순서로 부드럽게 변화
    private IEnumerator TransitionToMonitor()
    {
        float t = 0f;

        // 1단계: Room → Transition → Monitor를 바로 연속 Lerp
        float startBloom = bloom.intensity.value;
        float startScatter = bloom.scatter.value;
        float startVignette = vignette.intensity.value;
        float startSmooth = vignette.smoothness.value;
        float startGrain = filmGrain.intensity.value;

        // 목표값을 Monitor로 바로 잡고, Transition 값은 보정용으로 중간에서 계산
        float targetBloom = bloomIntensity_Monitor;
        float targetScatter = bloomScatter_Monitor;
        float targetVignette = vignetteIntensity_Monitor;
        float targetSmooth = vignetteSmoothness_Monitor;
        float targetGrain = filmGrainIntensity_Monitor;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionTime;
            float easedT = Mathf.SmoothStep(0, 1, t);

            // 중간값(Trajectory) 계산
            float midBloom = Mathf.Lerp(bloomIntensity_Room, bloomIntensity_Transition, easedT);
            float midScatter = Mathf.Lerp(bloomScatter_Room, bloomScatter_Transition, easedT);
            float midVignette = Mathf.Lerp(vignetteIntensity_Room, vignetteIntensity_Transition, easedT);
            float midSmooth = Mathf.Lerp(vignetteSmoothness_Room, vignetteSmoothness_Transition, easedT);
            float midGrain = Mathf.Lerp(filmGrainIntensity_Room, filmGrainIntensity_Transition, easedT);

            // 최종 Lerp (Transition → Monitor)
            bloom.intensity.value = Mathf.Lerp(midBloom, targetBloom, easedT);
            bloom.scatter.value = Mathf.Lerp(midScatter, targetScatter, easedT);
            vignette.intensity.value = Mathf.Lerp(midVignette, targetVignette, easedT);
            vignette.smoothness.value = Mathf.Lerp(midSmooth, targetSmooth, easedT);
            filmGrain.intensity.value = Mathf.Lerp(midGrain, targetGrain, easedT);

            yield return null;
        }

        // 전환 종료 시점에 S 입력 해제
        if (InputManager.Instance != null)
            InputManager.Instance.LockSInput(false);

        transitionCoroutine = null;
    }


    private IEnumerator LerpToRoom()
    {
        float t = 0f;

        float startBloom = bloom.intensity.value;
        float startScatter = bloom.scatter.value;
        float startVignette = vignette.intensity.value;
        float startSmooth = vignette.smoothness.value;
        float startGrain = filmGrain.intensity.value;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionTime;
            float easedT = Mathf.SmoothStep(0, 1, t);

            bloom.intensity.value = Mathf.Lerp(startBloom, bloomIntensity_Room, easedT);
            bloom.scatter.value = Mathf.Lerp(startScatter, bloomScatter_Room, easedT);
            vignette.intensity.value = Mathf.Lerp(startVignette, vignetteIntensity_Room, easedT);
            vignette.smoothness.value = Mathf.Lerp(startSmooth, vignetteSmoothness_Room, easedT);
            filmGrain.intensity.value = Mathf.Lerp(startGrain, filmGrainIntensity_Room, easedT);

            yield return null;
        }

        transitionCoroutine = null;
    }

    // --- 즉시 적용 함수 ---
    private void ApplyRoomValues()
    {
        if (bloom == null) return;
        bloom.intensity.value = bloomIntensity_Room;
        bloom.scatter.value = bloomScatter_Room;
        vignette.intensity.value = vignetteIntensity_Room;
        vignette.smoothness.value = vignetteSmoothness_Room;
        filmGrain.intensity.value = filmGrainIntensity_Room;
    }
}
