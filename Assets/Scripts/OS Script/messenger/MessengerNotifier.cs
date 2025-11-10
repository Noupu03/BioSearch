using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessengerNotifier : MonoBehaviour
{
    public GameObject popupUI;
    public TextMeshProUGUI popupText;

    private Dictionary<string, int> pendingMessages = new Dictionary<string, int>();
    private bool popupActive = false;

    public void HandleMessageArrival(MessengerProgram.MessageData msg)
    {
        // 누적 메시지 기록
        if (!pendingMessages.ContainsKey(msg.sender))
            pendingMessages[msg.sender] = 0;
        pendingMessages[msg.sender]++;

        ShowPopup();
    }

    private void ShowPopup()
    {
        popupUI.SetActive(true);
        popupText.text = BuildPopupText();

        if (!popupActive)
        {
            popupActive = true;
            StartCoroutine(AutoHideIfProgramActive());
        }
    }

    private IEnumerator AutoHideIfProgramActive()
    {
        while (true)
        {
            var program = FindObjectOfType<MessengerProgram>();
            if (program != null && program.gameObject.activeInHierarchy)
            {
                yield return new WaitForSeconds(5f);
                popupUI.SetActive(false);
                popupActive = false;
                pendingMessages.Clear();
                yield break;
            }
            yield return null; // 계속 확인
        }
    }

    private string BuildPopupText()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var kvp in pendingMessages)
        {
            sb.AppendLine($"{kvp.Key}에게 메시지 {kvp.Value}건");
        }
        return sb.ToString();
    }

    // MessengerProgram이 활성화될 때 호출
    public void DeliverPendingMessages()
    {
        popupUI.SetActive(false);
        popupActive = false;
        pendingMessages.Clear();
    }
}
