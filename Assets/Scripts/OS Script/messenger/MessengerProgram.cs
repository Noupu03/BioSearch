using UnityEngine;
using UnityEngine.UI;

public class MessengerProgram : MonoBehaviour
{
    public Button bossButton;
    public Button predecessorButton;
    public ProgramOpen programOpen;

    void Awake()
    {
        if (programOpen == null)
            programOpen = FindObjectOfType<ProgramOpen>();
    }

    void Start()
    {
        if (bossButton != null)
            bossButton.onClick.AddListener(() => OpenChat("상사"));
        if (predecessorButton != null)
            predecessorButton.onClick.AddListener(() => OpenChat("전임자"));
    }

    void OpenChat(string targetName)
    {
        programOpen.MessangerWindowOpen();

        GameObject messengerInstance = programOpen.GetActiveInstance(programOpen.messengerWindowprefab);

        MessengerChatUI chatUI = messengerInstance.GetComponentInChildren<MessengerChatUI>(true);

        chatUI.SetTarget(targetName);

        
    }
}

