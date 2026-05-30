using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraPostFX : MonoBehaviour
{
    [Header("Global Volume")]
    [SerializeField] private Volume globalVolume;

    private Bloom     bloom;
    private Vignette  vignette;
    private FilmGrain filmGrain;
    private Coroutine transitionCoroutine;

    [Header("전환 시간 (초)")]
    [SerializeField] private float transitionTime = 1f;

    [Header("1 Room (S 키)")]
    [SerializeField] private float bloomIntensity_Room      = 3f;
    [SerializeField] private float bloomScatter_Room        = 0.5f;
    [SerializeField] private float vignetteIntensity_Room   = 0.33f;
    [SerializeField] private float vignetteSmoothness_Room  = 1f;
    [SerializeField] private float filmGrainIntensity_Room  = 1f;

    [Header("2 Transition (이동 중)")]
    [SerializeField] private float bloomIntensity_Transition      = 1f;
    [SerializeField] private float bloomScatter_Transition        = 1f;
    [SerializeField] private float vignetteIntensity_Transition   = 1f;
    [SerializeField] private float vignetteSmoothness_Transition  = 1f;
    [SerializeField] private float filmGrainIntensity_Transition  = 1f;

    [Header("3 Monitor (W 키)")]
    [SerializeField] private float bloomIntensity_Monitor      = 40f;
    [SerializeField] private float bloomScatter_Monitor        = 0.65f;
    [SerializeField] private float vignetteIntensity_Monitor   = 0.3f;
    [SerializeField] private float vignetteSmoothness_Monitor  = 1f;
    [SerializeField] private float filmGrainIntensity_Monitor  = 1f;

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
            Debug.LogWarning("[CameraPostFX] Global Volume이 연결되지 않았습니다!");
        }

        ApplyRoomValues();

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

    private void OnWPressed()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionToMonitor());
    }

    private void OnSPressed()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(LerpToRoom());
    }

    // Room → Transition(중간점) → Monitor 로 이어지는 2차 베지어 곡선 전환
    private IEnumerator TransitionToMonitor()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionTime;
            float easedT = Mathf.SmoothStep(0, 1, t);

            float midBloom    = Mathf.Lerp(bloomIntensity_Room,     bloomIntensity_Transition,     easedT);
            float midScatter  = Mathf.Lerp(bloomScatter_Room,       bloomScatter_Transition,       easedT);
            float midVignette = Mathf.Lerp(vignetteIntensity_Room,  vignetteIntensity_Transition,  easedT);
            float midSmooth   = Mathf.Lerp(vignetteSmoothness_Room, vignetteSmoothness_Transition, easedT);
            float midGrain    = Mathf.Lerp(filmGrainIntensity_Room, filmGrainIntensity_Transition, easedT);

            bloom.intensity.value     = Mathf.Lerp(midBloom,    bloomIntensity_Monitor,     easedT);
            bloom.scatter.value       = Mathf.Lerp(midScatter,  bloomScatter_Monitor,       easedT);
            vignette.intensity.value  = Mathf.Lerp(midVignette, vignetteIntensity_Monitor,  easedT);
            vignette.smoothness.value = Mathf.Lerp(midSmooth,   vignetteSmoothness_Monitor, easedT);
            filmGrain.intensity.value = Mathf.Lerp(midGrain,    filmGrainIntensity_Monitor, easedT);

            yield return null;
        }

        transitionCoroutine = null;
    }

    private IEnumerator LerpToRoom()
    {
        float t = 0f;

        float startBloom    = bloom.intensity.value;
        float startScatter  = bloom.scatter.value;
        float startVignette = vignette.intensity.value;
        float startSmooth   = vignette.smoothness.value;
        float startGrain    = filmGrain.intensity.value;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionTime;
            float easedT = Mathf.SmoothStep(0, 1, t);

            bloom.intensity.value     = Mathf.Lerp(startBloom,    bloomIntensity_Room,     easedT);
            bloom.scatter.value       = Mathf.Lerp(startScatter,  bloomScatter_Room,       easedT);
            vignette.intensity.value  = Mathf.Lerp(startVignette, vignetteIntensity_Room,  easedT);
            vignette.smoothness.value = Mathf.Lerp(startSmooth,   vignetteSmoothness_Room, easedT);
            filmGrain.intensity.value = Mathf.Lerp(startGrain,    filmGrainIntensity_Room, easedT);

            yield return null;
        }

        transitionCoroutine = null;
    }

    private void ApplyRoomValues()
    {
        if (bloom == null) return;
        bloom.intensity.value     = bloomIntensity_Room;
        bloom.scatter.value       = bloomScatter_Room;
        vignette.intensity.value  = vignetteIntensity_Room;
        vignette.smoothness.value = vignetteSmoothness_Room;
        filmGrain.intensity.value = filmGrainIntensity_Room;
    }
}
