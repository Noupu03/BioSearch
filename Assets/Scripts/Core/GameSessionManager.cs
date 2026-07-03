using UnityEngine;
using Haare.Client.Routine;

/// <summary>
/// 씬 전환 사이에도 유지되는 세션 오브젝트.
/// 점수 데이터는 ScoreCount(static)가 담당하므로 이 클래스는 생존만 관리한다.
/// </summary>
public class GameSessionManager : MonoRoutine
{
    public static GameSessionManager Instance { get; private set; }

    protected override void Constructor()
    {
        base.Constructor();
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // MonoRoutine도 private OnDestroy()를 정의하므로(Awake와 같은 문제), 대신 OnDisable 사용.
    void OnDisable() { if (Instance == this) Instance = null; }
}
