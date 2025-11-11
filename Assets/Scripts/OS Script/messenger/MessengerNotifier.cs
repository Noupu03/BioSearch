using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessengerNotifier : MonoBehaviour
{
    public GameObject popupUI;
    public TextMeshProUGUI popupText;

    [Header("메신저 프로그램 참조")]
    public GameObject messengerProgramPrefab; // 인스펙터에서 프리팹 연결

    private Dictionary<string, int> pendingMessages = new Dictionary<string, int>();
    private bool popupActive = false;

    public void HandleMessageArrival(MessengerChatUI.MessageData msg)
    {
        if (!pendingMessages.ContainsKey(msg.sender))
            pendingMessages[msg.sender] = 0;

        pendingMessages[msg.sender]++;

        // 팝업 표시
        ShowPopup();
    }

    private void ShowPopup()
    {
        if (popupUI == null || popupText == null) return;

        popupUI.SetActive(true);
        popupText.text = BuildPopupText();

        if (!popupActive)
        {
            popupActive = true;
            StartCoroutine(AutoHideWhenProgramActive());
        }
    }

    private IEnumerator AutoHideWhenProgramActive()
    {
        while (true)
        {
            if (messengerProgramPrefab != null && messengerProgramPrefab.activeInHierarchy)
            {
                // 메신저가 켜져 있으면 5초 후 자동 숨김
                yield return new WaitForSeconds(5f);
                popupUI.SetActive(false);
                pendingMessages.Clear();
                popupActive = false;
                yield break;
            }

            // 프로그램이 꺼져있으면 계속 대기
            yield return null;
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

    public void DeliverPendingMessages()
    {
        if (popupUI != null)
            popupUI.SetActive(false);

        pendingMessages.Clear();
        popupActive = false;
    }
}
