using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Playmove
{
    public enum PYBundleManagerState
    {
        None,
        Loading,
        Ready
    }

    /// <summary>
    /// Bundle types we have
    /// </summary>
    public enum PYBundleType
    {
        Content,
        Data,
        Localization
    }

    /// <summary>
    /// Priority of PYBundleData
    /// </summary>
    public enum PYBundlePriority
    {
        Local,
        Global
    }

    public class PYBundleManager : MonoBehaviour
    {
        #region Events

        public class PYBundleManagerEventData
        {
            public bool IsDoneLoading { get; set; }
            public float ProgressLoading { get; set; }
        }

        public class PYBundleManagerEvent : UnityEvent<PYBundleManagerEventData> { }

        public PYBundleManagerEvent onStartLoading = new PYBundleManagerEvent();
        public PYBundleManagerEvent onLoadCompleted = new PYBundleManagerEvent();
        public PYBundleManagerEvent onLoading = new PYBundleManagerEvent();
        public PYBundleManagerEvent onUnloadCompleted = new PYBundleManagerEvent();

        #endregion Events

        private static PYBundleManager _instance;

        public static PYBundleManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PYBundleManager>();
                return _instance;
            }
        }

        private static PYContent _contentManager;

        public static PYContent Content
        {
            get
            {
                if (_contentManager == null)
                    _contentManager = new PYContent();
                return _contentManager;
            }
        }

        private static PYData _dataManager;

        public static PYData Data
        {
            get
            {
                if (_dataManager == null)
                    _dataManager = new PYData();
                return _dataManager;
            }
        }

        private static PYLocalization _localizationManager;

        public static PYLocalization Localization
        {
            get
            {
                if (_localizationManager == null)
                    _localizationManager = new PYLocalization();
                return _localizationManager;
            }
        }

        // 0: Current dateTime
        // 1: Content
        public const string LOG_TIME_PREFIX = "[{0}] {1}\n";

        public bool UnloadCacheOnSceneChange = true;

        public string ExpansionName { get { return ""; } set { } }
        public string Language { get { return ""; } set { } }
        public PYBundleManagerState State = PYBundleManagerState.None;

        public bool IsReady { get { return State == PYBundleManagerState.Ready; } }
        public bool IsLoading { get { return State == PYBundleManagerState.Loading; } }

        private int _totalAmountBundlesToLoad = 0;
        private int _currentBundlesLoaded = 0;

        #region Unity Functions

        private void Awake()
        {
            if (this != Instance)
            {
                Destroy(gameObject);
                return;
            }
            //DontDestroyOnLoad(gameObject);
        }

        private void OnLevelWasLoaded()
        {
            if (UnloadCacheOnSceneChange)
            {
                ClearCache();
                Resources.UnloadUnusedAssets();
            }
        }

        private void OnDestroy()
        {
            if (this != Instance)
                return;
            Unload(true);
        }

        #endregion Unity Functions

        public void WriteLog(string content)
        {
            string filePath = Application.persistentDataPath + "/_bundlelog.txt";
            try
            {
                StreamWriter writer = new StreamWriter(filePath, true, System.Text.Encoding.UTF8);
                writer.Write(content);
                writer.Close();
            }
            catch { }
        }

        public void Load()
        {
            State = PYBundleManagerState.Loading;

            // Prepare subsystems to load
            _totalAmountBundlesToLoad = Content.PrepareToLoad();
            _totalAmountBundlesToLoad += Data.PrepareToLoad();
            _totalAmountBundlesToLoad += Localization.PrepareToLoad();

            onStartLoading.Invoke(new PYBundleManagerEventData());

            //WriteLog("\n***** " + string.Format(LOG_TIME_PREFIX,
            //    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
            //    "Starting " + PlaytableWin32.GameName + " v" + PlaytableWin32.GameVersion +
            //    " accepting bundles with version greater or equals to v" + PlaytableWin32.BundleVersion + " *****"));

            // Load subsystems
            Content.Load(() =>
                {
                    Data.Load(() =>
                        {
                            Localization.Load(() =>
                                {
                                    State = PYBundleManagerState.Ready;
                                    onLoadCompleted.Invoke(new PYBundleManagerEventData() { IsDoneLoading = true, ProgressLoading = 1f });
                                });
                        });
                });
        }

        public void Unload(bool unloadAllAssets)
        {
            State = PYBundleManagerState.None;

            Content.UnloadBundle(unloadAllAssets);
            Data.UnloadBundle(unloadAllAssets);
            Localization.UnloadBundle(unloadAllAssets);

            onUnloadCompleted.Invoke(new PYBundleManagerEventData());
        }

        public void ReloadBundle(BundleData bundleData)
        {
            foreach (AssetBundle bundle in bundleData.Bundles)
                bundle.Unload(true);

            bundleData.Bundles.Clear();
            Resources.UnloadUnusedAssets();

            foreach (string bundlePath in bundleData.AbsoluteBundlePaths)
                bundleData.Bundles.Add(AssetBundle.LoadFromFile(bundlePath));
        }

        public void ReloadBundles(List<BundleData> bundlesData, Action callback)
        {
            StartCoroutine(ReloadBundlesRoutine(bundlesData, callback));
        }

        private IEnumerator ReloadBundlesRoutine(List<BundleData> bundlesData, Action callback)
        {
            foreach (BundleData bundleData in bundlesData)
            {
                foreach (AssetBundle bundle in bundleData.Bundles)
                    bundle.Unload(true);

                yield return new WaitForEndOfFrame();
                bundleData.Bundles.Clear();
            }

            Resources.UnloadUnusedAssets();

            foreach (BundleData bundleData in bundlesData)
            {
                foreach (string bundlePath in bundleData.AbsoluteBundlePaths)
                {
                    bundleData.Bundles.Add(AssetBundle.LoadFromFile(bundlePath));
                    yield return new WaitForEndOfFrame();
                }
            }

            if (callback != null)
                callback();
        }

        public void ClearCache()
        {
            Content.ClearAllCache();
            Data.ClearAllCache();
            Localization.ClearAllCache();
        }

        public void SendOnLoadingEvent()
        {
            _currentBundlesLoaded++;
            onLoading.Invoke(new PYBundleManagerEventData()
            {
                IsDoneLoading = _currentBundlesLoaded >= _totalAmountBundlesToLoad,
                ProgressLoading = (float)_currentBundlesLoaded / (float)_totalAmountBundlesToLoad
            });
        }

        #region Methods for Editor script

