using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 통합된 파일 아이콘
/// - 확장자별 아이콘 이미지를 ExtensionManager에서 가져옴
/// - 더블클릭 시 PopupManager 통해 파일 열림
/// - 드래그 앤 드롭 지원
/// - isImportant 여부에 따라 텍스트 색상 변경
/// </summary>
public class FileIcon : MonoBehaviour, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [Header("UI Components")]
    public Image iconImage;          // 아이콘 이미지
    public TMP_Text fileNameText;    // 파일 이름 표시

    public Image checkMarkImage;     // 체크 표시 이미지 (Inspector에서 연결)


    private FileWindow fileWindow;
    private File file;

    // 기본 색상
    private Color normalColor = Color.white;   // 정상 파일: 하양
    private Color abnormalColor = Color.red;   // 이상 파일: 빨강
    private Color selectedColor = Color.yellow;
    // 추가됨: 체크 상태 색상
    private Color checkedColor = Color.green;


    bool isautoed = true;
    /// <summary>
    /// 아이콘 초기화
    /// </summary>
    /// 

    public string FileName
    {
        get { return file != null ? file.name : string.Empty; }
    }

    public void Setup(File File, FileWindow window)
    {
        file = File;
        fileWindow = window;

        if (fileNameText != null)
            fileNameText.text = $"{file.name}.{file.extension}";

        if (iconImage != null && ExtensionManager.Instance != null)
            iconImage.sprite = ExtensionManager.Instance.GetIconForExtension(file.extension);

        /*  파일 이상 여부 반영(빨간색)
        if (isautoed == true)
        {
            if (fileNameText != null)
                fileNameText.color = file.isImportant ? abnormalColor : normalColor;
        }*/

        SetSelected(false);
        // 추가됨: 체크표시 초기 상태 반영
        ApplyCheckStateUI();
    }

    public File GetFile() => file;

    public void SetSelected(bool selected)
    {
        if (fileNameText == null) return;

        if (selected)
        {
            fileNameText.color = selectedColor;
        }
        else
        {
            //  선택 해제 시 다시 이상 여부 반영
            fileNameText.color = file.isImportant ? abnormalColor : normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        fileWindow.SetSelectedFileIcon(this);

        // 추가됨: 우클릭 시 체크 토글
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ToggleCheckState();
            return;
        }

        // 기존 더블클릭 파일 열기
        if (eventData.clickCount == 2)
        {
            if (FilePopupManager.Instance != null && file != null)
                FilePopupManager.Instance.OpenFile(file);
        }
    }
    private void ToggleCheckState()
    {
        file.isChecked = !file.isChecked;
        ApplyCheckStateUI();
    }

    private void ApplyCheckStateUI()
    {
        if (checkMarkImage != null)
            checkMarkImage.gameObject.SetActive(file.isChecked);

        if (iconImage != null)
        {
            if (file.isChecked)
            {
                iconImage.color = checkedColor;
            }
            else
            {
                iconImage.color = Color.white;
            }
        }
    }

    #region 드래그 구현

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (FolderDragManager.Instance != null)
            FolderDragManager.Instance.BeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (FolderDragManager.Instance != null)
            FolderDragManager.Instance.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (FolderDragManager.Instance != null)
            FolderDragManager.Instance.EndDrag();
    }

    // (선택적으로 버튼에서 직접 호출할 수 있는 메서드 제공)
    public void OnClickIconButton()
    {
        // UI Button에서 이 메서드를 연결하면 동일 동작
        fileWindow.SetSelectedFileIcon(this);
    }

    #endregion
    /* FileIcon.cs
    public void SetupDummy(string dummyName)
    {
        if (fileNameText != null)
            fileNameText.text = dummyName;

        // 클릭/드래그 비활성화
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = false;
    }삭제예정
    */
}
