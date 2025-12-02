using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SubmitProgress : MonoBehaviour
{
    [Header("UI")]
    public Slider progressBar;       // ← 인스펙터에서 받아오기
    public TMP_Text statusText;

    private bool isRunning = false;

    // 외부(SubmitWindow 등)에서 호출
    public void StartProgressBar()
    {
        if (!isRunning)
            StartCoroutine(FillProgress());
    }

    private IEnumerator FillProgress()
    {
        isRunning = true;

        float duration = 20f;   // 20초
        float elapsed = 0f;

        progressBar.value = 0f;
        if (statusText != null)
            statusText.text = "전송 중...";

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            progressBar.value = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        progressBar.value = 1f;

        if (statusText != null)
            statusText.text = "전송 완료!";

        isRunning = false;
    }
}
