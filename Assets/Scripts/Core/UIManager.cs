using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
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
