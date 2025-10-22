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
    public int successCount = 0;
    public int failCount = 0;

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
