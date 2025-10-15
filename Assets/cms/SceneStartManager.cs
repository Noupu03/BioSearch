using UnityEngine;

public class SceneStartManager : MonoBehaviour
{
    void Start()
    {
        TimerManager timer = FindObjectOfType<TimerManager>();
        if (timer != null)
        {
            timer.StartTimer();
        }
        Debug.Log("[SceneStartManager] �÷��̾� ����, Ÿ�̸� ����!");
    }
}
