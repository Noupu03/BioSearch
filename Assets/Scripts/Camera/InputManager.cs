using UnityEngine;
using System;
using TMPro;
using Haare.Client.Routine;

public class InputManager : MonoRoutine
{
    public static InputManager Instance { get; private set; }

    public bool APressed           { get; private set; }
    public bool DPressed           { get; private set; }
    public bool IsSwitchingLocked  { get; private set; }

    [SerializeField] private TMP_InputField blockSPressedField;
    [SerializeField] private float          wSwitchDelay = 0.25f;

    public event Action OnWPressed;
    public event Action OnSPressed;
    public event Action OnADChanged;

    protected override void Constructor()
    {
        base.Constructor();
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // MonoRoutine도 private OnDestroy()를 정의하므로(Awake와 같은 문제), 대신 OnDisable 사용.
    void OnDisable() { if (Instance == this) Instance = null; }

    public void LockSInput(bool locked)
    {
        IsSwitchingLocked = locked;
    }

    /// <summary>S키 입력을 강제로 발생. 잠금 무시 (씬 초기화용).</summary>
    public void SimulateSPress() => OnSPressed?.Invoke();

    protected override void UpdateProcess()
    {
        base.UpdateProcess();
        if (Input.GetKeyDown(KeyCode.S))
        {
            // ��ǲ�ʵ� ��Ŀ�� �� ����
            if (blockSPressedField != null && blockSPressedField.isFocused) return;
            // W ��ȯ ������ �� ����
            if (IsSwitchingLocked) return;

            OnSPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            // W ������ ���� ���� ����
            LockSInput(true);
            OnWPressed?.Invoke();
            StartCoroutine(UnlockSAfterDelay()); // ������ ����
        }

        bool prevA = APressed;
        bool prevD = DPressed;
        APressed = Input.GetKey(KeyCode.A);
        DPressed = Input.GetKey(KeyCode.D);

        if (prevA != APressed || prevD != DPressed)
            OnADChanged?.Invoke();
    }

    private System.Collections.IEnumerator UnlockSAfterDelay()
    {
        yield return new WaitForSeconds(wSwitchDelay);
        LockSInput(false);
    }
}
