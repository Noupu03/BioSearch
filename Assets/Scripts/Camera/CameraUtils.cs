using UnityEngine;

/// <summary>
/// 카메라 공통 유틸리티.
/// CameraSwitcher와 HybridCameraController의 중복 SetCameraState 로직을 통합한다.
/// </summary>
public static class CameraUtils
{
    /// <summary>카메라 활성/비활성 및 AudioListener 동기화.</summary>
    public static void SetActive(Camera cam, bool active)
    {
        if (cam == null) return;
        cam.enabled = active;
        var listener = cam.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = active;
    }
}
