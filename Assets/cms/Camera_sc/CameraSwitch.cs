using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    // ��ũ��Ʈ ��ܿ� ���� �߰�
    public Camera targetCamera;
    public float defaultFOV = 60f;
    public float zoomFOV = 40f;
    public float transitionSpeed = 5f;

    private float targetFOV;

    public Transform view1;     // ù ��° ���� (W)
    public Transform view2;     // �� ��° ���� (S, �⺻ ������)
    public Transform leftView;  // S ������ �� ���� (A)
    public Transform rightView; // S ������ �� ������ (D)


    private Transform currentView;
    private bool inView2 = false;   // ���� S ��������
    private bool mustPassThroughS = false; // A,D ���� �� S�� ���ľ� �ϴ��� üũ

    void Start()
    {
        // ������ �� S �信�� ����
        currentView = view2;
        transform.position = view2.position;
        transform.rotation = view2.rotation;
        inView2 = true;
        targetFOV = defaultFOV;
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();
    }

    void Update()
    {
        // W �� view1 ��ȯ
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentView = view1;
            inView2 = false;
            targetFOV = zoomFOV;
        }

        // S �� view2 ��ȯ
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentView = view2;
            inView2 = true;
            mustPassThroughS = true; // S�� ���� ������ �ٽ� �ʱ�ȭ
            targetFOV = defaultFOV;
        }


        // S ������ ���� A, D �۵�
        if (inView2)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (!mustPassThroughS) // ���� S�� ��ġ�� �ʾҴٸ�
                {
                    currentView = view2;   // ���� S�� �̵�
                    mustPassThroughS = true;
                }
                else // �̹� S�� ���ƴٸ�
                {
                    currentView = leftView;
                    mustPassThroughS = false; // ����
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (!mustPassThroughS)
                {
                    currentView = view2;   // ���� S�� �̵�
                    mustPassThroughS = true;
                }
                else
                {
                    currentView = rightView;
                    mustPassThroughS = false; // ����
                }
            }
        }

        // ī�޶� �ε巴�� �̵�/ȸ��
        transform.position = Vector3.Lerp(transform.position, currentView.position, Time.deltaTime * transitionSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentView.rotation, Time.deltaTime * transitionSpeed);

        targetCamera.fieldOfView = Mathf.Lerp(
        targetCamera.fieldOfView,
        targetFOV,
        Time.deltaTime * transitionSpeed);
    }
}
