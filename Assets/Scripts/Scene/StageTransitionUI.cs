using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Haare.Client.Routine;

/// <summary>
/// 스테이지 전환 연출 (검은 화면 페이드 + 메시지).
/// GameLoopManager.Constructor()에서 AddComponent로 생성되며,
/// 씬에 별도 오브젝트를 만들 필요 없다.
/// </summary>
public class StageTransitionUI : MonoRoutine
{
    private CanvasGroup     overlay;
    private TextMeshProUGUI messageText;

    private float  fadeDuration;
    private float  holdDuration;
    private string message;

    /// <summary>GameLoopManager에서 생성 직후 호출.</summary>
    public void Setup(float fade, float hold, string msg)
    {
        fadeDuration = fade;
        holdDuration = hold;
        message      = msg;
        CreateUI();
    }

    private void CreateUI()
    {
        // ── Canvas (Screen Space Overlay, 항상 최상단) ──────────
        var canvasGO = new GameObject("_Canvas");
        canvasGO.transform.SetParent(transform, false);

        var canvas          = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── 전체화면 검정 패널 ───────────────────────────────────
        var panelGO = new GameObject("_BlackOverlay");
        panelGO.transform.SetParent(canvasGO.transform, false);

        panelGO.AddComponent<Image>().color = Color.black;

        var panelRT    = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        overlay                = panelGO.AddComponent<CanvasGroup>();
        overlay.alpha          = 0f;
        overlay.blocksRaycasts = false;

        // ── 메시지 텍스트 ────────────────────────────────────────
        var textGO = new GameObject("_MessageText");
        textGO.transform.SetParent(panelGO.transform, false);

        messageText           = textGO.AddComponent<TextMeshProUGUI>();
        messageText.text      = message;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontSize  = 36;
        messageText.color     = Color.white;
        messageText.gameObject.SetActive(false);

        var textRT    = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.1f, 0.4f);
        textRT.anchorMax = new Vector2(0.9f, 0.6f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    /// <summary>검은 화면 페이드 인 후 메시지를 holdDuration 동안 표시.</summary>
    public IEnumerator FadeToBlack()
    {
        overlay.blocksRaycasts = true;
        yield return FadeTo(1f);

        messageText.text = message;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(holdDuration);
    }

    /// <summary>메시지를 숨기고 검은 화면에서 페이드 아웃.</summary>
    public IEnumerator FadeFromBlack()
    {
        messageText.gameObject.SetActive(false);
        yield return FadeTo(0f);
        overlay.blocksRaycasts = false;
    }

    private IEnumerator FadeTo(float target)
    {
        float start = overlay.alpha;
        float t     = 0f;
        while (t < 1f)
        {
            t             += Time.deltaTime / fadeDuration;
            overlay.alpha  = Mathf.Lerp(start, target, Mathf.Clamp01(t));
            yield return null;
        }
        overlay.alpha = target;
    }
}
