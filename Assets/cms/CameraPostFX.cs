using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CameraPostFX : MonoBehaviour
{
    [Header("Global Volume ����")]
    public Volume globalVolume;

    private Bloom bloom;
    private Vignette vignette;
    private FilmGrain filmGrain;

    [Header("��ȯ �ð� (��)")]
    public float transitionTime = 1f; // ��ȯ �ܰ� �ð�
    private Coroutine transitionCoroutine;

    // --- Room (�⺻ ����, S ����) ---
    [Header("1 Room (S ����)")]
    public float bloomIntensity_Room = 3f;
    public float bloomScatter_Room = 0.5f;
    public float vignetteIntensity_Room = 0.33f;
    public float vignetteSmoothness_Room = 1f;
    public float filmGrainIntensity_Room = 1f;

    // --- Transition (�̵� ��) ---
    [Header("2 Transition (�̵� ��)")]
    public float bloomIntensity_Transition = 1f;
    public float bloomScatter_Transition = 1f;
    public float vignetteIntensity_Transition = 1f;
    public float vignetteSmoothness_Transition = 1f;
    public float filmGrainIntensity_Transition = 1f;

    // --- Monitor (���� ��, W ����) ---
    [Header("3 Monitor (W ����)")]
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
            Debug.LogWarning(" Global Volume�� ������� �ʾҽ��ϴ�!");
        }

        ApplyRoomValues(); // ���� �� Room ����

        // --- InputManager �̺�Ʈ ���� ---
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

    // W ���� ��
    private void OnWPressed()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionToMonitor());
    }

    // S ���� ��
    private void OnSPressed()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(LerpToRoom());
    }

    // Room �� Transition �� Monitor ������ �ε巴�� ��ȭ
    private IEnumerator TransitionToMonitor()
    {
        float t = 0f;

        // 1�ܰ�: Room �� Transition �� Monitor�� �ٷ� ���� Lerp
        float startBloom = bloom.intensity.value;
        float startScatter = bloom.scatter.value;
        float startVignette = vignette.intensity.value;
        float startSmooth = vignette.smoothness.value;
        float startGrain = filmGrain.intensity.value;

        // ��ǥ���� Monitor�� �ٷ� ���, Transition ���� ���������� �߰����� ���
        float targetBloom = bloomIntensity_Monitor;
        float targetScatter = bloomScatter_Monitor;
        float targetVignette = vignetteIntensity_Monitor;
        float targetSmooth = vignetteSmoothness_Monitor;
        float targetGrain = filmGrainIntensity_Monitor;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionTime;
            float easedT = Mathf.SmoothStep(0, 1, t);

            // �߰���(Trajectory) ���
            float midBloom = Mathf.Lerp(bloomIntensity_Room, bloomIntensity_Transition, easedT);
            float midScatter = Mathf.Lerp(bloomScatter_Room, bloomScatter_Transition, easedT);
            float midVignette = Mathf.Lerp(vignetteIntensity_Room, vignetteIntensity_Transition, easedT);
            float midSmooth = Mathf.Lerp(vignetteSmoothness_Room, vignetteSmoothness_Transition, easedT);
            float midGrain = Mathf.Lerp(filmGrainIntensity_Room, filmGrainIntensity_Transition, easedT);

            // ���� Lerp (Transition �� Monitor)
            bloom.intensity.value = Mathf.Lerp(midBloom, targetBloom, easedT);
            bloom.scatter.value = Mathf.Lerp(midScatter, targetScatter, easedT);
            vignette.intensity.value = Mathf.Lerp(midVignette, targetVignette, easedT);
            vignette.smoothness.value = Mathf.Lerp(midSmooth, targetSmooth, easedT);
            filmGrain.intensity.value = Mathf.Lerp(midGrain, targetGrain, easedT);

            yield return null;
        }

        // ��ȯ ���� ������ S �Է� ����
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

    // --- ��� ���� �Լ� ---
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
