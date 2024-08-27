using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_5_3
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
#endif

namespace Playmove
{
    public class GameInitialize : MonoBehaviour
    {
        public TagManager.Scenes Scene;

        // Use this for initialization
        void Start()
        {
            //PYScoreData.Initialize();

            if (PYBundleManager.Instance.IsReady)
                Load();
            else
            {
                PYBundleManager.Instance.onLoadCompleted.AddListener((data) =>
                    {
                        Invoke("Load", 0.5f);

                    // Load Icon from all contents
                    //List<ContentAsset<Sprite>> icons = PYBundleManager.Content.GetAssetsFromAllContents<Sprite>("Icon");
                    //foreach (ContentAsset<Sprite> i in icons)
                    //{
                    //    Debug.Log(i.BundleName + " : " + i.Asset + " : IsBundleReadable: " + i.BundleData.Version.IsReadable);
                    //}

                    // Select multi contents and load a file from them
                    //PYBundleManager.Content.SetCurrentContents(icons[0].BundleName, icons[2].BundleName);
                    //List<ContentAsset<TextAsset>> question = PYBundleManager.Content.GetAssets<TextAsset>("Questions");
                    //foreach (ContentAsset<TextAsset> t in question)
                    //{
                    //    Debug.Log(t.Asset.text.Split('\n')[0]);
                    //}

                    // Reload all content manager and load new assets again
                    //Debug.Log("Temp:");
                    //PYBundleManager.Content.ReloadAll(() =>
                    //    {
                    //        Debug.Log("Reloaded");
                    //        List<ContentAsset<Sprite>> iconss = PYBundleManager.Content.GetAssetsFromAllContents<Sprite>("Icon");
                    //        foreach (ContentAsset<Sprite> i in iconss)
                    //        {
                    //            Debug.Log(i.BundleName + " : " + i.Asset + " : IsBundleReadable: " + i.BundleData.Version.IsReadable);
                    //        }
                    //    });
                });
            }
        }

        void Load()
        {
#if UNITY_5_3
            FaderManager.FadeInCamera(1, 0, Color.white, () => SceneManager.LoadScene("MainMenu"));
#else
        Application.LoadLevel("MainMenu");
#endif
        }
    }
}