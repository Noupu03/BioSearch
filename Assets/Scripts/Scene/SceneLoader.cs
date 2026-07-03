using UnityEngine;
using UnityEngine.SceneManagement;
using Haare.Client.Routine;

public class SceneLoader : MonoRoutine
{
    public void LoadNextScene(string sceneName)
    {
        Debug.Log("��ư Ŭ����! �� ��ȯ �õ� �� �� " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
