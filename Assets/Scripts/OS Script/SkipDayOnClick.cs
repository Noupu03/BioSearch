using UnityEngine;
using UnityEngine.UI;       // ★ 추가
using TMPro;                // 이미 사용 중이면 유지
using System.Collections;

public class SkipDayOnClick : MonoBehaviour
{
    public OSTimeManager timeManager;
    public int targetHour = 8;
    public int targetMinute = 0;
    public int minutesPerFrame = 60; // 스킵 시 한 프레임당 흐르는 분

    // 연출용 화면
    private GameObject skipOverlay;
    public float fadeSpeed = 2f; // 페이드 속도

    private void OnMouseDown()
    {
        if (timeManager == null)
        {
            Debug.LogWarning("TimeManager 미설정!");
            return;
        }

        StartCoroutine(FastForwardToNextMorningWithOverlay());
    }

    private IEnumerator FastForwardToNextMorningWithOverlay()
    {
        // ==========================
        // 1. 화면 오버레이 생성
        // ==========================
        if (skipOverlay == null)
        {
            skipOverlay = new GameObject("SkipOverlay");
            var canvas = skipOverlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            skipOverlay.AddComponent<CanvasScaler>();
            skipOverlay.AddComponent<GraphicRaycaster>();

            // 검은 배경 이미지
            var imageGO = new GameObject("Image");
            imageGO.transform.SetParent(skipOverlay.transform, false);
            var image = imageGO.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0); // 초기 투명
            var rt = image.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var textGO = new GameObject("SkipText");
            textGO.transform.SetParent(skipOverlay.transform, false);
            var text = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "Skipping Day...";
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.fontSize = 48;
            var ret = text.GetComponent<RectTransform>();
            ret.anchorMin = Vector2.zero;
            ret.anchorMax = Vector2.one;
            ret.offsetMin = Vector2.zero;
            ret.offsetMax = Vector2.zero;
            canvas.sortingOrder = 100; // 다른 UI보다 위에 표시
        }


        var overlayImage = skipOverlay.GetComponentInChildren<UnityEngine.UI.Image>();
        var overlayText = skipOverlay.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        // ==========================
        // 2. 화면 점점 검게 만들기
        // ==========================
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            overlayImage.color = new Color(0, 0, 0, Mathf.Clamp01(alpha));
            overlayText.color = new Color(1, 1, 1, Mathf.Clamp01(alpha)); // 텍스트 알파 동기화
            yield return null;
        }

        // ==========================
        // 3. 시간 스킵
        // ==========================
        GameDateTime now = timeManager.GetCurrentGameTime();
        int nextDay = now.day + 1;
        int nextMonth = now.month;
        int nextYear = now.year;

        if (nextDay > 31)
        {
            nextDay = 1;
            nextMonth++;
            if (nextMonth > 12)
            {
                nextMonth = 1;
                nextYear++;
            }
        }

        GameDateTime target = new GameDateTime(nextYear, nextMonth, nextDay, targetHour, targetMinute);

        while (now < target)
        {
            now.minute += minutesPerFrame;
            now.NormalizeTime();

            timeManager.year = now.year;
            timeManager.month = now.month;
            timeManager.day = now.day;
            timeManager.hour = now.hour;
            timeManager.minute = now.minute;
            timeManager.UpdateDisplay();

            InputManager.Instance?.TriggerS(); // S키 강제 입력
            yield return null;
        }

        timeManager.year = target.year;
        timeManager.month = target.month;
        timeManager.day = target.day;
        timeManager.hour = target.hour;
        timeManager.minute = target.minute;
        timeManager.UpdateDisplay();
        InputManager.Instance?.TriggerS(); // 마지막 S키

        // ==========================
        // 4. 화면 점점 투명하게 만들기
        // ==========================
        yield return new WaitForSeconds(2f);
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            overlayImage.color = new Color(0, 0, 0, Mathf.Clamp01(alpha));
            overlayText.color = new Color(1, 1, 1, Mathf.Clamp01(alpha)); // 텍스트 알파 동기화
            yield return null;
        }

        Debug.Log($"[Skip Completed] 현재 시간: {target}");
    }
}
