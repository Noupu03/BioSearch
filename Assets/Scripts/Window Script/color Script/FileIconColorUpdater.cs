using UnityEngine;
using TMPro;

// FileIcon의 텍스트 색상을 이상 폴더 여부에 따라 자동 적용
public class FileIconColorUpdater : MonoBehaviour
{
    private FolderIcon fileIcon;
    private TMP_Text text;
    private GlobalColorManager gcm;

    void Awake()
    {
        fileIcon = GetComponent<FolderIcon>();
        text = GetComponent<TMP_Text>();
        gcm = FindObjectOfType<GlobalColorManager>();
    }

    void Start()
    {
        UpdateColor();
    }

    public void UpdateColor()
    {
        if (fileIcon == null || text == null || gcm == null) return;

        Folder folder = fileIcon.GetFolder();
        if (folder != null && folder.isImportant)
            text.color = gcm.abnormalFolderTextColor;
    }
}
