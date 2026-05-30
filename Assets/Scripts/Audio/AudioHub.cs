// AudioHubExposed.cs
// 인스펙터 중심의 통합 오디오 허브
// - Mixer 라우팅, 볼륨 파라미터, BGM(인트로→루프), 환경음 루프, SFX/UI 원샷, 스팸 억제
// - 2D/3D 옵션, 피치/볼륨 랜덤, 테스트용 ContextMenu 지원

using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public enum AudioBusType { Music, Ambience, SFX, UI, Voice }

public class AudioHubExposed : MonoBehaviour
{
    public static AudioHubExposed I { get; private set; }

    #region Mixer / Routing
    [Header("■ 믹서/라우팅")]
    [Tooltip("프로젝트의 MasterMixer 에셋 드래그")]
    public AudioMixer mixer;

    [Tooltip("Music 그룹 할당(BGM 출력 라우팅)")]
    public AudioMixerGroup musicGroup;
    [Tooltip("Ambience 그룹 할당(환경음 출력 라우팅)")]
    public AudioMixerGroup ambienceGroup;
    [Tooltip("SFX 그룹 할당(게임 효과음 라우팅)")]
    public AudioMixerGroup sfxGroup;
    [Tooltip("UI 그룹 할당(버튼/메뉴 클릭음 라우팅)")]
    public AudioMixerGroup uiGroup;
    [Tooltip("Voice 그룹 할당(대사/나레이션 라우팅)")]
    public AudioMixerGroup voiceGroup;

    [Header("■ Exposed 파라미터 이름(dB)")]
    [Tooltip("Mixer에서 Expose한 파라미터명(예: MusicVolume)")]
    public string musicVolParam = "MusicVolume";
    [Tooltip("Mixer에서 Expose한 파라미터명(예: AmbienceVolume)")]
    public string ambienceVolParam = "AmbienceVolume";
    [Tooltip("Mixer에서 Expose한 파라미터명(예: SFXVolume)")]
    public string sfxVolParam = "SFXVolume";
    [Tooltip("Mixer에서 Expose한 파라미터명(예: UIVolume)")]
    public string uiVolParam = "UIVolume";
    [Tooltip("Mixer에서 Expose한 파라미터명(예: VoiceVolume)")]
    public string voiceVolParam = "VoiceVolume";
    [Tooltip("선택: 전체 마스터 볼륨 파라미터명(없으면 비움)")]
    public string masterVolParam = "MasterVolume";
    #endregion

    #region Music (Intro → Loop)
    [System.Serializable]
    public class MusicSettings
    {
        [Header("■ BGM 인트로/루프")]
        [Tooltip("인트로 1회 재생(없으면 비움)")]
        public AudioClip intro;
        [Tooltip("인트로 종료 후 무한 루프 음원(없으면 인트로만 재생)")]
        public AudioClip loop;

        [Header("페이드/전환")]
        [Tooltip("인트로→루프 전환 크로스페이드 시간(초)")]
        [Range(0f, 1f)] public float crossfadeSec = 0.2f;
        [Tooltip("BGM 정지 시 페이드아웃 시간(초)")]
        [Range(0f, 2f)] public float stopFadeSec = 0.3f;

