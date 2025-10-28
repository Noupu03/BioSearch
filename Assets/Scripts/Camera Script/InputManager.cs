using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool APressed { get; private set; }
    public bool DPressed { get; private set; }

    public event Action OnWPressed;
    public event Action OnSPressed;
    public event Action OnADChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // S키 이벤트
        if (Input.GetKeyDown(KeyCode.S))
            OnSPressed?.Invoke();

        // W키 이벤트 (항상 발생)
        if (Input.GetKeyDown(KeyCode.W))
            OnWPressed?.Invoke();

        // A/D 상태 체크
        bool prevA = APressed;
        bool prevD = DPressed;
        APressed = Input.GetKey(KeyCode.A);
        DPressed = Input.GetKey(KeyCode.D);

        if (prevA != APressed || prevD != DPressed)
            OnADChanged?.Invoke();
    }
}
