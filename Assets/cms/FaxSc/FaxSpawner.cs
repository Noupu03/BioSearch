using UnityEngine;
using UnityEngine.UI;

public class FaxSpawner : MonoBehaviour
{
    public GameObject faxPrefab;
    public Camera roomCamera;

    public void SpawnFax()
    {
        //  팩스 생성
        GameObject fax = Instantiate(faxPrefab);

        // 2 FaxViewer 연결
        FaxViewer viewer = fax.GetComponent<FaxViewer>();
        viewer.viewCamera = roomCamera;

        // 3 팩스 내부 Button 찾기
        Button expandButton = fax.GetComponentInChildren<Button>();

        if (expandButton != null)
        {
            expandButton.onClick.RemoveAllListeners();
            expandButton.onClick.AddListener(() => viewer.TriggerExpand());
        }
        else
        {
            Debug.LogError("[FAX] Expand 버튼을 찾을 수 없습니다.");
        }

        Debug.Log("[FAX] 팩스 생성 완료 + UI 연결 성공");
    }
}
