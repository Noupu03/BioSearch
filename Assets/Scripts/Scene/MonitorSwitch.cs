using UnityEngine;

public class MonitorSwitch : MonoBehaviour
{
    [SerializeField] private Camera monitorCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Material monitorMaterial;

    private bool isOn = true;

    void Start() => ApplyState();

    void OnMouseDown()
    {
        isOn = !isOn;
        ApplyState();
    }

    private void ApplyState()
    {
        if (monitorCamera == null || monitorMaterial == null || renderTexture == null) return;

        if (isOn)
        {
            monitorCamera.targetTexture = renderTexture;
            monitorMaterial.mainTexture = renderTexture;
        }
        else
        {
            monitorCamera.targetTexture = null;
            monitorMaterial.mainTexture = Texture2D.blackTexture;
        }
    }
}

