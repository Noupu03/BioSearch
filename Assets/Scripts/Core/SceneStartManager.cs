using UnityEngine;

/// <summary>
/// 씬 시작 시 GameEvents.OnSceneInitialized를 발생시켜
/// 각 매니저가 스스로 초기화하도록 위임한다.
/// </summary>
public class SceneStartManager : MonoBehaviour
{
    void Start()
    {
        GameEvents.RaiseSceneInitialized();
        Debug.Log("[SceneStartManager] 씬 초기화 이벤트 발생");
    }
}
