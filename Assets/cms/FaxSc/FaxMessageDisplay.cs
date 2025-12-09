using UnityEngine;
using TMPro;          // TMP ¾µ ¶§

public class FaxMessageDisplay : MonoBehaviour
{
    public TextMeshProUGUI textUI;

    public void SetMessage(string message)
    {
        if (textUI != null)
        {
            textUI.text = message;
        }
    }
}
