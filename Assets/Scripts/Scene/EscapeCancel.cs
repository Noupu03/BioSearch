using UnityEngine;

/// <summary>
/// 클릭 시 진행 중인 탈출 시도를 취소한다.
/// GameStateManager는 Instance로 접근하므로 인스펙터 참조 불필요.
/// </summary>
public class EscapeCancel : MonoBehaviour
{
    void OnMouseDown()
    {
        GameStateManager.Instance?.RequestCancelEscape();
    }
}
