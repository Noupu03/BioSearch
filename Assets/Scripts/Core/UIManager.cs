using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    private int currentValue = 10;

    void Start()
    {
        UpdateUI();
    }

    // 하나 줄이기
    public void DecreaseCounter()
    {
        if (currentValue > 0)
        {
            currentValue--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        counterText.text = currentValue.ToString();
    }
}
