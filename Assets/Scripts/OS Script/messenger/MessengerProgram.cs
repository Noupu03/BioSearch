using UnityEngine;
using UnityEngine.UI;

public class MessengerProgram : MonoBehaviour
{
    public Button bossButton;
    public Button leeButton;
    public Button seoButton;
    public Button jooButton;
    public Button parkButton;
    public Button choiButton;
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
            bossButton.onClick.AddListener(() => OpenChat("김부장"));
        if (leeButton != null)
            leeButton.onClick.AddListener(() => OpenChat("이대리"));
        if (seoButton != null)
            seoButton.onClick.AddListener(() => OpenChat("서주임"));
        if (jooButton != null)
            jooButton.onClick.AddListener(() => OpenChat("주인턴"));
        if (parkButton != null)
            parkButton.onClick.AddListener(() => OpenChat("박00"));
        if (choiButton != null)
            choiButton.onClick.AddListener(() => OpenChat("최00"));
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
