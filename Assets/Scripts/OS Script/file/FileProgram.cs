using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FileProgram : MonoBehaviour
{
    [Header("UI 연결용 컴포넌트")]
    public Transform contentArea;
    public TMP_Text emptyText;
    public PathPanelManager pathPanelManager;
    public Button backButton;

    void Start()
    {
        // FileWindow 찾아서 자동 연결
        FileWindow fileWindow = FindObjectOfType<FileWindow>();
        if (fileWindow != null)
        {
            fileWindow.LinkWithProgram(this);
            fileWindow.FileExplorerInitialize();
        }
        else
        {
            Debug.LogError("[FileProgram] FileWindow를 찾을 수 없습니다.");
        }
    }
}
