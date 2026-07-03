using UnityEngine;
using Haare.Client.Routine;

public class CameraFollower : MonoRoutine
{
    [SerializeField] private Transform mainCamera;

    protected override void LateUpdateProcess()
    {
        base.LateUpdateProcess();
        if (mainCamera != null)
        {
            transform.position = mainCamera.position;
            transform.rotation = mainCamera.rotation;
        }
    }
}
