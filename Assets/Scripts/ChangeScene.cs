using UnityEngine.SceneManagement;
using UnityEngine;

public class ChangeScene : MonoBehaviour
{
    private const string _transition = "Transition";
    private const string _swap = "Swap";

    private const float transitionTime = 1.0f;

    private FadeInAndOut finout = null;

    public int currentScene = 0;
    public int sceneNumber = 0;

    public bool auto = false;

    private void Awake()
    {
        GameObject go = GameObject.FindWithTag(_transition);
        finout = go.GetComponent<FadeInAndOut>();
    }

    private void Start()
    {
        if (auto)
        {
            SceneManager.LoadScene(sceneNumber, LoadSceneMode.Additive);
        }
    }

    public void Transition ()
    {
        finout.duration = transitionTime;
        finout.t = 0.0f;
        Invoke(_swap, transitionTime / 2.0f);
    }

    public void Swap ()
    {
        SceneManager.LoadScene(sceneNumber, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(currentScene);
    }
}
