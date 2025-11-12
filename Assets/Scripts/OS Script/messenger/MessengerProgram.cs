using UnityEngine;
using UnityEngine.UI;

public class MessengerProgram : MonoBehaviour
{
    public Button bossButton;
    public Button predecessorButton;

    public ProgramOpen programOpen; // 자동 연결 가능
    public GameObject messengerWindowPrefab; // 프리팹 연결

    void Awake()
    {
        // 자동 연결
        if (programOpen == null)
        {
            programOpen = FindObjectOfType<ProgramOpen>();
            if (programOpen == null)
            {
                Debug.LogWarning("MessengerProgram: 씬에서 ProgramOpen을 찾을 수 없습니다.");
            }
        }
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
        if (programOpen == null)
        {
            Debug.LogWarning("MessengerProgram: ProgramOpen이 연결되어 있지 않습니다.");
            return;
        }

        // ProgramOpen에서 메신저 창 열기/생성
        programOpen.MessangerWindowOpen();

        // messengerInstance 가져오기
        GameObject messengerInstance = programOpen.GetActiveInstance(programOpen.messengerWindowprefab);

        if (messengerInstance == null)
        {
            Debug.LogWarning("MessengerProgram: 메신저 창 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // MessengerChatUI 가져오기
        MessengerChatUI chatUI = messengerInstance.GetComponentInChildren<MessengerChatUI>(true);
        if (chatUI == null)
        {
            Debug.LogWarning("MessengerProgram: MessengerChatUI를 찾을 수 없습니다.");
            return;
        }

        // 대상 전환
        chatUI.SetTarget(targetName);
    }
}
