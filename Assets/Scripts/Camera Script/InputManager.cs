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
        // A/D ���� üũ
        bool prevA = APressed;
        bool prevD = DPressed;
        APressed = Input.GetKey(KeyCode.A);
        DPressed = Input.GetKey(KeyCode.D);
        if (prevA != APressed || prevD != DPressed)
            OnADChanged?.Invoke();

        // S ���� üũ
        SPressed = Input.GetKey(KeyCode.S);
        if (Input.GetKeyDown(KeyCode.S))
            OnSPressed?.Invoke();

        // WŰ �̺�Ʈ: �ݵ�� S ���� + A/D ���Է�
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (SPressed && !APressed && !DPressed)
            {
                OnWPressed?.Invoke();
            }
            else
            {
                Debug.Log("W �Է� ���õ�: S ����=" + SPressed + ", A=" + APressed + ", D=" + DPressed);
            }
        }
    }
}
