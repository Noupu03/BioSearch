using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MessengerProgram : MonoBehaviour
{
    [Header("상단 전환 버튼")]
    public Button bossButton;
    public Button predecessorButton;

    [Header("MessengerWindow 연결")]
    public ProgramOpen programOpen;
    public GameObject messengerWindowPrefab; // 채팅창 프리팹

    private Dictionary<string, List<MessengerChatUI.MessageData>> conversations = new();

    void Start()
    {
        if (bossButton != null)
        {
            bossButton.onClick.AddListener(() => OpenChat("상사"));
        }
        if (predecessorButton != null)
        {
            predecessorButton.onClick.AddListener(() => OpenChat("전임자"));
        }
    }

    void OnEnable()
    {
        // DataManager에서 누적 메시지 가져오기
        foreach (var kvp in MessengerDataManager.Instance.conversations)
        {
            conversations[kvp.Key] = new List<MessengerChatUI.MessageData>(kvp.Value);
        }
    }

    void OpenChat(string targetName)
    {
        // 기존 ProgramOpen 기능 호출
        if (programOpen != null)
        {
            programOpen.MessangerWindowOpen();
        }

        // ChatUI는 MessengerWindow 안에서 활성화되어야 하므로, MessengerChatUI가 찾아지면 초기화
        MessengerChatUI chatUI = messengerWindowPrefab.GetComponentInChildren<MessengerChatUI>(true);
        if (chatUI == null) return;

        chatUI.target = targetName;

        if (!conversations.ContainsKey(targetName))
            conversations[targetName] = new List<MessengerChatUI.MessageData>();

        chatUI.SetConversation(conversations[targetName]);
        chatUI.RefreshChat();
        chatUI.gameObject.SetActive(true);
    }
}
