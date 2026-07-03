// GlobalMousePressReleaseSfxExposed.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Haare.Client.Routine;

[RequireComponent(typeof(AudioSource))]
public class GlobalMousePressReleaseSfxExposed : MonoRoutine
{
    [Header("Mixer �����")]
    [Tooltip("����� AudioMixerGroup (��: UI �Ǵ� SFX �׷�)")]
    public AudioMixerGroup outputGroup;

    [Header("UI ���� ó��")]
    [Tooltip("���콺�� UI ���� ���� ���� ���带 ���� ����")]
    public bool ignoreWhenOverUI = false;

    [Header("������ ��ư")]
    public bool useLeft = true;
    public bool useRight = false;
    public bool useMiddle = false;

    [System.Serializable]
    public class ClipSettings
    {
        [Tooltip("����� Ŭ��")]
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Tooltip("��ġ ���� ����(��)")]
        [Range(0f, 0.2f)] public float pitchVariance = 0.03f;
        [Tooltip("��Ÿ �ּ� ����(ms). 0�̸� ���� ����")]
        [Range(0, 200)] public int minIntervalMs = 30;
    }

    [Header("���� ��(Down) ����")]
    public ClipSettings leftDown = new ClipSettings();
    public ClipSettings rightDown = new ClipSettings();
    public ClipSettings middleDown = new ClipSettings();

    [Header("�� ��(Up) ����")]
    public ClipSettings leftUp = new ClipSettings();
    public ClipSettings rightUp = new ClipSettings();
    public ClipSettings middleUp = new ClipSettings();

    private AudioSource _src;
    private readonly Dictionary<string, double> _lastDsp = new Dictionary<string, double>();

    protected override void Constructor()
    {
        base.Constructor();
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.spatialBlend = 0f; // ���� UI�� �ǵ���̹Ƿ� 2D ����
        if (outputGroup) _src.outputAudioMixerGroup = outputGroup;
    }

    protected override void UpdateProcess()
    {
        base.UpdateProcess();
        if (ignoreWhenOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Left
        if (useLeft)
        {
            if (Input.GetMouseButtonDown(0)) TryPlay("L_D", leftDown);
            if (Input.GetMouseButtonUp(0)) TryPlay("L_U", leftUp);
        }

        // Right
        if (useRight)
        {
            if (Input.GetMouseButtonDown(1)) TryPlay("R_D", rightDown);
            if (Input.GetMouseButtonUp(1)) TryPlay("R_U", rightUp);
        }

        // Middle
        if (useMiddle)
        {
            if (Input.GetMouseButtonDown(2)) TryPlay("M_D", middleDown);
            if (Input.GetMouseButtonUp(2)) TryPlay("M_U", middleUp);
        }
    }

    void TryPlay(string key, ClipSettings cfg)
    {
        if (cfg == null || cfg.clip == null) return;

        // ���� ����
        if (cfg.minIntervalMs > 0 && _lastDsp.TryGetValue(key, out var last))
        {
            double dtMs = (AudioSettings.dspTime - last) * 1000.0;
            if (dtMs < cfg.minIntervalMs) return;
        }
        _lastDsp[key] = AudioSettings.dspTime;

        // ��ġ ���� ����
        float prevPitch = _src.pitch;
        _src.pitch = Mathf.Clamp(cfg.pitch + Random.Range(-cfg.pitchVariance, cfg.pitchVariance), 0.5f, 2f);

        _src.PlayOneShot(cfg.clip, cfg.volume);

        _src.pitch = prevPitch; // ����
    }
}
