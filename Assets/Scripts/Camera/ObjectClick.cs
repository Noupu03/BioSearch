using UnityEngine;
using Haare.Client.Routine;

/// <summary>
/// 마우스 클릭으로 3D 오브젝트에 레이캐스트해 BlinkObject를 활성화한다.
/// </summary>
public class ObjectClick : MonoRoutine
{
    [SerializeField] private Camera targetCamera;

    protected override void UpdateProcess()
    {
        base.UpdateProcess();
        if (!Input.GetMouseButtonDown(0)) return;
        if (targetCamera == null) return;

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
            hit.collider.GetComponent<BlinkObject>()?.StartBlink();
    }
}
