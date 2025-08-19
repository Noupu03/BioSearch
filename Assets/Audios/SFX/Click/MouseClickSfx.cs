// GlobalMouseClickSfxExposed.cs
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MouseClickSfx : MonoBehaviour
{
    [Header("Ŭ�� ����")]
    [Tooltip("Ŭ�� �� ����� Ŭ��")]
    public AudioClip clickClip;

    [Tooltip("��Ŭ���� �������� ���� (false�� ��/��Ŭ���� ����)")]
    public bool leftClickOnly = true;

    [Header("Mixer �����")]
    [Tooltip("����� AudioMixerGroup (��: UI �Ǵ� SFX �׷�)")]
    public AudioMixerGroup outputGroup;

    [Header("����/��ġ ����")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float pitch = 1f;
    [Tooltip("��ġ ���� ���� ���� (��)")]
    [Range(0f, 0.2f)] public float pitchVariance = 0.03f;

    [Header("���� ����")]
    [Tooltip("���� Ŭ�� �� �ּ� ���� (�и���). 0�̸� ���� ����")]
    [Range(0, 200)] public int minIntervalMs = 40;

    private AudioSource source;
    private double lastPlayDsp = -1;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D ���� (���� ����)
        if (outputGroup) source.outputAudioMixerGroup = outputGroup;
    }

    void Update()
    {
        if (leftClickOnly)
        {
            if (Input.GetMouseButtonDown(0)) TryPlay();
        }
        else
        {
            if (Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1) ||
                Input.GetMouseButtonDown(2))
                TryPlay();
        }
    }

    void TryPlay()
    {
        if (clickClip == null) return;

        // ���� ����
        if (minIntervalMs > 0 && lastPlayDsp > 0)
        {
            double dtMs = (AudioSettings.dspTime - lastPlayDsp) * 1000.0;
            if (dtMs < minIntervalMs) return;
        }
        lastPlayDsp = AudioSettings.dspTime;

        // ��ġ ���� ����
        float prevPitch = source.pitch;
        source.pitch = Mathf.Clamp(pitch + Random.Range(-pitchVariance, pitchVariance), 0.5f, 2f);

        // ���
        source.PlayOneShot(clickClip, volume);

        // ��ġ ����
        source.pitch = prevPitch;
    }
}