#if UNITY_EDITOR

        public static string[] GetAssetsTag()
        {
            List<string> _bundlesAssetsTags = new List<string>();

            // Data
            try
            {
                _bundlesAssetsTags.AddRange(Directory.GetFiles(string.Format("Assets" + PYBundleFolderScanner.GLOBAL_ASSETS_FOLDERS, PYBundleType.Data), "*.*", SearchOption.AllDirectories));
            }
            catch { }
            try
            {
                _bundlesAssetsTags.AddRange(Directory.GetFiles(string.Format("Assets" + PYBundleFolderScanner.EXPANSION_ASSETS_FOLDERS, PYBundleManager.Instance.ExpansionName, PYBundleType.Data), "*.*", SearchOption.AllDirectories));
            }
            catch { }

            // Localization
            try
            {
                _bundlesAssetsTags.AddRange(Directory.GetFiles(string.Format("Assets" + PYBundleFolderScanner.GLOBAL_ASSETS_FOLDERS, PYBundleType.Localization), "*.*", SearchOption.AllDirectories));
            }
            catch { }
            try
            {
                _bundlesAssetsTags.AddRange(Directory.GetFiles(string.Format("Assets" + PYBundleFolderScanner.EXPANSION_ASSETS_FOLDERS, PYBundleManager.Instance.ExpansionName, PYBundleType.Localization), "*.*", SearchOption.AllDirectories));
            }
            catch { }

            // Content
            try
            {
                _bundlesAssetsTags.AddRange(Directory.GetFiles(string.Format("Assets" + PYBundleFolderScanner.GLOBAL_ASSETS_FOLDERS, PYBundleType.Content), "*.*", SearchOption.AllDirectories));
            }
            catch { }
            try
            {
                _bundlesAssetsTags.AddRange(Directory.GetFiles(string.Format("Assets" + PYBundleFolderScanner.EXPANSION_ASSETS_FOLDERS, PYBundleManager.Instance.ExpansionName, PYBundleType.Content), "*.*", SearchOption.AllDirectories));
            }
            catch { }

            // Process
            _bundlesAssetsTags = _bundlesAssetsTags.Where(name => !name.EndsWith(".meta")).ToList();

            ReadLocalizationTextTags(_bundlesAssetsTags);
            _bundlesAssetsTags.Sort();

            for (int x = 0; x < _bundlesAssetsTags.Count; x++)
            {
                // If the current filtering tag is a Text we dont need to proccess it
                if (_bundlesAssetsTags[x].Contains(": Text"))
                    continue;

                _bundlesAssetsTags[x] = _bundlesAssetsTags[x].Replace("/", "\\");

                string[] tempSplits = _bundlesAssetsTags[x].Split('\\');
                _bundlesAssetsTags[x] = tempSplits[tempSplits.Length - 1];

                string[] extension = _bundlesAssetsTags[x].Split('.');
                if (extension.Length > 1)
                    _bundlesAssetsTags[x] = extension[0] + " : " + GetTypeByExtension(extension[1]);
            }

            _bundlesAssetsTags = _bundlesAssetsTags.Distinct().ToList();
            _bundlesAssetsTags.Sort();

            return _bundlesAssetsTags.ToArray();
        }

        private static void ReadLocalizationTextTags(List<string> paths)
        {
            for (int x = 0; x < paths.Count; x++)
            {
                if (paths[x].Contains("string") && paths[x].Contains(".json"))
                {
                    using (StreamReader reader = new StreamReader(paths[x]))
                    {
                        List<LocalizationData> jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LocalizationData>>(reader.ReadToEnd());
                        //JSONData<List<LocalizationData>> jsonObject = JsonUtility.FromJson<JSONData<List<LocalizationData>>>(reader.ReadToEnd());

                        paths.RemoveAt(x);
                        foreach (LocalizationData data in jsonObject)
                        {
                            string tag = data.TagNome + " : Text";
                            bool foundTag = false;
                            for (int y = 0; y < paths.Count; y++)
                            {
                                if (paths[y].Contains(tag))
                                {
                                    paths[y] += "\n" + data.Texto;
                                    foundTag = true;
                                    break;
                                }
                            }

                            if (!foundTag)
                                paths.Insert(x, tag + "<||>" + data.Texto);
                        }
                    }
                }
            }
        }

        public static string GetTypeByRealType(object obj)
        {
            //if (asset is PYImage)
            //    return "Sprite";
            //else if (asset is PYText)
            //    return "Text";
            //else if (asset is PYPrefab)
            //    return "GameObject";
            return "";
        }

        private static string GetTypeByExtension(string extension)
        {
            switch (extension)
            {
                case "mp3":
                case "wav":
                    return "Audio";

                case "png":
                case "jpg":
                    return "Sprite";

                case "prefab":
                    return "GameObject";

                case "ttf":
                    return "Font";
            }

            return "TextAsset";
        }

#endif

        #endregion Methods for Editor script
    }
}