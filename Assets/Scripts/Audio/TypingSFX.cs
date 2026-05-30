using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TypingSFX : MonoBehaviour
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
    public bool playBackspaceOnEmpty = true; // ← 빈칸/경계에서도 백스페이스 SFX 재생

    private AudioSource src;
    private int lastIndex = -1;

    private float lastTypingTime = -999f;
    private float lastEnterTime = -999f;
    private float lastBackspaceTime = -999f;

    private int lastTextLength = 0;
    private bool enterGuard = false;

    void Awake()
    {
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

    void Update()
    {
        // ── 경계(backspace/delete) 전용 처리 ──
        if (!inputField || !inputField.isFocused) return;
        if (!string.IsNullOrEmpty(Input.compositionString)) return; // IME 조합 중 무시

        // Backspace/Delete 키다운이지만 실제 텍스트 변화가 없을 상황:
        bool backspaceDown = Input.GetKeyDown(KeyCode.Backspace);
        bool deleteDown = Input.GetKeyDown(KeyCode.Delete);

        if ((backspaceDown || deleteDown) && playBackspaceOnEmpty)
        {
            int len = inputField.text?.Length ?? 0;

            // 커서 위치 (TMP 3.x: caretPosition 사용; selection 고려해 anchor/focus 동일 시 단일 커서로 간주)
            int caret = inputField.caretPosition;

            bool atEmpty = len == 0;
            bool atLeftBoundary = caret <= 0 && backspaceDown;   // 왼쪽에 지울 문자가 없음
            bool atRightBoundary = caret >= len && deleteDown;    // 오른쪽에 지울 문자가 없음

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

        // 엔터 개행은 onSubmit에서만 처리
        if (delta > 0 && (newText.EndsWith("\n") || newText.EndsWith("\r")))
        {
            if (enterGuard) enterGuard = false;
            return;
        }

        // 붙여넣기 과다 입력 억제
        if (delta > 3) delta = 1;

        // onlyOnKeyDown 가드: 실제 키다운 프레임만 재생
        bool keyDownGate =
            !onlyOnKeyDown ||
            Input.anyKeyDown ||
            Input.GetKeyDown(KeyCode.V) || // 붙여넣기 대응
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Backspace) ||
            Input.GetKeyDown(KeyCode.Delete);
        if (!keyDownGate) return;

        // 삭제(실제 텍스트 변화가 발생한 경우)
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

        // 일반 타이핑
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
        yield return null; // 1프레임 후 개행 무음 처리 종료
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
