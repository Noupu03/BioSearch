using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.position = mainCamera.position;
            transform.rotation = mainCamera.rotation;
        }
    }
}
