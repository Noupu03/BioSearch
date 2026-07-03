using UnityEngine;
using TMPro;
using Haare.Client.Routine;

public class UIManager : MonoRoutine
{
    [SerializeField] private TextMeshProUGUI counterText;
    private int currentValue = 10;

    void Start() => UpdateUI();

    public void DecreaseCounter()
    {
        if (currentValue > 0) { currentValue--; UpdateUI(); }
    }

    private void UpdateUI()
    {
        if (counterText != null) counterText.text = currentValue.ToString();
    }
}
