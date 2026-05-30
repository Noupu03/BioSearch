using UnityEngine;

/// <summary>
/// 파일 확장자에 맞는 아이콘 스프라이트를 반환하는 싱글톤 서비스.
/// </summary>
public class ExtensionManager : MonoBehaviour
{
    public static ExtensionManager Instance { get; private set; }

    [Header("확장자별 아이콘")]
    [SerializeField] private Sprite txtIconSprite;
    [SerializeField] private Sprite pngIconSprite;
    [SerializeField] private Sprite defaultIconSprite;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public Sprite GetIconForExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension)) return defaultIconSprite;

        return extension.ToLower() switch
        {
            "txt"  => txtIconSprite,
            "png"  => pngIconSprite,
            "jpg"  => pngIconSprite,
            "jpeg" => pngIconSprite,
            _      => defaultIconSprite,
        };
    }
}
