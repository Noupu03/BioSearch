using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public bool APressed { get; private set; }
    public bool DPressed { get; private set; }
    public bool SPressed { get; private set; }

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
        // A/D 상태 체크
        bool prevA = APressed;
        bool prevD = DPressed;
        APressed = Input.GetKey(KeyCode.A);
        DPressed = Input.GetKey(KeyCode.D);
        if (prevA != APressed || prevD != DPressed)
            OnADChanged?.Invoke();

        // S 상태 체크
        SPressed = Input.GetKey(KeyCode.S);
        if (Input.GetKeyDown(KeyCode.S))
            OnSPressed?.Invoke();

        // W키 이벤트: 반드시 S 상태 + A/D 미입력
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (SPressed && !APressed && !DPressed)
            {
                OnWPressed?.Invoke();
            }
            else
            {
                Debug.Log("W 입력 무시됨: S 상태=" + SPressed + ", A=" + APressed + ", D=" + DPressed);
            }
        }
    }
}
