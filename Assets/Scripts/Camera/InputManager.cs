using UnityEngine;
using System;
using TMPro;

public class InputManager : MonoBehaviour
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

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

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
