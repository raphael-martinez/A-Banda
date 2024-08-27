using Playmove.Core.BasicEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Playmove.Core.Bundles
{
    /// <summary>
    /// Bundle system to add dynamic content, expansion and localization
    /// We have two context of bundles Global and Expansion.
    /// Only one Expansion can be activated by time.
    /// We also have three type of bundles Content, Data and Localization
    /// </summary>
    public class PlaytableBundle : MonoBehaviour
    {
        /// <summary>
        /// Class just to group the different type of bundles into global and expansion
        /// </summary>
        public class ContextBundles
        {
            public List<Bundle> ContentBundles = new List<Bundle>();
            public List<Bundle> DataBundles = new List<Bundle>();
            public List<Bundle> LocalizationBundles = new List<Bundle>();
        }

        private static PlaytableBundle _instance = null;
        public static PlaytableBundle Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PlaytableBundle>();
                return _instance;
            }
        }

        /// <summary>
        /// This event is raised everytime a bundle is loaded passing the proggress on all bundles
        /// </summary>
        public PlaytableEventFloat OnLoading = new PlaytableEventFloat();
        /// <summary>
        /// This event is raised when all bundles get loaded
        /// </summary>
        public PlaytableEvent OnLoaded = new PlaytableEvent();

        /// <summary>
        /// All bundles for the current Expansion context
        /// </summary>
        public ContextBundles ExpansionBundles { get; private set; }
        /// <summary>
        /// All bundles for Global context
        /// </summary>
        public ContextBundles GlobalBundles { get; private set; }

        public bool IsLoaded { get; set; }

        private float _bundlesLoaded = 0;
        private int _bundlesToLoad = 0;

        public void Load(UnityAction onLoaded = null)
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (onLoaded != null)
                OnLoaded.AddListener(onLoaded);

            ExpansionBundles = new ContextBundles();
            GlobalBundles = new ContextBundles();

            int loaded = 0;
            LoadContentBundles(() =>
            {
                Content.Initialize();
                loaded++;
                if (loaded == 3) OnLoadedCompleted();
            });
            LoadDataBundles(() =>
            {
                Data.Initialize();
                loaded++;
                if (loaded == 3) OnLoadedCompleted();
            });
            LoadLocalizationBundles(() =>
            {
                Localization.Initialize();
                loaded++;
                if (loaded == 3) OnLoadedCompleted();
            });
        }
        public void Reload(UnityAction completed = null)
        {
            Content.Reload(() =>
            {
                Data.Reload(() => Localization.Reload(() => completed?.Invoke()));
            });
        }

        /// <summary>
        /// Get all content bundles from Global and Current Expansion
        /// </summary>
        /// <returns></returns>
        public List<Bundle> GetContentBundles()
        {
            List<Bundle> bundles = new List<Bundle>(ExpansionBundles.ContentBundles);
            bundles.AddRange(GlobalBundles.ContentBundles);
            return bundles;
        }
        /// <summary>
        /// Get content bundles specified by name
        /// </summary>
        /// <param name="contentName">Content names</param>
        /// <returns>List of contents requested</returns>
        public List<Bundle> GetContentBundles(params string[] contentName)
        {
            for (int i = 0; i < contentName.Length; i++)
                contentName[i] = contentName[i].ToLower();
            return GetContentBundles().Where(bundle => contentName.Contains(bundle.Name.ToLower())).ToList();
        }

        /// <summary>
        /// Get all data bundles from Global and Current Expansion
        /// </summary>
        /// <returns></returns>
        public List<Bundle> GetDataBundles()
        {
            List<Bundle> bundles = new List<Bundle>(ExpansionBundles.DataBundles);
            bundles.AddRange(GlobalBundles.DataBundles);
            return bundles;
        }

        /// <summary>
        /// Get all localization bundles from Global and Current Expansion
        /// based on the current language of Playtable
        /// </summary>
        /// <returns>List of localizations requested</returns>
        public List<Bundle> GetLocalizationBundle()
        {
            return GetLocalizationBundle(GameSettings.Language);
        }
        /// <summary>
        /// Get all localization bundles from Global and Current Expansion
        /// from specified language
        /// </summary>
        /// <param name="language">Language to load the bundles</param>
        /// <returns>List of localizations requested</returns>
        public List<Bundle> GetLocalizationBundle(string language)
        {
            List<Bundle> bundles = new List<Bundle>(ExpansionBundles.LocalizationBundles.Where(bundle => bundle.Name.ToLower() == language.ToLower()));
            bundles.AddRange(GlobalBundles.LocalizationBundles
                .Where(bundle => bundle.Name.ToLower() == language.ToLower()));
            return bundles;
        }

        /// <summary>
        /// Load all contents from Global and Current Expansion
        /// </summary>
        /// <param name="completedCallback">Callback when all bundles are loaded</param>
        private void LoadContentBundles(Action completedCallback)
        {
            int loadedBundles = 0;
            StartCoroutine(LoadBundlesRoutine(GlobalBundles.ContentBundles, PlaytableBundlesPath.GetContentsPath(), 
                () =>
                {
                    loadedBundles++;
                    if (loadedBundles == 2)
                        completedCallback?.Invoke();
                }));
            StartCoroutine(LoadBundlesRoutine(ExpansionBundles.ContentBundles,
                PlaytableBundlesPath.GetContentsPath(GameSettings.Expansion),
                () =>
                {
                    loadedBundles++;
                    if (loadedBundles == 2)
                        completedCallback?.Invoke();
                }));
        }
        /// <summary>
        /// Load all datas from Global and Current Expansion
        /// </summary>
        /// <param name="completedCallback">Callback when all bundles are loaded</param>
        private void LoadDataBundles(Action completedCallback)
        {
            int loadedBundles = 0;
            StartCoroutine(LoadBundlesRoutine(GlobalBundles.DataBundles, PlaytableBundlesPath.GetDatasPath(), 
                () =>
                {
                    loadedBundles++;
                    if (loadedBundles == 2)
                        completedCallback?.Invoke();
                }));
            StartCoroutine(LoadBundlesRoutine(ExpansionBundles.DataBundles, 
                PlaytableBundlesPath.GetDatasPath(GameSettings.Expansion), 
                () =>
                {
                    loadedBundles++;
                    if (loadedBundles == 2)
                        completedCallback?.Invoke();
                }));
        }
        /// <summary>
        /// Load all localizations from Global and Current Expansion
        /// </summary>
        /// <param name="completedCallback">Callback when all bundles are loaded</param>
        private void LoadLocalizationBundles(Action completedCallback)
        {
            int loadedBundles = 0;
            StartCoroutine(LoadBundlesRoutine(GlobalBundles.LocalizationBundles, 
                PlaytableBundlesPath.GetLocalizationsPath(), 
                () =>
                {
                    loadedBundles++;
                    if (loadedBundles == 2)
                        completedCallback?.Invoke();
                }));
            StartCoroutine(LoadBundlesRoutine(ExpansionBundles.LocalizationBundles,
                PlaytableBundlesPath.GetLocalizationsPath(GameSettings.Expansion),
                () =>
                {
                    loadedBundles++;
                    if (loadedBundles == 2)
                        completedCallback?.Invoke();
                }));
        }

        /// <summary>
        /// Routine to load bundles
        /// </summary>
        /// <param name="bundles">List to hold a reference to bundles being loaded</param>
        /// <param name="bundlesRootPath">Path where the bundles resides</param>
        /// <param name="completedCallback">Callback when bundles are loaded</param>
        /// <returns>Coroutine from Unity</returns>
        private IEnumerator LoadBundlesRoutine(List<Bundle> bundles, List<string> bundlesRootPath, Action completedCallback)
        {
            foreach (var bundleRootPath in bundlesRootPath)
            {
                string[] bundlesPath = new string[0];
                try
                {
                    bundlesPath = Directory.GetFiles(bundleRootPath, "*.bundle");
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    continue;
                }
                
                if (bundlesPath.Length > 0)
                    bundles.Add(new Bundle(new DirectoryInfo(bundleRootPath).Name));

                _bundlesToLoad += bundlesPath.Length;
                foreach (var bundlePath in bundlesPath)
                {
                    AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
                    yield return bundleRequest;

                    if (bundleRequest.assetBundle == null)
                    {
                        Debug.LogError($"Could not load bundle at path {bundlePath}! We will skip it!");
                        continue;
                    }

                    bundles[bundles.Count - 1].AbsolutePaths.Add(bundlePath);
                    bundles[bundles.Count - 1].AssetBundlesPlusSceneBundles.Add(bundleRequest.assetBundle);

                    _bundlesLoaded++;
                    OnLoading.Invoke(_bundlesLoaded / _bundlesToLoad);
                }
            }

            completedCallback?.Invoke();
        }

        private void OnLoadedCompleted()
        {
            IsLoaded = true;
            OnLoaded.Invoke();
        }
    }
}
