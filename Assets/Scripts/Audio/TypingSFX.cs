using UnityEngine;
using TMPro;
using System.Collections;
using Haare.Client.Routine;

[RequireComponent(typeof(AudioSource))]
public class TypingSFX : MonoRoutine
{
    [Header("Bindings")]
    public TMP_InputField inputField;

    [Header("Clips")]
    public AudioClip[] typingClips;
    public AudioClip enterClip;
    public AudioClip backspaceClip;

    [Header("Cooldowns (sec)")]
    public float typingCooldown = 0.07f;
    public float enterCooldown = 0.15f;
    public float backspaceCooldown = 0.07f;

    [Header("Variation")]
    [Range(0f, 0.5f)] public float pitchJitter = 0.05f;
    [Range(0f, 0.5f)] public float volumeJitter = 0.1f;

    [Header("Behavior")]
    public bool onlyOnKeyDown = true;
    public bool playBackspaceOnEmpty = true; // �� ��ĭ/��迡���� �齺���̽� SFX ���

    private AudioSource src;
    private int lastIndex = -1;

    private float lastTypingTime = -999f;
    private float lastEnterTime = -999f;
    private float lastBackspaceTime = -999f;

    private int lastTextLength = 0;
    private bool enterGuard = false;

    protected override void Constructor()
    {
        base.Constructor();
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;
    }

    void OnEnable()
    {
        if (inputField == null) return;
        inputField.onValueChanged.AddListener(OnTextChanged);
        inputField.onSubmit.AddListener(OnSubmit);
        lastTextLength = inputField.text?.Length ?? 0;
    }
    void OnDisable()
    {
        if (inputField == null) return;
        inputField.onValueChanged.RemoveListener(OnTextChanged);
        inputField.onSubmit.RemoveListener(OnSubmit);
    }

    protected override void UpdateProcess()
    {
        base.UpdateProcess();
        // ���� ���(backspace/delete) ���� ó�� ����
        if (!inputField || !inputField.isFocused) return;
        if (!string.IsNullOrEmpty(Input.compositionString)) return; // IME ���� �� ����

        // Backspace/Delete Ű�ٿ������� ���� �ؽ�Ʈ ��ȭ�� ���� ��Ȳ:
        bool backspaceDown = Input.GetKeyDown(KeyCode.Backspace);
        bool deleteDown = Input.GetKeyDown(KeyCode.Delete);

        if ((backspaceDown || deleteDown) && playBackspaceOnEmpty)
        {
            int len = inputField.text?.Length ?? 0;

            // Ŀ�� ��ġ (TMP 3.x: caretPosition ���; selection ������ anchor/focus ���� �� ���� Ŀ���� ����)
            int caret = inputField.caretPosition;

            bool atEmpty = len == 0;
            bool atLeftBoundary = caret <= 0 && backspaceDown;   // ���ʿ� ���� ���ڰ� ����
            bool atRightBoundary = caret >= len && deleteDown;    // �����ʿ� ���� ���ڰ� ����

            if (atEmpty || atLeftBoundary || atRightBoundary)
            {
                if (backspaceClip != null && Time.time - lastBackspaceTime >= backspaceCooldown)
                {
                    PlayOneShot(backspaceClip, 0.02f, 0.08f);
                    lastBackspaceTime = Time.time;
                }
            }
        }
    }

    void OnTextChanged(string newText)
    {
        if (!inputField.isFocused) { lastTextLength = newText.Length; return; }
        if (!string.IsNullOrEmpty(Input.compositionString)) { lastTextLength = newText.Length; return; }

        int delta = newText.Length - lastTextLength;
        lastTextLength = newText.Length;

        // ���� ������ onSubmit������ ó��
        if (delta > 0 && (newText.EndsWith("\n") || newText.EndsWith("\r")))
        {
            if (enterGuard) enterGuard = false;
            return;
        }

        // �ٿ��ֱ� ���� �Է� ����
        if (delta > 3) delta = 1;

        // onlyOnKeyDown ����: ���� Ű�ٿ� �����Ӹ� ���
        bool keyDownGate =
            !onlyOnKeyDown ||
            Input.anyKeyDown ||
            Input.GetKeyDown(KeyCode.V) || // �ٿ��ֱ� ����
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Backspace) ||
            Input.GetKeyDown(KeyCode.Delete);
        if (!keyDownGate) return;

        // ����(���� �ؽ�Ʈ ��ȭ�� �߻��� ���)
        if (delta < 0)
        {
            if (backspaceClip != null && (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)))
            {
                if (Time.time - lastBackspaceTime >= backspaceCooldown)
                {
                    PlayOneShot(backspaceClip, 0.02f, 0.08f);
                    lastBackspaceTime = Time.time;
                }
            }
            return;
        }

        // �Ϲ� Ÿ����
        if (delta > 0 && Time.time - lastTypingTime >= typingCooldown)
        {
            PlayTypingOneShot();
            lastTypingTime = Time.time;
        }
    }

    void OnSubmit(string _)
    {
        if (enterClip == null) return;
        if (Time.time - lastEnterTime < enterCooldown) return;

        enterGuard = true;
        PlayOneShot(enterClip, 0f, 0f, fixedPitch: 1f, fixedVol: 1f);
        lastEnterTime = Time.time;
        StartCoroutine(ClearEnterGuardNextFrame());
    }

    IEnumerator ClearEnterGuardNextFrame()
    {
        yield return null; // 1������ �� ���� ���� ó�� ����
        enterGuard = false;
    }

    void PlayTypingOneShot()
    {
        if (typingClips == null || typingClips.Length == 0) return;
        int index;
        do { index = Random.Range(0, typingClips.Length); }
        while (index == lastIndex && typingClips.Length > 1);
        lastIndex = index;
        PlayOneShot(typingClips[index], pitchJitter, volumeJitter);
    }

    void PlayOneShot(AudioClip clip, float pitchJit, float volJit, float fixedPitch = -1f, float fixedVol = -1f)
    {
        src.pitch = (fixedPitch < 0f) ? 1f + Random.Range(-pitchJit, pitchJit) : fixedPitch;
        src.volume = (fixedVol < 0f) ? 1f - Random.Range(0f, volJit) : fixedVol;
        src.PlayOneShot(clip);
    }
}
