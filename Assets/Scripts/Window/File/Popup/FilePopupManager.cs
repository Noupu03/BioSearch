using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 파일 팝업 창을 관리하는 싱글톤.
/// Canvas는 컴포넌트 계층에서 자동으로 찾으므로 인스펙터 참조 불필요.
/// </summary>
public class FilePopupManager : MonoBehaviour
{
    public static FilePopupManager Instance { get; private set; }

    [Header("팝업 프리팹")]
    public GameObject popupPrefab;

    private Canvas canvas;
    private readonly Dictionary<string, GameObject> openPopups = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        canvas   = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("[FilePopupManager] 부모 계층에 Canvas가 없습니다.");
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public void OpenFile(File file)
    {
        if (file == null || canvas == null || popupPrefab == null) return;
        if (openPopups.ContainsKey(file.name)) return;

        var go     = Instantiate(popupPrefab, canvas.transform, false);
        var popup  = go.GetComponent<FilePopup>();

        openPopups[file.name] = go;

        if (popup != null)
        {
            popup.Initialize(file.name, file.extension, canvas);
            popup.SetFileKey(file.name);
        }

        go.transform.SetAsLastSibling();
        SetupContent(go, file);
    }

    public void ClosePopup(string fileName)
    {
        if (!openPopups.TryGetValue(fileName, out var go)) return;
        if (go != null) Destroy(go);
        openPopups.Remove(fileName);
    }

    public void OnPopupDestroyed(string fileKey) => openPopups.Remove(fileKey);

    public void CloseAllPopups()
    {
        foreach (var go in openPopups.Values)
            if (go != null) Destroy(go);
        openPopups.Clear();
    }

    public bool IsPopupOpen(string fileName) => openPopups.ContainsKey(fileName);

    // ── 내부 헬퍼 ────────────────────────────────
    // 드래그 처리는 FilePopup.Initialize() 내에서 DragHandler 컴포넌트가 담당한다.

    private static void SetupContent(GameObject go, File file)
    {
        var popupImage = FindDeepChild(go.transform, "PopupImage");
        var popupText  = FindDeepChild(go.transform, "PopupText");
        if (popupImage == null || popupText == null) return;

        popupImage.gameObject.SetActive(false);
        popupText.gameObject.SetActive(false);

        string ext = (file.extension ?? "").ToLower();
        switch (ext)
        {
            case "png": case "jpg": case "jpeg":
                popupImage.gameObject.SetActive(true);
                var img = popupImage.GetComponent<Image>();
                if (img != null && file.imageContent != null)
                    img.sprite = file.imageContent;
                break;

            case "txt":
                popupText.gameObject.SetActive(true);
                var txt = popupText.GetComponent<TMP_Text>();
                if (txt != null)
                    txt.text = file.textContent ?? $"{file.name}.{file.extension}";
                break;
        }
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
