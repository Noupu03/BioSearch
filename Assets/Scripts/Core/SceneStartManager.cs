using UnityEngine;
using System.Collections;

/// <summary>
/// 씬 시작 시 GameEvents.OnSceneInitialized를 발생시켜
/// 각 매니저가 스스로 초기화하도록 위임한다.
///
/// RequestFirstStage는 한 프레임 뒤에 호출해
/// 모든 오브젝트의 Start()가 완료(이벤트 구독 완료)된 후 이벤트가 발사되도록 한다.
/// </summary>
public class SceneStartManager : MonoBehaviour
{
    IEnumerator Start()
    {
        GameEvents.RaiseSceneInitialized();
        yield return null; // 모든 Start() 완료 대기
        GameLoopManager.Instance?.RequestFirstStage();
        Debug.Log("[SceneStartManager] 씬 초기화 완료");
    }
}
