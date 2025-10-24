using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    private bool isGameOver = false;
    public float returnDelay = 0f; // 5�� �� ����
    public string startSceneName = "StartScene"; // ������ �� �̸�

    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return; // �ߺ� ȣ�� ����
        isGameOver = true;

        Debug.Log($"[GameOver] �߻�! ����: {reason}");

        // ���ӿ��� �� ��������/����/���� ī��Ʈ �ʱ�ȭ
        SelectPopupManager popupManager = FindObjectOfType<SelectPopupManager>();
        if (popupManager != null)
            popupManager.ResetCounts();
       

        StartCoroutine(ReturnToStartScene());


    }

 IEnumerator ReturnToStartScene()
{
    // optional delay
    yield return new WaitForSeconds(returnDelay);

    // ���ӿ��� �� �ʱ�ȭ
    SanityManager sanity = FindObjectOfType<SanityManager>();
    if (sanity != null)
        sanity.ResetSanity();

    // ���� �� �ε�
    SceneManager.LoadScene(startSceneName);
}


    public void ResetGameOver()
    {
        isGameOver = false;
        Debug.Log("[GameOverManager] ���� �ʱ�ȭ��.");
    }

    public bool IsGameOver()
    {
        return isGameOver;

    }
}
