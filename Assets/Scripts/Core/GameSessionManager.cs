using UnityEngine;

/// <summary>
/// 씬 전환 사이에도 유지되는 세션 오브젝트.
/// 점수 데이터는 ScoreCount(static)가 담당하므로 이 클래스는 생존만 관리한다.
/// </summary>
public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
