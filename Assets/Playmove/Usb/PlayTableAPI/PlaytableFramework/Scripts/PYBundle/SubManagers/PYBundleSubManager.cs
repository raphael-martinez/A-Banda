using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Playmove
{
    public class BundleData
    {
        public string Name = string.Empty;
        public PYBundleVersion Version = new PYBundleVersion();
        public List<string> AbsoluteBundlePaths = new List<string>();
        public List<AssetBundle> Bundles = new List<AssetBundle>();

        public AssetBundle Bundle
        {
            get
            {
                if (Bundles.Count > 0)
                    return Bundles[0];
                else
                    return null;
            }
        }

        public BundleData(string name)
        {
            Name = name;
            AbsoluteBundlePaths = new List<string>();
            Bundles = new List<AssetBundle>();
        }

        public T GetAsset<T>(string assetTag) where T : Object
        {
            Object obj = default(T);

            for (int i = 0; i < Bundles.Count; i++)
            {
                obj = Bundles[i].LoadAsset<T>(assetTag);
                if (obj != null)
                    break;
            }

            return (T)obj;
        }
    }

    public abstract class PYBundleSubManager
    {
        public int AmountBundlesToLoad
        {
            get
            {
                return _localBundlesPath.Count + _globalBundlesPath.Count;
            }
        }

        protected Dictionary<PYBundlePriority, List<BundleData>> _bundles = new Dictionary<PYBundlePriority, List<BundleData>>();

        public Dictionary<PYBundlePriority, List<BundleData>> Bundles
        {
            get { return _bundles; }
            private set { _bundles = value; }
        }

        protected Dictionary<Type, Dictionary<string, Object>> _cachedAssets = new Dictionary<Type, Dictionary<string, Object>>();
        protected Dictionary<string, string> _cachedStringAssets = new Dictionary<string, string>();

        protected List<string> _globalBundlesPath = new List<string>();
        protected List<string> _localBundlesPath = new List<string>();

        public void PreCacheAsset<T>(params string[] assetsTag)
        {
            PYBundleManager.Instance.StartCoroutine(PreCacheAssetRoutine<T>(assetsTag));
        }

        private IEnumerator PreCacheAssetRoutine<T>(string[] assetsTag)
        {
            for (int i = 0; i < assetsTag.Length; i++)
            {
                GetAsset<T>(assetsTag[i], true);
                yield return null;
            }
        }

        public virtual bool ExistAsset(string assetTag)
        {
            // Tries to find asset in Local bundles
            List<BundleData> tempBundles = Bundles[PYBundlePriority.Local];
            for (int i = 0; i < tempBundles.Count; i++)
            {
                for (int i2 = tempBundles[i].Bundles.Count - 1; i2 >= 0; i2--)
                {
                    if (tempBundles[i].Bundles[i2].Contains(assetTag))
                        return true;
                }
            }

            // Tries to find asset in Global bundes
            tempBundles = Bundles[PYBundlePriority.Global];
            for (int i = 0; i < tempBundles.Count; i++)
            {
                for (int i2 = tempBundles[i].Bundles.Count - 1; i2 >= 0; i2--)
                {
                    if (tempBundles[i].Bundles[i2].Contains(assetTag))
                        return true;
                }
            }

            return false;
        }

        [Obsolete("Use ExistAsset(string assetTag) instead")]
        public virtual bool ExistTag<T>(PYBundlePriority priority, string assetTag)
        {
            return ExistAsset(assetTag);
        }

        [Obsolete("Use ExistAsset(string assetTag) instead")]
        public virtual bool ExistTag<T>(PYBundlePriority priority, PYBundleTags assetTag)
        {
            return ExistAsset(assetTag.ToStringTag());
        }

        /// <summary>
        /// Get a Local or a Global asset from this manager.
        /// Local > Global, Local has a greater priority over Global.
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetTag">Asset tag</param>
        /// <param name="defaultValue">Default asset value</param>
        /// <returns>Returns the asset from the Type or default if not found</returns>
        public virtual T GetAsset<T>(string assetTag, bool cacheThis, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(assetTag))
                return defaultValue;

            // If for some reason this bundleManager was not initialized
            if (Bundles.Count == 0)
                return defaultValue;

            object asset = null;
            Type assetType = typeof(T);

            // Try to get asset from cache
            asset = GetCachedAsset(assetTag, assetType);

            if (asset != null)
                return (T)asset;

            // Load object from Local bundles
            asset = GetAssetFromBundles(Bundles[PYBundlePriority.Local], assetTag, assetType);

            // Load object from Global bundles if we not find any in Local
            if (asset == null)
                asset = GetAssetFromBundles(Bundles[PYBundlePriority.Global], assetTag, assetType);

            // If any asset was found we return the default asset,
            // otherwise case the new asset if needed
            if (asset == null)
                asset = defaultValue;
            else if (cacheThis)
                SetAssetInCache(assetTag, (Object)asset);

            return (T)asset;
        }

        public virtual T GetAsset<T>(string assetTag, T defaultValue = default(T))
        {
            return GetAsset<T>(assetTag, true, defaultValue);
        }

        public virtual T GetAsset<T>(PYBundleTags assetTag, T defaultValue = default(T))
        {
            return GetAsset<T>(assetTag.ToStringTag(), defaultValue);
        }

        public virtual List<T> GetAssetsInSequence<T>(string assetTagPrefix, List<T> defaultaValues = null)
        {
            return GetAssetsInSequence<T>(assetTagPrefix, true, defaultaValues);
        }

        public virtual List<T> GetAssetsInSequence<T>(string assetTagPrefix, bool cacheThose, List<T> defaultaValues = null)
        {
            List<T> sequenceAssets = new List<T>();

            // Try to get a asset just with the prefix, we can use like this
            // strings | strings0 | strings1 | ...
            object obj = GetAsset<T>(assetTagPrefix, cacheThose);
            if (obj != null)
                sequenceAssets.Add((T)obj);

            for (int i = 0; ; i++)
            {
                obj = GetAsset<T>(assetTagPrefix + i, cacheThose);
                if (obj != null)
                    sequenceAssets.Add((T)obj);
                else
                    break;
            }

            return sequenceAssets;
        }

        /// <summary>
        /// Clears all assets from a specific type.
        /// Strings are stored in a separete cache so it will not be cleared
        /// </summary>
        public void ClearCache(Type typeOfCache)
        {
            if (_cachedAssets.ContainsKey(typeOfCache))
            {
                _cachedAssets[typeOfCache].Clear();

                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }

        /// <summary>
        /// Clears all asset cache.
        /// Strings are stored in a separete cache so it will not be cleared
        /// </summary>
        public void ClearAllCache()
        {
            _cachedAssets.Clear();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public void ReloadAll(Action callback)
        {
            PYBundleManager.Instance.StartCoroutine(ReloadAllRoutine(callback));
        }

        private IEnumerator ReloadAllRoutine(Action callback)
        {
            foreach (PYBundlePriority key in Bundles.Keys)
            {
                foreach (BundleData bundleData in Bundles[key])
                {
                    for (int i = 0; i < bundleData.Bundles.Count; i++)
                        bundleData.Bundles[i].Unload(true);

                    yield return new WaitForEndOfFrame();
                    bundleData.Bundles.Clear();
                }
            }

            Resources.UnloadUnusedAssets();

            foreach (PYBundlePriority key in Bundles.Keys)
            {
                foreach (BundleData bundleData in Bundles[key])
                {
                    foreach (string bundlePath in bundleData.AbsoluteBundlePaths)
                    {
                        bundleData.Bundles.Add(AssetBundle.LoadFromFile(bundlePath));
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

            if (callback != null)
                callback();
        }

        public abstract int PrepareToLoad();

        public abstract void Load(Action callbackCompleted);

        protected void LoadBundle(Action callbackCompleted)
        {
            PYBundleManager.Instance.StartCoroutine(LoadBundleRoutine(callbackCompleted));
        }

        private IEnumerator LoadBundleRoutine(Action callbackCompleted)
        {
            Bundles.Add(PYBundlePriority.Local, new List<BundleData>());
            Bundles.Add(PYBundlePriority.Global, new List<BundleData>());

            // Create local bundles
            for (int i = 0; i < _localBundlesPath.Count; i++)
            {
                yield return CreateAssetBundleReference(_localBundlesPath[i], PYBundlePriority.Local);
                PYBundleManager.Instance.SendOnLoadingEvent();
            }

            // Create global bundles
            for (int i = 0; i < _globalBundlesPath.Count; i++)
            {
                yield return CreateAssetBundleReference(_globalBundlesPath[i], PYBundlePriority.Global);
                PYBundleManager.Instance.SendOnLoadingEvent();
            }

            yield return VerifyAllBundles();

            // Cache string.json files
            CacheAllStringsAsset();

            if (callbackCompleted != null)
                callbackCompleted();
        }

        public virtual void UnloadBundle(bool unloadAllAssets)
        {
            foreach (PYBundlePriority key in Bundles.Keys)
            {
                for (int i = 0; i < Bundles[key].Count; i++)
                {
                    for (int i2 = 0; i2 < Bundles[key][i].Bundles.Count; i2++)
                    {
                        if (Bundles[key][i].Bundles[i2] != null)
                            Bundles[key][i].Bundles[i2].Unload(unloadAllAssets);
                    }
                }
            }
        }

        private IEnumerator VerifyAllBundles()
        {
            string logContent = string.Format(PYBundleManager.LOG_TIME_PREFIX, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                "Loading bundles from " + this.GetType());
            foreach (PYBundlePriority key in Bundles.Keys)
            {
                foreach (BundleData bundleData in Bundles[key].GetRange(0, Bundles[key].Count))
                {
                    bool shouldRemoveBundleData = false;
                    for (int i = 0; i < bundleData.Bundles.Count; i++)
                    {
                        // Bad assetBundle
                        if (bundleData.Bundles[i] == null)
                        {
                            logContent += " - " + string.Format(PYBundleManager.LOG_TIME_PREFIX, "ERROR",
                                string.Format("{0} assetBundle file {1} has some kind of file corruption. File path {2}",
                                key, bundleData.Name, bundleData.AbsoluteBundlePaths[i]));
                            shouldRemoveBundleData = true;
                        }
                        // Good assetBundle
                        else
                        {
                            // Dont have version.xml file
                            if (bundleData.Version.Version == "-1.-1.-1.-1")
                            {
                                logContent += " - " + string.Format(PYBundleManager.LOG_TIME_PREFIX, "SUCCESS-WARNING",
                                    string.Format("{0} assetBundle file {1} it's okay, but version.xml file is missing! File path {2}",
                                    key, bundleData.Name, bundleData.AbsoluteBundlePaths[i]));
                            }
                            else if (bundleData.Version.IsReadable)
                            {
                                logContent += " - " + string.Format(PYBundleManager.LOG_TIME_PREFIX, "SUCCESS",
                                    string.Format("{0} assetBundle file {1} v{2} it's successfully loaded! File path {3}",
                                    key, bundleData.Name, bundleData.Version, bundleData.AbsoluteBundlePaths[i]));
                            }
                            else
                            {
                                logContent += " - " + string.Format(PYBundleManager.LOG_TIME_PREFIX, "SUCCESS-WARNING",
                                    string.Format("{0} assetBundle file {1} v{2} it's okay, but it's a old version and is not supported anymore! File path {3}",
                                    key, bundleData.Name, bundleData.Version, bundleData.AbsoluteBundlePaths[i]));
                            }
                        }
                    }

                    // Remove bundleData with it has bad assetBundle
                    if (shouldRemoveBundleData)
                        Bundles[key].Remove(bundleData);

                    yield return new WaitForEndOfFrame();
                }
            }
            PYBundleManager.Instance.WriteLog(logContent);
        }

        protected void CacheAllStringsAsset()
        {
            string STRINGS_TAG = "string";
            TextAsset jsonString = GetAsset<TextAsset>(STRINGS_TAG, false);
            if (jsonString != null)
                CacheStringsFile(jsonString);

            // Cache sequence strings file if exist
            for (int i = 0; ; i++)
            {
                jsonString = GetAsset<TextAsset>(STRINGS_TAG + i, false);
                if (jsonString != null)
                    CacheStringsFile(jsonString);
                else
                    break;
            }
        }

        protected void CacheStringsFile(TextAsset jsonString)
        {
            String cleanLinebreak = jsonString.text.Replace(@"\\n", "\n");
            List<LocalizationData> jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LocalizationData>>(cleanLinebreak);
            foreach (LocalizationData data in jsonObject)
            {
                if (!_cachedStringAssets.ContainsKey(data.TagNome))
                    _cachedStringAssets.Add(data.TagNome, data.Texto);
            }
        }

        protected void SetAssetInCache(string assetTag, Object obj)
        {
            Type typeofT = obj.GetType();
            if (!_cachedAssets.ContainsKey(typeofT))
                _cachedAssets.Add(typeofT, new Dictionary<string, Object>());
            if (!_cachedAssets[typeofT].ContainsKey(assetTag))
                _cachedAssets[typeofT].Add(assetTag, obj);
        }

        protected object GetCachedAsset(string assetTag, Type typeofT)
        {
            object obj = null;

            // If the requested type is a string we check in string cache
            if (typeofT == typeof(string))
            {
                if (_cachedStringAssets.ContainsKey(assetTag))
                    obj = string.IsNullOrEmpty(_cachedStringAssets[assetTag]) ? null : _cachedStringAssets[assetTag];
                return obj;
            }

            // Check if this requested asset is on cache
            if (_cachedAssets.ContainsKey(typeofT) &&
                _cachedAssets[typeofT].ContainsKey(assetTag))
            {
                obj = _cachedAssets[typeofT][assetTag];
                if (obj != null)
                    return obj;
            }

            return obj;
        }

        protected object GetAssetFromBundles(List<BundleData> bundles, string assetTag, Type assetType)
        {
            object asset = null;

            for (int i = 0; i < bundles.Count; i++)
            {
                for (int i2 = bundles[i].Bundles.Count - 1; i2 >= 0; i2--)
                {
                    asset = bundles[i].Bundles[i2].LoadAsset(assetTag, assetType);
                    if (asset != null)
                        break;
                }
                if (asset != null) break;
            }

            return asset;
        }

        private IEnumerator CreateAssetBundleReference(string bundlePath, PYBundlePriority priority)
        {
            DirectoryInfo dir = new DirectoryInfo(bundlePath);
            BundleData data = Bundles[priority].Find((t) => dir.Parent.Name == t.Name);
            if (data == null)
            {
                data = new BundleData(dir.Parent.Name.Split('.')[0]);
                Bundles[priority].Add(data);
            }

            AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return bundleRequest;

            data.Bundles.Add(bundleRequest.assetBundle);
            data.AbsoluteBundlePaths.Add(bundlePath);

            if (bundleRequest.assetBundle != null)
            {
                // Load PYBundleVersion for this bundleData
                TextAsset versionFile = bundleRequest.assetBundle.LoadAsset<TextAsset>("version");
                if (versionFile == null)
                {
                    Debug.LogWarning(string.Format("PYBUNDLE ({0}): Reading bundle ({1}) version.xml file not found!",
                        this.ToString().Replace("PYBundleSystem.", ""), data.Name));
                }
                else
                {
                    data.Version = PYXML.DeserializerFromContent<PYBundleVersion>(versionFile.text);

                    // Debug for unity output file
#if !UNITY_EDITOR
                Debug.LogWarning(string.Format("PYBUNDLE ({0}): Reading bundle ({1}) version: ({2}) creationDate: ({3}) isReadable/Uptodate: ({4})",
                    this.ToString().Replace("PYBundleSystem.", ""), data.Name, data.Version.Version, data.Version.CreationDate, data.Version.IsReadable));
#endif
                }
            }
        }
    }
}