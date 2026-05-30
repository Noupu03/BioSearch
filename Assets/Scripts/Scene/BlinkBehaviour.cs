using UnityEngine;
using System.Collections;

/// <summary>
/// 렌더러 색상을 깜빡이는 공통 동작. BlinkObject와 BlinkPill이 상속한다.
/// </summary>
public abstract class BlinkBehaviour : MonoBehaviour
{
    [Header("깜빡임 설정")]
    [SerializeField] protected Color blinkColor    = Color.red;
    [SerializeField] protected float blinkDuration = 0.1f;
    [SerializeField] protected int   blinkCount    = 3;

    protected Renderer rend;
    private   Color    originalColor;

    protected virtual void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    public void StartBlink()
    {
        if (rend == null) return;
        StopAllCoroutines();
        StartCoroutine(Blink());
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
}
