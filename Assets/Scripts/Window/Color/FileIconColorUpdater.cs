using UnityEngine;
using TMPro;

/// <summary>
/// 이상 폴더 아이콘 텍스트에 GlobalColorManager의 이상 색상을 적용한다.
/// GlobalColorManager는 Instance로 접근하므로 FindObjectOfType 없음.
/// </summary>
public class FileIconColorUpdater : MonoBehaviour
{
    private FolderIcon folderIcon;
    private TMP_Text   text;

    void Awake()
    {
        folderIcon = GetComponent<FolderIcon>();
        text       = GetComponentInChildren<TMP_Text>();
    }

    void Start() => UpdateColor();

    public void UpdateColor()
    {
        if (folderIcon == null || text == null) return;

        Folder folder = folderIcon.GetFolder();
        if (folder != null && folder.isAbnormal && GlobalColorManager.Instance != null)
            text.color = GlobalColorManager.Instance.abnormalFolderTextColor;
    }
}
