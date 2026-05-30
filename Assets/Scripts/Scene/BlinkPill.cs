using UnityEngine;
using System.Collections;

/// <summary>
/// 클릭 시 깜빡이고 연결된 UI 패널을 축소하는 알약 오브젝트.
/// </summary>
public class BlinkPill : BlinkBehaviour
{
    [Header("UI 축소")]
    [SerializeField] private RectTransform uiPanel;
    [SerializeField] private float         shrinkTarget   = 0.8f;
    [SerializeField] private float         shrinkDuration = 0.2f;

    public void OnClickPill()
    {
        StartBlink();
        if (uiPanel != null)
            StartCoroutine(ShrinkUI());
    }

    private IEnumerator ShrinkUI()
    {
        Vector3 original = uiPanel.localScale;
        Vector3 target   = original * shrinkTarget;
        float   t        = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / shrinkDuration;
            uiPanel.localScale = Vector3.Lerp(original, target, t);
            yield return null;
        }
        uiPanel.localScale = target;
    }
}
