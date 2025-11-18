using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 팝업 UI를 관리하는 매니저
/// - 파일 더블클릭 시 팝업 생성
/// - 중복 팝업 방지
/// - 확장자 변경 시 관련 팝업 자동 닫힘
/// </summary>
public class FilePopupManager : MonoBehaviour
{
    public static FilePopupManager Instance;

    [Header("Prefab & Canvas")]
    public GameObject popupPrefab;  // Popup 프리팹
    public Canvas canvas;           // Inspector에서 지정 가능

    // 현재 열린 팝업 목록 (파일 이름 기준)
    private Dictionary<string, GameObject> openPopups = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();
    }

    /// <summary>
    /// 파일 열기 - 팝업 생성 (중복 방지)
    /// </summary>
    public void OpenFile(File file)
    {
        if (file == null) return;

        if (canvas == null)
        {
            Debug.LogError("PopupManager: Canvas가 지정되지 않았습니다!");
            return;
        }

        if (popupPrefab == null)
        {
            Debug.LogError("PopupManager: popupPrefab이 지정되지 않았습니다!");
            return;
        }

        string popupKey = file.name;

        // 이미 같은 이름의 팝업이 열려 있다면 새로 생성하지 않음
        if (openPopups.ContainsKey(popupKey))
        {
            Debug.Log($"PopupManager: '{popupKey}' 팝업이 이미 열려 있습니다.");
            return;
        }

        // 팝업 생성
        GameObject popupInstance = Instantiate(popupPrefab, canvas.transform, false);
        if (popupInstance == null)
        {
            Debug.LogError("PopupManager: Popup 생성 실패!");
            return;
        }

        // 목록에 등록
        openPopups.Add(popupKey, popupInstance);

        // Popup 스크립트 가져오기
        FilePopup popupScript = popupInstance.GetComponent<FilePopup>();
        if (popupScript != null)
        {
            popupScript.Initialize(file.name, file.extension, canvas);
            popupScript.SetFileKey(popupKey); // 파일 이름 키 저장

            // 탑바 드래그 이벤트 연결 (이 부분이 탑바 드래그가 안 되던 원인 해결)
            if (popupScript.topBar != null)
            {
                EventTrigger trigger = popupScript.topBar.gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                    trigger = popupScript.topBar.gameObject.AddComponent<EventTrigger>();

                // 기존 트리거 초기화 (중복 연결 방지)
                trigger.triggers.Clear();

                // PointerDown
                EventTrigger.Entry entryDown = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown
                };
                entryDown.callback.AddListener((data) => popupScript.OnTopBarPointerDown((BaseEventData)data));
                trigger.triggers.Add(entryDown);

                // Drag
                EventTrigger.Entry entryDrag = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.Drag
                };
                entryDrag.callback.AddListener((data) => popupScript.OnTopBarDrag((BaseEventData)data));
                trigger.triggers.Add(entryDrag);
            }
            else
            {
                Debug.LogWarning("PopupManager: 팝업 프리팹의 topBar가 할당되지 않았습니다. 드래그 불가.");
            }
        }
        else
        {
            Debug.LogError("PopupManager: Popup 프리팹에 Popup 스크립트가 없습니다!");
            return;
        }

        // 생성 후 최상단으로
        popupInstance.transform.SetAsLastSibling();

        // 확장자에 따라 내용 표시
        Transform popupImage = FindDeepChild(popupInstance.transform, "PopupImage");
        Transform popupText = FindDeepChild(popupInstance.transform, "PopupText");

        if (popupImage == null || popupText == null)
        {
            Debug.LogError("PopupPrefab 안에 'PopupImage' 또는 'PopupText' 오브젝트가 없습니다!");
            return;
        }

        // 확장자별 표시
        string ext = file.extension != null ? file.extension.ToLower() : "";
        popupImage.gameObject.SetActive(false);
        popupText.gameObject.SetActive(false);

        switch (ext)
        {
            case "png":
            case "jpg":
            case "jpeg":
                popupImage.gameObject.SetActive(true);
                Image img = popupImage.GetComponent<Image>();
                if (img != null && file.imageContent != null)
                    img.sprite = file.imageContent;
                else
                    Debug.LogWarning($"{file.name}.{file.extension} : 이미지 내용 없음");
                break;

            case "txt":
                popupText.gameObject.SetActive(true);
                TMP_Text textComp = popupText.GetComponent<TMP_Text>();
                if (textComp != null)
                    textComp.text = file.textContent ?? $"{file.name}.{file.extension} (내용 없음)";
                else
                    Debug.LogWarning($"{file.name}.{file.extension} : TMP_Text 컴포넌트 없음");
                break;

            default:
                Debug.LogWarning($"{file.name}.{file.extension} : 지원되지 않는 파일 형식");
                break;
        }
        if (CheckList.Instance != null)
        {
            string fullName = file.name + "." + file.extension;

            // 파일 이름만 전달 (항목 리스트는 MessageSetup에서 전달)
            CheckList.Instance.ApplyStrikeForFile(file.name);
        }
        else
        {
            Debug.LogWarning("CheckList 인스턴스 없음. 체크리스트 갱신 실패.");
        }


    }

    /// <summary>
    /// 특정 이름의 팝업 닫기
    /// </summary>
    public void ClosePopup(string fileName)
    {
        if (openPopups.TryGetValue(fileName, out GameObject popup))
        {
            if (popup != null)
                Destroy(popup);
            openPopups.Remove(fileName);
        }
    }

    /// <summary>
    /// 팝업이 삭제될 때 매니저에서도 제거
    /// </summary>
    public void OnPopupDestroyed(string fileKey)
    {
        if (openPopups.ContainsKey(fileKey))
            openPopups.Remove(fileKey);
    }

    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        foreach (var popup in openPopups.Values)
        {
            if (popup != null) Destroy(popup);
        }
        openPopups.Clear();
    }

    /// <summary>
    /// 팝업 존재 여부 확인
    /// </summary>
    public bool IsPopupOpen(string fileName)
    {
        return openPopups.ContainsKey(fileName);
    }

    // 프리팹 내부 재귀 탐색
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
