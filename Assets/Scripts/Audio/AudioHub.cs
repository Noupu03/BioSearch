// AudioHubExposed.cs
// �ν����� �߽��� ���� ����� ���
// - Mixer �����, ���� �Ķ����, BGM(��Ʈ�Ρ����), ȯ���� ����, SFX/UI ����, ���� ����
// - 2D/3D �ɼ�, ��ġ/���� ����, �׽�Ʈ�� ContextMenu ����

using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Haare.Client.Routine;

public enum AudioBusType { Music, Ambience, SFX, UI, Voice }

public class AudioHubExposed : MonoRoutine
{
    public static AudioHubExposed I { get; private set; }

    #region Mixer / Routing
    [Header("�� �ͼ�/�����")]
    [Tooltip("������Ʈ�� MasterMixer ���� �巡��")]
    public AudioMixer mixer;

    [Tooltip("Music �׷� �Ҵ�(BGM ��� �����)")]
    public AudioMixerGroup musicGroup;
    [Tooltip("Ambience �׷� �Ҵ�(ȯ���� ��� �����)")]
    public AudioMixerGroup ambienceGroup;
    [Tooltip("SFX �׷� �Ҵ�(���� ȿ���� �����)")]
    public AudioMixerGroup sfxGroup;
    [Tooltip("UI �׷� �Ҵ�(��ư/�޴� Ŭ���� �����)")]
    public AudioMixerGroup uiGroup;
    [Tooltip("Voice �׷� �Ҵ�(���/�����̼� �����)")]
    public AudioMixerGroup voiceGroup;

    [Header("�� Exposed �Ķ���� �̸�(dB)")]
    [Tooltip("Mixer���� Expose�� �Ķ���͸�(��: MusicVolume)")]
    public string musicVolParam = "MusicVolume";
    [Tooltip("Mixer���� Expose�� �Ķ���͸�(��: AmbienceVolume)")]
    public string ambienceVolParam = "AmbienceVolume";
    [Tooltip("Mixer���� Expose�� �Ķ���͸�(��: SFXVolume)")]
    public string sfxVolParam = "SFXVolume";
    [Tooltip("Mixer���� Expose�� �Ķ���͸�(��: UIVolume)")]
    public string uiVolParam = "UIVolume";
    [Tooltip("Mixer���� Expose�� �Ķ���͸�(��: VoiceVolume)")]
    public string voiceVolParam = "VoiceVolume";
    [Tooltip("����: ��ü ������ ���� �Ķ���͸�(������ ���)")]
    public string masterVolParam = "MasterVolume";
    #endregion

    #region Music (Intro �� Loop)
    [System.Serializable]
    public class MusicSettings
    {
        [Header("�� BGM ��Ʈ��/����")]
        [Tooltip("��Ʈ�� 1ȸ ���(������ ���)")]
        public AudioClip intro;
        [Tooltip("��Ʈ�� ���� �� ���� ���� ����(������ ��Ʈ�θ� ���)")]
        public AudioClip loop;

        [Header("���̵�/��ȯ")]
        [Tooltip("��Ʈ�Ρ���� ��ȯ ũ�ν����̵� �ð�(��)")]
        [Range(0f, 1f)] public float crossfadeSec = 0.2f;
        [Tooltip("BGM ���� �� ���̵�ƿ� �ð�(��)")]
        [Range(0f, 2f)] public float stopFadeSec = 0.3f;

