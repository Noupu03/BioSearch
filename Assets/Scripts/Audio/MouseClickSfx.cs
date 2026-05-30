// GlobalMousePressReleaseSfxExposed.cs
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class GlobalMousePressReleaseSfxExposed : MonoBehaviour
{
    [Header("Mixer 라우팅")]
    [Tooltip("출력할 AudioMixerGroup (예: UI 또는 SFX 그룹)")]
    public AudioMixerGroup outputGroup;

    [Header("UI 영역 처리")]
    [Tooltip("마우스가 UI 위에 있을 때는 사운드를 내지 않음")]
    public bool ignoreWhenOverUI = false;

    [Header("감지할 버튼")]
    public bool useLeft = true;
    public bool useRight = false;
    public bool useMiddle = false;

    [System.Serializable]
    public class ClipSettings
    {
        [Tooltip("재생할 클립")]
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Tooltip("피치 랜덤 가감(±)")]
        [Range(0f, 0.2f)] public float pitchVariance = 0.03f;
        [Tooltip("연타 최소 간격(ms). 0이면 제한 없음")]
        [Range(0, 200)] public int minIntervalMs = 30;
    }

    [Header("누를 때(Down) 사운드")]
    public ClipSettings leftDown = new ClipSettings();
    public ClipSettings rightDown = new ClipSettings();
    public ClipSettings middleDown = new ClipSettings();

    [Header("뗄 때(Up) 사운드")]
    public ClipSettings leftUp = new ClipSettings();
    public ClipSettings rightUp = new ClipSettings();
    public ClipSettings middleUp = new ClipSettings();

    private AudioSource _src;
    private readonly Dictionary<string, double> _lastDsp = new Dictionary<string, double>();

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.spatialBlend = 0f; // 전역 UI성 피드백이므로 2D 권장
        if (outputGroup) _src.outputAudioMixerGroup = outputGroup;
    }

    void Update()
    {
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

        // 스팸 억제
        if (cfg.minIntervalMs > 0 && _lastDsp.TryGetValue(key, out var last))
        {
            double dtMs = (AudioSettings.dspTime - last) * 1000.0;
            if (dtMs < cfg.minIntervalMs) return;
        }
        _lastDsp[key] = AudioSettings.dspTime;

        // 피치 랜덤 적용
        float prevPitch = _src.pitch;
        _src.pitch = Mathf.Clamp(cfg.pitch + Random.Range(-cfg.pitchVariance, cfg.pitchVariance), 0.5f, 2f);

        _src.PlayOneShot(cfg.clip, cfg.volume);

        _src.pitch = prevPitch; // 복구
    }
}
