using UnityEngine;
using UnityEngine.UI;

public class MessengerProgram : MonoBehaviour
{
    public Button bossButton;
    public Button predecessorButton;
    public ProgramOpen programOpen;
    public Button submitButton;   // ★ 제출 버튼 추가

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
        if (submitButton != null)
            submitButton.onClick.AddListener(OpenSubmitWindow); // ★ 추가
    }

    void OpenChat(string targetName)
    {
        programOpen.MessangerWindowOpen();

        GameObject messengerInstance = programOpen.GetActiveInstance(programOpen.messengerWindowprefab);

        MessengerChatUI chatUI = messengerInstance.GetComponentInChildren<MessengerChatUI>(true);

        chatUI.SetTarget(targetName);

        // --- 추가: 해당 상대의 메시지들에서 체크리스트 매핑을 UI로 추가 ---
        if (MessengerDataManager.Instance != null)
        {
            MessengerDataManager.Instance.CreateChecklistsForTarget(targetName);
        }
    }
    // ★ 제출창 열기
    void OpenSubmitWindow()
    {
        programOpen.OpenSubmitWindow();
    }
}
