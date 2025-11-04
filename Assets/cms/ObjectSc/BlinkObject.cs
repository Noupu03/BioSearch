using UnityEngine;
using System.Collections;

public class BlinkObject : MonoBehaviour
{
    private Renderer rend;
    public Color blinkColor = Color.black; // ±ôºıÀÏ »ö
    public float blinkDuration = 0.1f;   // ±ôºıÀÓ ½Ã°£
    public int blinkCount = 1;           // ¸î ¹ø ±ôºıÀÏÁö

    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void StartBlink()
    {
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
