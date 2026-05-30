using UnityEngine;

public class ObjectClick : MonoBehaviour
{
    public Camera targetCamera;  // 사용할 카메라 직접 연결
    public UIManager uiManager;  // UI 매니저 연결

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 클릭
        {
            // Camera.main 대신 targetCamera 사용
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                BlinkObject blink = hit.collider.GetComponent<BlinkObject>();
                if (blink != null)
                {
                    blink.StartBlink();
                    //uiManager.DecreaseCounter();
                }
            }
        }
    }
}
