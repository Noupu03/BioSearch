using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadNextScene(string sceneName)
    {
        Debug.Log("버튼 클릭됨! 씬 전환 시도 중 → " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
