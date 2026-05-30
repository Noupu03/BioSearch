using UnityEngine;
using System;
using TMPro;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool APressed { get; private set; }
    public bool DPressed { get; private set; }

    public bool IsSwitchingLocked { get; private set; } // 전환 중인지 여부

    public TMP_InputField blockSPressedField;

    public event Action OnWPressed;
    public event Action OnSPressed;
    public event Action OnADChanged;

    public float wSwitchDelay = 0.25f; // W 전환 이후 S 차단 시간

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void LockSInput(bool locked)
    {
        IsSwitchingLocked = locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            // 인풋필드 포커스 중 차단
            if (blockSPressedField != null && blockSPressedField.isFocused) return;
            // W 전환 딜레이 중 차단
            if (IsSwitchingLocked) return;

            OnSPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            // W 누르는 순간 차단 시작
            LockSInput(true);
            OnWPressed?.Invoke();
            StartCoroutine(UnlockSAfterDelay()); // 딜레이 시작
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
