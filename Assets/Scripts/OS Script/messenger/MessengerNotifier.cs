using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessengerNotifier : MonoBehaviour
{
    public GameObject popupUI;
    public TextMeshProUGUI popupText;

    private Dictionary<string, int> pendingMessages = new Dictionary<string, int>();
    private Coroutine autoHideCoroutine = null;

    // ProgramOpen에서 전달받는 메신저 창 인스턴스
    private GameObject messengerInstance;

    public void SetMessengerProgramInstance(GameObject instance)
    {
        messengerInstance = instance;
    }

    public void HandleMessageArrival(MessengerChatUI.MessageData msg)
    {
        if (!pendingMessages.ContainsKey(msg.sender))
            pendingMessages[msg.sender] = 0;

        pendingMessages[msg.sender]++;
        ShowPopup();
    }

    private void ShowPopup()
    {
        if (popupUI == null || popupText == null) return;

        popupUI.SetActive(true);
        popupText.text = BuildPopupText();

        if (autoHideCoroutine == null)
            autoHideCoroutine = StartCoroutine(AutoHideRoutine());
    }

    private IEnumerator AutoHideRoutine()
    {
        while (true)
        {
            // 메신저 창이 활성화 중이면 5초 후 알림창 닫기
            if (messengerInstance != null && messengerInstance.activeInHierarchy)
            {
                yield return new WaitForSeconds(5f);
                HidePopup();
                yield break;
            }

            // 꺼져있으면 계속 대기
            yield return null;
        }
    }

    private void HidePopup()
    {
        if (popupUI != null)
            popupUI.SetActive(false);

        pendingMessages.Clear();

        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
    }

    public void DeliverPendingMessages()
    {
        HidePopup();
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
}
