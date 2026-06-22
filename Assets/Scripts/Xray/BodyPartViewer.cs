using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Xray 패널 코디네이터. 각 신체 버튼(BodyPartButton)이 Start에서 자기 자신을
/// Register()로 등록하므로 Inspector에서 버튼 목록을 수동 할당할 필요가 없다.
/// </summary>
public class XRayPanel : MonoBehaviour
{
    [Header("카메라창")]
    public Image cameraPanel;

    private readonly List<BodyPartButton> parts = new();
    private BodyPartButton currentPart;

    public void Register(BodyPartButton btn)
    {
        if (!parts.Contains(btn))
            parts.Add(btn);
    }

    public void ShowPart(BodyPartButton btn)
    {
        // 재클릭 시 선택 해제
        if (currentPart == btn)
        {
            currentPart.SetHighlight(false);
            currentPart = null;
            return;
        }

        currentPart?.SetHighlight(false);
        currentPart = btn;
        btn.SetHighlight(true);

        UpdateCameraPanel(btn);
    }

    private void UpdateCameraPanel(BodyPartButton btn)
    {
        if (cameraPanel == null) return;

        var fw = FileWindow.Instance;
        if (fw == null) return;

        string[] path = btn.FolderPath;
        if (path == null || path.Length == 0) return;

        Folder target = fw.GetRootFolder();
        foreach (var node in path)
        {
            if (target == null) break;
            Folder next = null;
            foreach (var child in target.children)
                if (child.name == node) { next = child; break; }
            target = next;
        }

        if (target == null)
        {
            cameraPanel.sprite = btn.NormalSprite;
            return;
        }

        bool isAbnormal    = AbnormalDetector.HasAbnormal(target);
        cameraPanel.sprite = (isAbnormal && btn.AbnormalSprite != null)
            ? btn.AbnormalSprite
            : btn.NormalSprite;
    }
}
