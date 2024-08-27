using Playmove.Avatars.API;
using Playmove.Core.API;
using Playmove.Metrics.API;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Playmove.Core
{
    public class PlaytableBootstrap : MonoBehaviour
    {
#if UNITY_EDITOR
        private static int TriesToLoadBootstrapAgain = 0;
#endif
        [SerializeField] private string _sceneToLoad = string.Empty;

         private void Awake()
        {
            TestConnection();
        }

        private void TestConnection()
        {
            Playtable.Instance.OnPlaytableReady.AddListener(LoadNextScene);
            if(Playtable.Instance.Key == null)
            {
                Invoke("TestConnection", .02f);
                Playtable.Instance.OnPlaytableReady.RemoveListener(LoadNextScene);
                return;
            }
            Playtable.Instance.Initialize();
        }

        private void LoadNextScene()
        {
            Playtable.Instance.OnPlaytableReady.RemoveListener(LoadNextScene);
        
            //Used to Start Session
            MetricsAPI.StartSession();
            // ---

            if (string.IsNullOrEmpty(_sceneToLoad))
            {
            // #if UNITY_EDITOR
            //     if (TriesToLoadBootstrapAgain == 0)
            //     {
            //         TriesToLoadBootstrapAgain++;
            //         SceneManager.LoadScene(0);
            //     }
            //     else
            //     {
            //         Debug.LogWarning("You need to add your's scenes to unity Build Settings!");
            //     }
            // #else
                AvatarAPI.Open(result => SceneManager.LoadScene(_sceneToLoad));
            // #endif
            } else
            {
                AvatarAPI.Open(result => SceneManager.LoadScene(_sceneToLoad));
            }
        }

    }
}
