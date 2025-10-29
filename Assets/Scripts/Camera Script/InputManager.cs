using UnityEngine;
using System;
using TMPro;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool APressed { get; private set; }
    public bool DPressed { get; private set; }

    public bool IsSwitchingLocked { get; private set; } // ��ȯ ������ ����

    public TMP_InputField blockSPressedField;

    public event Action OnWPressed;
    public event Action OnSPressed;
    public event Action OnADChanged;

    public float wSwitchDelay = 0.25f; // W ��ȯ ���� S ���� �ð�

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
