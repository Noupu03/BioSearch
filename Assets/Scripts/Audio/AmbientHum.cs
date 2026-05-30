using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AmbientHum : MonoBehaviour
{
    public AudioClip introClip;
    public AudioClip loopClip;

    [Header("Levels")]
    [Range(0f, 1f)] public float volume = 0.6f;
    [Range(0.5f, 2f)] public float pitch = 1.0f;
    [Range(0f, 0.2f)] public float fadeTime = 0.05f;
    public bool autoPlayOnStart = false;

    [Header("Scheduling")]
    [Tooltip("스케줄 여유시간(초). 자동 계산값과 중 큰 값을 사용")]
    [Range(0.02f, 0.25f)] public float scheduleLead = 0.10f;
    [Tooltip("루프 시작 폴백 체크 지연(초)")]
    [Range(0.02f, 0.25f)] public float fallbackProbe = 0.08f;

    AudioSource aIntro, bLoop;
    bool started = false;
    enum Stage { Stopped, Intro, Loop }
    Stage stage = Stage.Stopped;

    void Awake()
    {
        aIntro = gameObject.AddComponent<AudioSource>();
        bLoop = gameObject.AddComponent<AudioSource>();
        foreach (var s in new[] { aIntro, bLoop })
        {
            s.playOnAwake = false; s.loop = false; s.spatialBlend = 0f;
            s.volume = 0f; s.pitch = pitch;
            s.bypassEffects = true; s.bypassListenerEffects = true; s.bypassReverbZones = true;
#if UNITY_2020_2_OR_NEWER
            s.ignoreListenerPause = true; s.ignoreListenerVolume = true;
#endif
            s.dopplerLevel = 0f;
        }

        // DSP 버퍼 기반 리드타임 상향(플랫폼별 안정화)
        int buf, nbuf; AudioSettings.GetDSPBufferSize(out buf, out nbuf);
        float sr = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 48000f;
        float minLead = Mathf.Clamp((buf / sr) * nbuf * 2f, 0.06f, 0.15f);
        scheduleLead = Mathf.Max(scheduleLead, minLead);
    }

    void Start()
    {
        if (autoPlayOnStart) PlayFromIntro();
    }

    // ───────── API ─────────
    public void Toggle() { if (started) Stop(); else PlayFromIntro(); }
    public void ResetFromIntro() => PlayFromIntro();
    public void SetVolume(float v) { volume = Mathf.Clamp01(v); ApplyVolumes(); }
    public void SetPitch(float p, bool restartIfIntro = true)
    {
        pitch = Mathf.Clamp(p, 0.5f, 2f);
        if (stage == Stage.Intro && restartIfIntro) { bool was = started; Stop(true); if (was) PlayFromIntro(); return; }
        aIntro.pitch = bLoop.pitch = pitch;
    }
    // ───────────────────────

    void ApplyVolumes()
    {
        if (!started) { aIntro.volume = bLoop.volume = 0f; return; }
        aIntro.volume = bLoop.volume = volume;
    }

    public void PlayFromIntro()
    {
        if (!introClip || !loopClip) { Debug.LogWarning("AmbientHum: Clips not assigned."); return; }
        StopAllCoroutines();

        // 사전 로드
        introClip.LoadAudioData();
        loopClip.LoadAudioData();
        started = true; stage = Stage.Intro;
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        // 로드 완료 대기
        while (introClip.loadState == AudioDataLoadState.Loading ||
               loopClip.loadState == AudioDataLoadState.Loading)
            yield return null;

        if (introClip.loadState != AudioDataLoadState.Loaded ||
            loopClip.loadState != AudioDataLoadState.Loaded)
        {
            Debug.LogWarning("AmbientHum: Clip not loaded. Check import settings.");
            yield break;
        }

        // 공통 세팅
        aIntro.Stop(); bLoop.Stop();
        aIntro.clip = introClip; bLoop.clip = loopClip;
        aIntro.loop = false; bLoop.loop = true;
        aIntro.pitch = bLoop.pitch = pitch;
        aIntro.volume = 0f; bLoop.volume = 0f;

        // 스케줄
        double now = AudioSettings.dspTime;
        double start = now + scheduleLead;
        double introDur = (double)introClip.samples / introClip.frequency / pitch;
        double loopStart = start + introDur;

        aIntro.PlayScheduled(start);
        bLoop.PlayScheduled(loopStart);

        // 페이드
        StartCoroutine(FadeTo(aIntro, volume, fadeTime, 0f));
        StartCoroutine(FadeTo(bLoop, volume, fadeTime, (float)(loopStart - now)));

        // 루프 진입 마킹
        yield return new WaitForSecondsRealtime((float)(loopStart - now) + 0.01f);
        if (started) stage = Stage.Loop;

        // ─ 폴백: 루프 시작 직후에도 미재생이면 즉시 재생
        yield return new WaitForSecondsRealtime(fallbackProbe);
        if (started && !bLoop.isPlaying)
        {
            bLoop.time = 0f;
            bLoop.loop = true;
            bLoop.volume = volume; bLoop.pitch = pitch;
            bLoop.Play(); // 샘플 정확도는 아니지만 무음 방지
        }
    }

    public void Stop(bool instant = false)
    {
        if (!started) return;
        started = false; stage = Stage.Stopped;
        StopAllCoroutines();
        if (instant || fadeTime <= 0f) { aIntro.Stop(); bLoop.Stop(); aIntro.volume = bLoop.volume = 0f; }
        else StartCoroutine(FadeOutAndStop());
    }

    IEnumerator FadeTo(AudioSource s, float target, float time, float delay)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        float start = s.volume, t = 0f;
        while (t < time) { t += Time.unscaledDeltaTime; s.volume = Mathf.Lerp(start, target, t / time); yield return null; }
        s.volume = target;
    }

    IEnumerator FadeOutAndStop()
    {
        float a0 = aIntro.volume, b0 = bLoop.volume, t = 0f;
        while (t < fadeTime) { t += Time.unscaledDeltaTime; float f = 1f - Mathf.Clamp01(t / fadeTime); aIntro.volume = a0 * f; bLoop.volume = b0 * f; yield return null; }
        aIntro.Stop(); bLoop.Stop(); aIntro.volume = bLoop.volume = 0f;
    }
}
