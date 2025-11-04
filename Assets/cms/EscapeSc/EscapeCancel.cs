using UnityEngine;

public class EscapeCancel : MonoBehaviour
{
    public GameStateManager gameStateManager;

    void OnMouseDown()
    {
        if (gameStateManager == null)
        {
            Debug.LogWarning("[EscapeCancel] GameStateManager가 할당되어 있지 않습니다!");
            return;
        }

        // 언제든지 탈주 취소를 요청 (GameStateManager에서 안전 처리)
        gameStateManager.RequestCancelEscape();
    }
}
