using UnityEngine;

public class MonitorSwitch : MonoBehaviour
{
    [SerializeField] private Camera monitorCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Material monitorMaterial;

    private bool isOn = true;

    void OnMouseDown() // 오브젝트 클릭 시 실행
    {
        ToggleMonitor();
    }

    void ToggleMonitor()
    {
        isOn = !isOn;

        if (isOn)
        {
            monitorCamera.targetTexture = renderTexture;
            monitorMaterial.mainTexture = renderTexture;
        }
        else
        {
            monitorCamera.targetTexture = null;
            monitorMaterial.mainTexture = Texture2D.blackTexture; // 꺼진 화면
        }
    }
}

