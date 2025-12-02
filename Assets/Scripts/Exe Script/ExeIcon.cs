using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExeIcon : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Components")]
    public Image iconImage;
    public TMP_Text exeNameText;

    private Exe exe;
    private FileWindow fileWindow;

    // 선택 텍스트 색상
    private Color normalColor = Color.white;
    private Color selectedColor = Color.yellow;

    // 더블클릭 처리
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f; // 0.3초 내 연속 클릭이면 더블클릭

    public void Setup(Exe exe, FileWindow window)
    {
        this.exe = exe;
        this.fileWindow = window;

        if (exeNameText != null)
            exeNameText.text = exe != null ? exe.name + ".exe" : "null.exe";

        if (iconImage != null && ExtensionManager.Instance != null)
            iconImage.sprite = ExtensionManager.Instance.GetIconForExtension("exe");

    }

    public Exe GetExe() => exe;

    public void SetSelected(bool selected)
    {
        if (exeNameText == null) return;
        exeNameText.color = selected ? selectedColor : normalColor;
    }

    // IPointerClickHandler 구현
    public void OnPointerClick(PointerEventData eventData)
    {
        // 한 번 클릭 → 선택
        FileWindow.Instance.SetSelectedExeIcon(this);

        // 더블클릭 판정
        if (Time.time - lastClickTime <= doubleClickThreshold)
        {
            // 더블클릭 발생
            if (ExeGameManager.Instance != null && exe != null)
            {
                ExeGameManager.Instance.RunExe(exe);
            }
            else
            {
                Debug.LogWarning("[ExeIcon] Exe 혹은 ExeGameManager가 준비되지 않았습니다.");
            }

            lastClickTime = 0f; // 초기화
        }
        else
        {
            lastClickTime = Time.time;
        }
    }
}