        [Header("재생 옵션")]
        [Tooltip("씬 시작 시 BGM 자동 재생")]
        public bool playOnStart = false;
        [Tooltip("BGM AudioSource의 기본 피치")]
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Tooltip("BGM을 2D로 재생(권장). 3D 공간화를 원하면 해제")]
        public bool force2D = true;
    }

    [Header("=== BGM 설정 ===")]
    public MusicSettings music = new MusicSettings();

    [Tooltip("인트로/루프 재생용 소스 2개(자동생성). 필요 시 직접 할당 가능")]
    public AudioSource musicA; // intro
    public AudioSource musicB; // loop
    #endregion

    #region Ambience (Loop)
    [System.Serializable]
    public class AmbienceSettings
    {
        [Header("■ 환경음 루프")]
        [Tooltip("환경음 루프 음원(화이트노이즈/바람/기계음 등)")]
        public AudioClip loop;
        [Tooltip("씬 시작 시 환경음 자동 재생")]
        public bool playOnStart = false;

        [Header("재생 옵션")]
        [Tooltip("환경음 AudioSource의 기본 피치")]
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Tooltip("환경음을 2D로 재생(권장). 3D 공간화를 원하면 해제")]
        public bool force2D = true;
    }

    [Header("=== 환경음 설정 ===")]
    public AmbienceSettings ambienceCfg = new AmbienceSettings();

    [Tooltip("환경음 루프용 AudioSource(자동생성). 필요 시 직접 할당 가능")]
    public AudioSource ambienceSrc;
    #endregion

    #region One-Shot (SFX, UI, Voice)
    [System.Serializable]
    public class OneShotSettings
    {
        [Header("■ 공통(원샷)")]
        [Tooltip("효과음 풀 사이즈(동시 재생 가능한 수)")]
        [Range(1, 32)] public int voices = 12;
        [Tooltip("모든 원샷에 곱해지는 전역 볼륨 스케일(0~1)")]
        [Range(0f, 1f)] public float masterOneShotVolume = 1f;

        [Header("볼륨/피치")]
        [Tooltip("PlayOneShot 호출 시 기본 볼륨(개별 호출값과 곱연산)")]
        [Range(0f, 1f)] public float defaultVolume = 1f;
        [Tooltip("PlayOneShot 호출 시 기본 피치")]
        [Range(0.5f, 2f)] public float defaultPitch = 1f;
        [Tooltip("재생 시 피치 랜덤 범위(±값)")]
        [Range(0f, 0.2f)] public float defaultPitchVariance = 0f;

        [Header("스팸 억제(선택)")]
        [Tooltip("그룹 키별 최소 간격(ms). 0이면 제한 없음")]
        [Range(0, 500)] public int minIntervalMs = 40;
        [Tooltip("기본 그룹 키(예: ui-click). 호출 시 덮어쓸 수 있음")]
        public string defaultSpamKey = "ui-click";

        [Header("2D/3D 설정")]
        [Tooltip("기본 2D 재생(권장). 월드 사운드는 호출 시 worldPos 사용")]
        public bool force2D = true;
        [Tooltip("3D 재생 시 기본 SpatialBlend(0=2D,1=3D)")]
        [Range(0f, 1f)] public float spatialBlend3D = 1f;
        [Tooltip("3D 재생 시 Min Distance")]
        public float minDistance = 1f;
        [Tooltip("3D 재생 시 Max Distance")]
        public float maxDistance = 25f;
        [Tooltip("3D 재생 시 롤오프 모드(0=Log,1=Linear,2=Custom)")]
        [Range(0, 2)] public int rolloffMode = 0;
    }

    [Header("=== 원샷(SFX/UI/Voice) 설정 ===")]
    public OneShotSettings oneShot = new OneShotSettings();

    [Tooltip("원샷용 AudioSource 풀(자동생성)")]
    public List<AudioSource> sfxPool = new List<AudioSource>();
    #endregion

    // 내부 상태
    int _nextVoice = 0;
    readonly Dictionary<string, double> _lastByKey = new Dictionary<string, double>();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // 음악 소스 준비
        if (!musicA) musicA = NewChildSource("MusicA", musicGroup, loop: false, force2D: music.force2D, pitch: music.pitch);
        if (!musicB) musicB = NewChildSource("MusicB", musicGroup, loop: true, force2D: music.force2D, pitch: music.pitch);

        // 환경음 소스 준비
        if (!ambienceSrc) ambienceSrc = NewChildSource("Ambience", ambienceGroup, loop: true, force2D: ambienceCfg.force2D, pitch: ambienceCfg.pitch);

        // 원샷 풀 준비
        BuildPool();
    }

    void Start()
    {
        if (music.playOnStart) PlayMusic(music.intro, music.loop, music.crossfadeSec);
        if (ambienceCfg.playOnStart && ambienceCfg.loop) PlayAmbience(ambienceCfg.loop);
    }

    #region Factory / Pool
    AudioSource NewChildSource(string name, AudioMixerGroup group, bool loop, bool force2D, float pitch)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        src.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        src.spatialBlend = force2D ? 0f : 1f;
        if (!force2D)
        {
            src.minDistance = oneShot.minDistance;
            src.maxDistance = oneShot.maxDistance;
            src.rolloffMode = (AudioRolloffMode)oneShot.rolloffMode;
        }
        if (group) src.outputAudioMixerGroup = group;
        return src;
    }

    void BuildPool()
    {
        // 기존 정리
        foreach (var s in sfxPool)
            if (s) Destroy(s.gameObject);
        sfxPool.Clear();

        for (int i = 0; i < Mathf.Max(1, oneShot.voices); i++)
        {
            var s = NewChildSource($"SFX_{i}", sfxGroup, loop: false, force2D: oneShot.force2D, pitch: oneShot.defaultPitch);
            s.spatialBlend = oneShot.force2D ? 0f : oneShot.spatialBlend3D;
            s.minDistance = oneShot.minDistance;
            s.maxDistance = oneShot.maxDistance;
            s.rolloffMode = (AudioRolloffMode)oneShot.rolloffMode;
            sfxPool.Add(s);
        }
        _nextVoice = 0;
    }
    #endregion

    #region Music API (Intro → Loop)
    [ContextMenu("BGM/재생(인트로→루프)")]
    public void Ctx_PlayMusic() => PlayMusic(music.intro, music.loop, music.crossfadeSec);

    [ContextMenu("BGM/정지(페이드아웃)")]
    public void Ctx_StopMusic() => StopMusic(music.stopFadeSec);

    public void PlayMusic(AudioClip intro, AudioClip loop, float crossfadeSec = 0.2f)
    {
        StopAllCoroutines();

        // 루프 세팅
        musicB.clip = loop;
        musicB.time = 0f;
        musicB.loop = loop != null;
        musicB.volume = 1f;
        musicB.pitch = music.pitch;

        if (intro != null)
        {
            musicA.clip = intro;
            musicA.time = 0f;
            musicA.loop = false;
            musicA.volume = 1f;
            musicA.pitch = music.pitch;
            musicA.Play();

            if (loop != null)
            {
                double startDsp = AudioSettings.dspTime;
                double introLen = intro.length / Mathf.Max(0.01f, musicA.pitch);
                double loopStartDsp = startDsp + introLen - crossfadeSec;
                musicB.PlayScheduled(loopStartDsp);
                StartCoroutine(FadeAB(musicA, musicB, crossfadeSec, (float)(loopStartDsp - AudioSettings.dspTime)));
            }
        }
        else
        {
            if (loop != null) musicB.Play();
            musicA.Stop();
        }
    }

    public void StopMusic(float fadeSec = 0.25f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndStop(musicA, fadeSec));
        StartCoroutine(FadeOutAndStop(musicB, fadeSec));
    }
    #endregion

    #region Ambience API (Loop)
    [ContextMenu("Ambience/재생(루프)")]
    public void Ctx_PlayAmbience() { if (ambienceCfg.loop) PlayAmbience(ambienceCfg.loop); }

    [ContextMenu("Ambience/토글")]
    public void Ctx_ToggleAmbience()
    {
        if (!ambienceSrc.clip && ambienceCfg.loop) ambienceSrc.clip = ambienceCfg.loop;
        ToggleAmbience(!ambienceSrc.isPlaying);
    }

    [ContextMenu("Ambience/리셋(처음부터)")]
    public void Ctx_ResetAmbience() => ResetAmbience();

    public void PlayAmbience(AudioClip loop)
    {
        if (!loop) { ambienceSrc.Stop(); return; }
        ambienceSrc.clip = loop;
        ambienceSrc.time = 0f;
        ambienceSrc.loop = true;
        ambienceSrc.pitch = ambienceCfg.pitch;
        ambienceSrc.spatialBlend = ambienceCfg.force2D ? 0f : 1f;
        ambienceSrc.Play();
    }

    public void ToggleAmbience(bool on)
    {
        if (on)
        {
            if (ambienceSrc.clip == null) ambienceSrc.clip = ambienceCfg.loop;
            if (ambienceSrc.clip && !ambienceSrc.isPlaying) ambienceSrc.Play();
        }
        else ambienceSrc.Stop();
    }

    public void ResetAmbience()
    {
        if (ambienceSrc.clip == null) return;
        ambienceSrc.Stop(); ambienceSrc.time = 0f; ambienceSrc.Play();
    }
    #endregion

    #region One-Shot API (SFX/UI/Voice)
    [ContextMenu("SFX/테스트 원샷(UI 라우팅)")]
    public void Ctx_TestOneShotUI()
    {
        // 인스펙터에서 테스트용 클립을 임시로 지정하고 싶다면, 아래 변수로 받아서 호출하면 됨.
        // (별도의 전용 슬롯을 만들지 않고 ContextMenu로만 예시 제공)
        Debug.Log("ContextMenu 예시: AudioHubExposed.Ctx_TestOneShotUI() 호출");
    }

    public void PlayOneShot(AudioClip clip, AudioBusType bus = AudioBusType.SFX,
                            float volume = -1f, float pitch = -1f, float pitchVar = -1f,
                            string spamKey = null, int minIntervalMs = -1,
                            Vector3? worldPos = null)
    {
        if (!clip || sfxPool.Count == 0) return;

        // 인자 기본값 대입(인스펙터 값 우선 사용)
        float vol = (volume < 0f) ? oneShot.defaultVolume : volume;
        float pit = (pitch < 0f) ? oneShot.defaultPitch : pitch;
        float pvr = (pitchVar < 0f) ? oneShot.defaultPitchVariance : pitchVar;
        string key = string.IsNullOrEmpty(spamKey) ? oneShot.defaultSpamKey : spamKey;
        int minMs = (minIntervalMs < 0) ? oneShot.minIntervalMs : minIntervalMs;

        // 스팸 억제
        if (minMs > 0 && !string.IsNullOrEmpty(key))
        {
            if (_lastByKey.TryGetValue(key, out var last))
            {
                double dtMs = (AudioSettings.dspTime - last) * 1000.0;
                if (dtMs < minMs) return;
            }
            _lastByKey[key] = AudioSettings.dspTime;
        }

        var src = Rent(bus);

        // 2D/3D 위치
        if (worldPos.HasValue && !oneShot.force2D)
        {
            src.transform.position = worldPos.Value;
            src.spatialBlend = oneShot.spatialBlend3D;
        }
        else
        {
            src.transform.localPosition = Vector3.zero;
            src.spatialBlend = 0f;
        }

        src.pitch = Mathf.Clamp(pit + Random.Range(-pvr, pvr), 0.5f, 2f);
        src.volume = Mathf.Clamp01(vol * oneShot.masterOneShotVolume);
        src.PlayOneShot(clip);
    }

    AudioSource Rent(AudioBusType bus)
    {
        // 라우팅
        AudioMixerGroup group = sfxGroup;
        if (bus == AudioBusType.UI && uiGroup) group = uiGroup;
        else if (bus == AudioBusType.Voice && voiceGroup) group = voiceGroup;
        else if (bus == AudioBusType.Ambience && ambienceGroup) group = ambienceGroup;
        else if (bus == AudioBusType.Music && musicGroup) group = musicGroup;

        // 빈 소스 우선, 없으면 라운드로빈 스틸
        for (int i = 0; i < sfxPool.Count; i++)
        {
            int idx = (_nextVoice + i) % sfxPool.Count;
            if (!sfxPool[idx].isPlaying)
            {
                _nextVoice = (idx + 1) % sfxPool.Count;
                sfxPool[idx].outputAudioMixerGroup = group;
                return sfxPool[idx];
            }
        }
        var s = sfxPool[_nextVoice];
        _nextVoice = (_nextVoice + 1) % sfxPool.Count;
        s.outputAudioMixerGroup = group;
        return s;
    }
    #endregion

    #region Volume / Mute (Mixer Exposed dB)
    [ContextMenu("Mixer/초기 볼륨값 확인(Log)")]
    public void Ctx_LogVolumes()
    {
        TryGetVolume(AudioBusType.Music, out var a);
        TryGetVolume(AudioBusType.Ambience, out var b);
        TryGetVolume(AudioBusType.SFX, out var c);
        TryGetVolume(AudioBusType.UI, out var d);
        TryGetVolume(AudioBusType.Voice, out var e);
        Debug.Log($"[AudioHub] Music:{a}dB / Amb:{b}dB / SFX:{c}dB / UI:{d}dB / Voice:{e}dB");
    }

    public void SetVolume(AudioBusType bus, float dB)
    {
        if (!mixer) return;
        mixer.SetFloat(ParamOf(bus), dB);
    }

    public bool TryGetVolume(AudioBusType bus, out float dB)
    {
        dB = 0f;
        return mixer && mixer.GetFloat(ParamOf(bus), out dB);
    }

    public void Mute(AudioBusType bus, bool mute)
    {
        if (!mixer) return;
        string p = ParamOf(bus);
        if (!mixer.GetFloat(p, out float cur)) cur = 0f;
        mixer.SetFloat(p, mute ? -80f : Mathf.Clamp(cur, -80f, 0f));
    }

    string ParamOf(AudioBusType bus)
    {
        return bus switch
        {
            AudioBusType.Music => musicVolParam,
            AudioBusType.Ambience => ambienceVolParam,
            AudioBusType.SFX => sfxVolParam,
            AudioBusType.UI => uiVolParam,
            AudioBusType.Voice => voiceVolParam,
            _ => masterVolParam
        };
    }
    #endregion

    #region Fade Utils
    System.Collections.IEnumerator FadeAB(AudioSource a, AudioSource b, float sec, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float t = 0f;
        b.volume = 0f;
        while (t < sec)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / sec);
            if (a) a.volume = 1f - k;
            if (b) b.volume = k;
            yield return null;
        }
        if (a) { a.Stop(); a.volume = 1f; }
        if (b) b.volume = 1f;
    }

    System.Collections.IEnumerator FadeOutAndStop(AudioSource s, float sec)
    {
        if (!s || !s.isPlaying || sec <= 0f) { if (s) s.Stop(); yield break; }
        float t = 0f; float v0 = s.volume;
        while (t < sec)
        {
            t += Time.deltaTime; s.volume = Mathf.Lerp(v0, 0f, t / sec);
            yield return null;
        }
        s.Stop(); s.volume = v0;
    }
    #endregion

    #region 에디터 편의(런타임 중 변경 반영)
    void OnValidate()
    {
        // 런타임 중 인스펙터 값 변경 시에도 반영되도록 최소한의 반영 처리
        if (musicA)
        {
            musicA.pitch = music.pitch;
            musicA.spatialBlend = music.force2D ? 0f : 1f;
        }
        if (musicB)
        {
            musicB.pitch = music.pitch;
            musicB.spatialBlend = music.force2D ? 0f : 1f;
        }
        if (ambienceSrc)
        {
            ambienceSrc.pitch = ambienceCfg.pitch;
            ambienceSrc.spatialBlend = ambienceCfg.force2D ? 0f : 1f;
        }
    }

    [ContextMenu("Pool/재빌드(원샷 보이스 수 변경 반영)")]
    public void RebuildPool() => BuildPool();
    #endregion
}
