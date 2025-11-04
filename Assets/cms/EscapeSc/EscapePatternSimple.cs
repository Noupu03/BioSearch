using UnityEngine;
using System.Collections;

public class EscapePatternSimple : MonoBehaviour
{
    public Transform pointA;      // 시작 위치
    public Transform pointB;      // 중간 위치
    public Transform pointC;      // 최종 위치
    public float speed = 2f;      // 이동 속도
    private bool isMoving = false;

    // A -> B -> C 이동 (탈주 시 호출)
    public void MoveToEscape()
    {
        StopAllCoroutines();
        StartCoroutine(MoveToEscapeSequence());
    }

    // C -> B -> A 복귀 (문 클릭 시 호출)
    public void MoveBack()
    {
        StopAllCoroutines();
        StartCoroutine(MoveBackSequence());
    }

    private IEnumerator MoveToEscapeSequence()
    {
        isMoving = true;
        yield return MoveTo(pointB);
        yield return MoveTo(pointC);
        isMoving = false;
        Debug.Log("[EscapePatternSimple] 적 문앞에 도착 ");

    }

    private IEnumerator MoveBackSequence()
    {
        isMoving = true;
        yield return MoveTo(pointB);
        yield return MoveTo(pointA);
        isMoving = false;
        Debug.Log("[EscapePatternSimple] 적 원 위치로 돌아감 ");
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
