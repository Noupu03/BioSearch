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
    public float transitionTime = 1.5f; // ��ȯ �ܰ� �ð�
    private Coroutine transitionCoroutine;

    // --- Room (�⺻ ����, S ����) ---
    [Header("1 Room (S ����)")]
    public float bloomIntensity_Room = 1f;
    public float bloomScatter_Room = 0.65f;
    public float vignetteIntensity_Room = 0.25f;
    public float vignetteSmoothness_Room = 1f;
    public float filmGrainIntensity_Room = 1f;

    // --- Transition (�̵� ��) ---
    [Header("2 Transition (�̵� ��)")]
    public float bloomIntensity_Transition = 0.6f;
    public float bloomScatter_Transition = 0.45f;
    public float vignetteIntensity_Transition = 0.4f;
    public float vignetteSmoothness_Transition = 0.9f;
    public float filmGrainIntensity_Transition = 0.7f;

    // --- Monitor (���� ��, W ����) ---
    [Header("3 Monitor (W ����)")]
    public float bloomIntensity_Monitor = 0.3f;
    public float bloomScatter_Monitor = 0.3f;
    public float vignetteIntensity_Monitor = 0.55f;
    public float vignetteSmoothness_Monitor = 0.8f;
    public float filmGrainIntensity_Monitor = 0.5f;

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
    }

    void Update()
    {
        // --- W Ű: Transition �� ���� �ð� �� Monitor ---
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(TransitionToMonitor());
        }

        // --- S Ű: Room���� ���� ---
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(LerpToRoom());
        }
    }

    // Room �� Transition �� Monitor ������ ��ȭ
    IEnumerator TransitionToMonitor()
    {
        // 1�ܰ�: Transition �� ��� ����
        ApplyTransitionValues();

        // 2�ܰ�: ���� �ð� ���
        yield return new WaitForSeconds(transitionTime);

        // 3�ܰ�: �ε巴�� Monitor ������ ��ȯ
        yield return StartCoroutine(LerpToMonitor());
    }

    IEnumerator LerpToMonitor()
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
            float easedT = Mathf.SmoothStep(0, 1, t); // �ڿ������� ���̵�

            bloom.intensity.value = Mathf.Lerp(startBloom, bloomIntensity_Monitor, easedT);
            bloom.scatter.value = Mathf.Lerp(startScatter, bloomScatter_Monitor, easedT);
            vignette.intensity.value = Mathf.Lerp(startVignette, vignetteIntensity_Monitor, easedT);
            vignette.smoothness.value = Mathf.Lerp(startSmooth, vignetteSmoothness_Monitor, easedT);
            filmGrain.intensity.value = Mathf.Lerp(startGrain, filmGrainIntensity_Monitor, easedT);

            yield return null;
        }
    }

    IEnumerator LerpToRoom()
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
            float easedT = Mathf.SmoothStep(0, 1, t); // �ڿ������� ����

            bloom.intensity.value = Mathf.Lerp(startBloom, bloomIntensity_Room, easedT);
            bloom.scatter.value = Mathf.Lerp(startScatter, bloomScatter_Room, easedT);
            vignette.intensity.value = Mathf.Lerp(startVignette, vignetteIntensity_Room, easedT);
            vignette.smoothness.value = Mathf.Lerp(startSmooth, vignetteSmoothness_Room, easedT);
            filmGrain.intensity.value = Mathf.Lerp(startGrain, filmGrainIntensity_Room, easedT);

            yield return null;
        }
    }

    // --- ��� ���� �Լ� ---
    void ApplyRoomValues()
    {
        if (bloom == null) return;
        bloom.intensity.value = bloomIntensity_Room;
        bloom.scatter.value = bloomScatter_Room;
        vignette.intensity.value = vignetteIntensity_Room;
        vignette.smoothness.value = vignetteSmoothness_Room;
        filmGrain.intensity.value = filmGrainIntensity_Room;
    }

    void ApplyTransitionValues()
    {
        if (bloom == null) return;
        bloom.intensity.value = bloomIntensity_Transition;
        bloom.scatter.value = bloomScatter_Transition;
        vignette.intensity.value = vignetteIntensity_Transition;
        vignette.smoothness.value = vignetteSmoothness_Transition;
        filmGrain.intensity.value = filmGrainIntensity_Transition;
    }
}