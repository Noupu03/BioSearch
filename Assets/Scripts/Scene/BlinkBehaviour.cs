using UnityEngine;
using System.Collections;
using Haare.Client.Routine;

/// <summary>
/// 렌더러 색상을 깜빡이는 공통 동작. BlinkObject와 BlinkPill이 상속한다.
/// </summary>
public abstract class BlinkBehaviour : MonoRoutine
{
    [Header("깜빡임 설정")]
    [SerializeField] protected Color blinkColor    = Color.red;
    [SerializeField] protected float blinkDuration = 0.1f;
    [SerializeField] protected int   blinkCount    = 3;

    protected Renderer rend;
    private   Color    originalColor;

    // 하위 클래스가 초기화를 추가하려면 Awake가 아니라 이 메서드를 override해야 한다
    // (MonoRoutine.Awake()가 private이라 하위 Awake와 공존하지 않는다).
    protected override void Constructor()
    {
        base.Constructor();
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