        [Header("��� �ɼ�")]
        [Tooltip("�� ���� �� BGM �ڵ� ���")]
        public bool playOnStart = false;
        [Tooltip("BGM AudioSource�� �⺻ ��ġ")]
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Tooltip("BGM�� 2D�� ���(����). 3D ����ȭ�� ���ϸ� ����")]
        public bool force2D = true;
    }

    [Header("=== BGM ���� ===")]
    public MusicSettings music = new MusicSettings();

    [Tooltip("��Ʈ��/���� ����� �ҽ� 2��(�ڵ�����). �ʿ� �� ���� �Ҵ� ����")]
    public AudioSource musicA; // intro
    public AudioSource musicB; // loop
    #endregion

    #region Ambience (Loop)
    [System.Serializable]
    public class AmbienceSettings
    {
        [Header("�� ȯ���� ����")]
        [Tooltip("ȯ���� ���� ����(ȭ��Ʈ������/�ٶ�/����� ��)")]
        public AudioClip loop;
        [Tooltip("�� ���� �� ȯ���� �ڵ� ���")]
        public bool playOnStart = false;

        [Header("��� �ɼ�")]
        [Tooltip("ȯ���� AudioSource�� �⺻ ��ġ")]
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Tooltip("ȯ������ 2D�� ���(����). 3D ����ȭ�� ���ϸ� ����")]
        public bool force2D = true;
    }

    [Header("=== ȯ���� ���� ===")]
    public AmbienceSettings ambienceCfg = new AmbienceSettings();

    [Tooltip("ȯ���� ������ AudioSource(�ڵ�����). �ʿ� �� ���� �Ҵ� ����")]
    public AudioSource ambienceSrc;
    #endregion

    #region One-Shot (SFX, UI, Voice)
    [System.Serializable]
    public class OneShotSettings
    {
        [Header("�� ����(����)")]
        [Tooltip("ȿ���� Ǯ ������(���� ��� ������ ��)")]
        [Range(1, 32)] public int voices = 12;
        [Tooltip("��� ������ �������� ���� ���� ������(0~1)")]
        [Range(0f, 1f)] public float masterOneShotVolume = 1f;

        [Header("����/��ġ")]
        [Tooltip("PlayOneShot ȣ�� �� �⺻ ����(���� ȣ�Ⱚ�� ������)")]
        [Range(0f, 1f)] public float defaultVolume = 1f;
        [Tooltip("PlayOneShot ȣ�� �� �⺻ ��ġ")]
        [Range(0.5f, 2f)] public float defaultPitch = 1f;
        [Tooltip("��� �� ��ġ ���� ����(����)")]
        [Range(0f, 0.2f)] public float defaultPitchVariance = 0f;

        [Header("���� ����(����)")]
        [Tooltip("�׷� Ű�� �ּ� ����(ms). 0�̸� ���� ����")]
        [Range(0, 500)] public int minIntervalMs = 40;
        [Tooltip("�⺻ �׷� Ű(��: ui-click). ȣ�� �� ��� �� ����")]
        public string defaultSpamKey = "ui-click";

        [Header("2D/3D ����")]
        [Tooltip("�⺻ 2D ���(����). ���� ����� ȣ�� �� worldPos ���")]
        public bool force2D = true;
        [Tooltip("3D ��� �� �⺻ SpatialBlend(0=2D,1=3D)")]
        [Range(0f, 1f)] public float spatialBlend3D = 1f;
        [Tooltip("3D ��� �� Min Distance")]
        public float minDistance = 1f;
        [Tooltip("3D ��� �� Max Distance")]
        public float maxDistance = 25f;
        [Tooltip("3D ��� �� �ѿ��� ���(0=Log,1=Linear,2=Custom)")]
        [Range(0, 2)] public int rolloffMode = 0;
    }

    [Header("=== ����(SFX/UI/Voice) ���� ===")]
    public OneShotSettings oneShot = new OneShotSettings();

    [Tooltip("������ AudioSource Ǯ(�ڵ�����)")]
    public List<AudioSource> sfxPool = new List<AudioSource>();
    #endregion

    // ���� ����
    int _nextVoice = 0;
    readonly Dictionary<string, double> _lastByKey = new Dictionary<string, double>();

    protected override void Constructor()
    {
        base.Constructor();
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // ���� �ҽ� �غ�
        if (!musicA) musicA = NewChildSource("MusicA", musicGroup, loop: false, force2D: music.force2D, pitch: music.pitch);
        if (!musicB) musicB = NewChildSource("MusicB", musicGroup, loop: true, force2D: music.force2D, pitch: music.pitch);

        // ȯ���� �ҽ� �غ�
        if (!ambienceSrc) ambienceSrc = NewChildSource("Ambience", ambienceGroup, loop: true, force2D: ambienceCfg.force2D, pitch: ambienceCfg.pitch);

        // ���� Ǯ �غ�
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
        // ���� ����
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

    #region Music API (Intro �� Loop)
    [ContextMenu("BGM/���(��Ʈ�Ρ����)")]
    public void Ctx_PlayMusic() => PlayMusic(music.intro, music.loop, music.crossfadeSec);

    [ContextMenu("BGM/����(���̵�ƿ�)")]
    public void Ctx_StopMusic() => StopMusic(music.stopFadeSec);

    public void PlayMusic(AudioClip intro, AudioClip loop, float crossfadeSec = 0.2f)
    {
        StopAllCoroutines();

        // ���� ����
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
    [ContextMenu("Ambience/���(����)")]
    public void Ctx_PlayAmbience() { if (ambienceCfg.loop) PlayAmbience(ambienceCfg.loop); }

    [ContextMenu("Ambience/���")]
    public void Ctx_ToggleAmbience()
    {
        if (!ambienceSrc.clip && ambienceCfg.loop) ambienceSrc.clip = ambienceCfg.loop;
        ToggleAmbience(!ambienceSrc.isPlaying);
    }

    [ContextMenu("Ambience/����(ó������)")]
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
    [ContextMenu("SFX/�׽�Ʈ ����(UI �����)")]
    public void Ctx_TestOneShotUI()
    {
        // �ν����Ϳ��� �׽�Ʈ�� Ŭ���� �ӽ÷� �����ϰ� �ʹٸ�, �Ʒ� ������ �޾Ƽ� ȣ���ϸ� ��.
        // (������ ���� ������ ������ �ʰ� ContextMenu�θ� ���� ����)
        Debug.Log("ContextMenu ����: AudioHubExposed.Ctx_TestOneShotUI() ȣ��");
    }

    public void PlayOneShot(AudioClip clip, AudioBusType bus = AudioBusType.SFX,
                            float volume = -1f, float pitch = -1f, float pitchVar = -1f,
                            string spamKey = null, int minIntervalMs = -1,
                            Vector3? worldPos = null)
    {
        if (!clip || sfxPool.Count == 0) return;

        // ���� �⺻�� ����(�ν����� �� �켱 ���)
        float vol = (volume < 0f) ? oneShot.defaultVolume : volume;
        float pit = (pitch < 0f) ? oneShot.defaultPitch : pitch;
        float pvr = (pitchVar < 0f) ? oneShot.defaultPitchVariance : pitchVar;
        string key = string.IsNullOrEmpty(spamKey) ? oneShot.defaultSpamKey : spamKey;
        int minMs = (minIntervalMs < 0) ? oneShot.minIntervalMs : minIntervalMs;

        // ���� ����
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

        // 2D/3D ��ġ
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
        // �����
        AudioMixerGroup group = sfxGroup;
        if (bus == AudioBusType.UI && uiGroup) group = uiGroup;
        else if (bus == AudioBusType.Voice && voiceGroup) group = voiceGroup;
        else if (bus == AudioBusType.Ambience && ambienceGroup) group = ambienceGroup;
        else if (bus == AudioBusType.Music && musicGroup) group = musicGroup;

        // �� �ҽ� �켱, ������ ����κ� ��ƿ
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
    [ContextMenu("Mixer/�ʱ� ������ Ȯ��(Log)")]
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

    #region ������ ����(��Ÿ�� �� ���� �ݿ�)
    void OnValidate()
    {
        // ��Ÿ�� �� �ν����� �� ���� �ÿ��� �ݿ��ǵ��� �ּ����� �ݿ� ó��
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

    [ContextMenu("Pool/�����(���� ���̽� �� ���� �ݿ�)")]
    public void RebuildPool() => BuildPool();
    #endregion
}
