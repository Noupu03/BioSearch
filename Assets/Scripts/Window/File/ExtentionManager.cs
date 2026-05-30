using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 파일 확장자에 따라 아이콘 스프라이트를 관리하는 매니저
/// </summary>
public class ExtensionManager : MonoBehaviour
{
    public static ExtensionManager Instance;

    [Header("아이콘 스프라이트")]
    public Sprite txtIconSprite;
    public Sprite pngIconSprite;
    public Sprite defaultIconSprite; // 정의되지 않은 확장자용 기본 아이콘

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 확장자에 맞는 아이콘 스프라이트를 반환
    /// </summary>
    public Sprite GetIconForExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return defaultIconSprite;

        switch (extension.ToLower())
        {
            case "txt":
                return txtIconSprite;
            case "png":
                return pngIconSprite;
            default:
                return defaultIconSprite;
        }
    }
}
