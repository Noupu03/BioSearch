using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AutoScrollManager : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;
    public TMP_Text contentText;   // Content에 들어 있는 TMP_Text
    private RectTransform contentRt;

    [Header("Settings")]
    public bool autoScroll = true;     // 새 로그 입력 시 자동 스크롤 여부
    public float scrollDelay = 0.02f;  // 스크롤 타이밍 조절

    private bool userScrolling = false;
    private float lastHeight = -1f;

    void Awake()
    {
        contentRt = contentText.rectTransform;

        // 유저가 움직이면 자동 스크롤 해제
        scrollRect.onValueChanged.AddListener((v) =>
        {
            if (scrollRect.verticalNormalizedPosition > 0.01f)
                userScrolling = true;
        });
    }

    void LateUpdate()
    {
        UpdateContentHeight();

        // 자동 스크롤
        if (autoScroll && !userScrolling)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        // 1프레임 후 자동 스크롤 false
        autoScroll = false;

        // 매 프레임 초기화
        userScrolling = false;
    }

    // TMP 텍스트가 늘어나면 Content도 늘려줘야 함
    private void UpdateContentHeight()
    {
        float h = contentText.preferredHeight;

        if (Mathf.Abs(h - lastHeight) > 1f)
        {
            contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, h);
            lastHeight = h;
        }
    }

    // 외부에서 로그 추가할 때 호출
    public void AddLine(string msg)
    {
        contentText.text += msg + "\n";
        autoScroll = true;
    }

    // 필요하면 전체 리셋
    public void Clear()
    {
        contentText.text = "";
        autoScroll = true;
    }
}

