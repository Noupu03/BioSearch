using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EscapePatternSimple : MonoBehaviour
{
    [Tooltip("이동 경유지 목록. [0]=시작(A), [1]=중간(B), [2]=끝(C) 순서로 배치")]
    public List<Transform> waypoints = new List<Transform>();
    public float speed = 2f;

    // 앞방향 이동: waypoints[0] -> ... -> waypoints[마지막] (탈출 시 호출)
    public void MoveToEscape()
    {
        StopAllCoroutines();
        StartCoroutine(MoveSequenceForward());
    }

    // 역방향 이동: waypoints[마지막] -> ... -> waypoints[0] (복귀 시 호출)
    public void MoveBack()
    {
        StopAllCoroutines();
        StartCoroutine(MoveSequenceReverse());
    }

    private IEnumerator MoveSequenceForward()
    {
        for (int i = 1; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
                yield return MoveTo(waypoints[i]);
        }
        Debug.Log("[EscapePatternSimple] 탈출 위치에 도달");
    }

    private IEnumerator MoveSequenceReverse()
    {
        for (int i = waypoints.Count - 2; i >= 0; i--)
        {
            if (waypoints[i] != null)
                yield return MoveTo(waypoints[i]);
        }
        Debug.Log("[EscapePatternSimple] 원래 위치로 복귀");
    }

    private IEnumerator MoveTo(Transform target)
    {
        while (Vector3.Distance(transform.position, target.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            yield return null;
        }
    }
}
