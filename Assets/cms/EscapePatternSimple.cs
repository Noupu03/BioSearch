using UnityEngine;
using System.Collections;

public class EscapePatternSimple : MonoBehaviour
{
    public Transform pointA;      // ���� ��ġ
    public Transform pointB;      // ��ǥ ��ġ
    public float speed = 2f;      // �̵� �ӵ�
    private bool isMoving = false;

    // A -> B �� �� �̵� (Ż�� �� ȣ��)
    public void MoveToEscape()
    {
        StopAllCoroutines(); // ���� �̵� ���̸� ��� ���߰� ���� ����
        StartCoroutine(MoveTo(pointB));
    }

    // B -> A ���� (�� Ŭ�� �� ȣ��)
    public void MoveBack()
    {
        StopAllCoroutines();
        StartCoroutine(MoveTo(pointA));
    }

    IEnumerator MoveTo(Transform target)
    {
        isMoving = true;
        while (Vector3.Distance(transform.position, target.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            yield return null;
        }
        isMoving = false;
    }
}
