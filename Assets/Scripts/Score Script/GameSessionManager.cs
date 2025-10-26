using UnityEngine;

/// <summary>
/// 씬을 넘어가도 점수(성공/실패) 유지
/// - 싱글톤
/// - DontDestroyOnLoad 적용
/// </summary>
public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance;

    [Header("Score")]
    public int UIsuccessCount = 0;
    public int UIfailCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 Reload 시 유지
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }
}
