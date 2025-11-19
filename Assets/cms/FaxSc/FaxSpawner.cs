using UnityEngine;

public class FaxSpawner : MonoBehaviour
{
    public GameObject faxPrefab;
    public Camera roomCamera;
    public Vector3 spawnPos;

    public void SpawnFax()
    {
        GameObject fax = Instantiate(faxPrefab, spawnPos, Quaternion.identity);

        // FaxViewer 연결
        FaxViewer viewer = fax.GetComponent<FaxViewer>();
        viewer.viewCamera = roomCamera;

        // 삭제 버튼 카메라 연결
        FaxDeleteButton del = fax.GetComponentInChildren<FaxDeleteButton>();
        if (del != null)
        {
            del.targetCamera = roomCamera;
            del.viewer = viewer;
            del.faxObject = fax;
        }

        Debug.Log("FAX 생성 완료 + 카메라 자동 주입됨");
    }
}
