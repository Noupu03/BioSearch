using UnityEngine;

/// <summary>
/// 스캔 팝업 드래그 이동. DragHandler 컴포넌트에 위임한다.
/// 이 컴포넌트는 씬 오브젝트에 DragHandler가 없는 경우를 위한 래퍼로만 존재.
/// </summary>
[RequireComponent(typeof(DragHandler))]
public class LoadingDragHandler : MonoBehaviour { }
