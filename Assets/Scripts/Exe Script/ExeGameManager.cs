using UnityEngine;

public class ExeGameManager : MonoBehaviour
{
    public static ExeGameManager Instance;

    [Header("텍스트 게임 프리팹")]
    public TextGamePopup textGamePopupPrefab; // Inspector에서 할당
    [Header("Canvas")]
    public Transform canvasTransform; // Inspector에서 Canvas 연결


    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// EXE 실행 → 텍스트 미니게임 열기
    /// </summary>
    public void RunExe(Exe exe)
    {
        if (exe == null)
        {
            Debug.LogWarning("Exe 실행 불가: exe가 null");
            return;
        }

        if (textGamePopupPrefab == null)
        {
            Debug.LogError("TextGamePopup Prefab이 할당되지 않았습니다! Inspector에서 설정해주세요.");
            return;
        }

        if (textGamePopupPrefab == null || canvasTransform == null)
        {
            Debug.LogWarning("Prefab 또는 Canvas 미할당");
            return;
        }

        // Canvas 아래에 프리팹 생성
        TextGamePopup popup = Instantiate(textGamePopupPrefab, canvasTransform, false);
        popup.Open(exe.name, exe.exeContent);

        Debug.Log("[ExeGameManager] EXE 실행됨 → " + exe.name);
    }
}
