using UnityEngine;
using System.Collections;

public class BlinkPill : MonoBehaviour
{
    private Renderer rend;
    public Color blinkColor = Color.black;
    public float blinkDuration = 0.1f;
    public int blinkCount = 1;
    private Color originalColor;

    [SerializeField] private RectTransform uiPanel; // UI 연결

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void OnClickPill()
    {
        StartCoroutine(Blink());
        StartCoroutine(ShrinkUI());
    }

    private IEnumerator Blink()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            rend.material.color = blinkColor;
            yield return new WaitForSeconds(blinkDuration);
            rend.material.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private IEnumerator ShrinkUI()
    {
        Vector3 originalScale = uiPanel.localScale;
        Vector3 targetScale = originalScale * 0.8f; // 예: 80%로 줄이기
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f; // 0.2초 동안 줄어듦
            uiPanel.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
    }
}
