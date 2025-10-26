using UnityEngine;

/// <summary>
/// ���� �Ѿ�� ����(����/����) ����
/// - �̱���
/// - DontDestroyOnLoad ����
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
            DontDestroyOnLoad(gameObject); // �� Reload �� ����
        }
        else
        {
            Destroy(gameObject); // �ߺ� ����
        }
    }
}
