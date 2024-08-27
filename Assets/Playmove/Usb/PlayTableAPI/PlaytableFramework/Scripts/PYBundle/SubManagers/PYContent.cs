using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

using Object = UnityEngine.Object;

namespace Playmove
{
    public class ContentAsset<T>
    {
        public T Asset;
        public BundleData BundleData;
        public string BundleName
        {
            get { return BundleData.Name; }
        }

        public ContentAsset(BundleData bundleData, T asset)
        {
            BundleData = bundleData;
            Asset = asset;
        }
    }

    /// <summary>
    /// Responsible for all Game Content bundles
    /// </summary>
    public partial class PYContent : PYBundleSubManager
    {
        private List<BundleData> _currentContentBundles = new List<BundleData>();
        public List<BundleData> CurrentContentBundles
        {
            get { return _currentContentBundles; }
            private set { _currentContentBundles = value; }
        }
        public BundleData CurrentContentBundle
        {
            get
            {
                return CurrentContentBundles.Count == 0 ? null : CurrentContentBundles[0];
            }
        }

        public override int PrepareToLoad()
        {
            _globalBundlesPath = PYBundleFolderScanner.GetGlobalLocalizedBundlesPath(PYBundleType.Content, PYBundleManager.Instance.Language);
            if (_globalBundlesPath.Count == 0)
                _globalBundlesPath = PYBundleFolderScanner.GetGlobalBundlesPath(PYBundleType.Content);

            _localBundlesPath = PYBundleFolderScanner.GetExpansionLocalizedBundlesPath(PYBundleManager.Instance.ExpansionName,
                PYBundleType.Content, PYBundleManager.Instance.Language);
            if (_localBundlesPath.Count == 0)
                _localBundlesPath = PYBundleFolderScanner.GetExpansionBundlesPath(PYBundleManager.Instance.ExpansionName, PYBundleType.Content);

            return _globalBundlesPath.Count + _localBundlesPath.Count;
        }

        public override void Load(Action callbackCompleted)
        {
            LoadBundle(callbackCompleted);
        }

        public BundleData GetBundleByName(string bundleName)
        {
            BundleData data = null;
            foreach (PYBundlePriority key in Bundles.Keys)
            {
                data = Bundles[key].Find((bundleData) => bundleData.Name == bundleName);
                if (data != null)
                    return data;
            }
            return data;
        }

        public void SetCurrentContent(string bundleName)
        {
            CurrentContentBundles.Clear();
            foreach (PYBundlePriority key in Bundles.Keys)
            {
                for (int i = 0; i < Bundles[key].Count; i++)
                {
                    if (Bundles[key][i].Name == bundleName)
                    {
                        CurrentContentBundles.Add(Bundles[key][i]);
                    }
                }
            }
        }
        public void SetCurrentContent(BundleData bundleData)
        {
            CurrentContentBundles.Clear();
            CurrentContentBundles.Add(bundleData);
        }

        public void SetCurrentContents(params string[] bundlesName)
        {
            CurrentContentBundles.Clear();
            foreach (string name in bundlesName)
            {
                foreach (PYBundlePriority key in Bundles.Keys)
                {
                    for (int i = 0; i < Bundles[key].Count; i++)
                    {
                        if (Bundles[key][i].Name == name)
                            CurrentContentBundles.Add(Bundles[key][i]);
                    }
                }
            }
        }
        public void SetCurrentContents(params BundleData[] bundlesData)
        {
            CurrentContentBundles.Clear();
            CurrentContentBundles.AddRange(bundlesData);
        }

        /// <summary>
        /// Used usually in MainMenu to get a Icon for the content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetTag"></param>
        /// <returns></returns>
        public List<ContentAsset<T>> GetAssetsFromAllContents<T>(string assetTag)
        {
            if (string.IsNullOrEmpty(assetTag))
                return new List<ContentAsset<T>>();

            List<ContentAsset<T>> assets = new List<ContentAsset<T>>();
            Type assetType = typeof(T);

            // Load object from Local bundles
            assets.AddRange(GetContentAssetsFromBundles<T>(Bundles[PYBundlePriority.Local], assetTag, assetType));

            // Load object from Global bundles if we not find any in Local
            assets.AddRange(GetContentAssetsFromBundles<T>(Bundles[PYBundlePriority.Global], assetTag, assetType));

            return assets;
        }

        public override bool ExistAsset(string assetTag)
        {
            if (CurrentContentBundles.Count == 0)
                return false;

            for (int i = 0; i < CurrentContentBundles.Count; i++)
            {
                for (int i2 = 0; i2 < CurrentContentBundles[i].Bundles.Count; i2++)
                {
                    if (CurrentContentBundles[i].Bundles[i2].Contains(assetTag))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return a asset from selected content in MainMenu for example
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetTag"></param>
        /// <param name="cacheThis"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override T GetAsset<T>(string assetTag, bool cacheThis, T defaultValue = default(T))
        {
            return GetAsset<T>(assetTag, defaultValue);
        }
        /// <summary>
        /// Return a asset from selected content in MainMenu for example
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetTag"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override T GetAsset<T>(string assetTag, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(assetTag) || CurrentContentBundles.Count == 0)
                return default(T);

            object asset = null;
            Type assetType = typeof(T);

            // TODO: Cached assets and strings dont work in PYContent
            // Improve this later
            // Try to get asset from cache
            //asset = GetCachedAsset(assetTag, assetType);
            //if (asset != null)
            //    return (T)asset;

            asset = GetAssetFromBundles(CurrentContentBundles, assetTag, assetType);

            // If any asset was found we return the default asset,
            // otherwise case the new asset if needed
            if (asset == null)
                asset = defaultValue;
            //else if (cacheThis)
            //    SetAssetInCache(assetTag, (Object)asset);

            return (T)asset;
        }
        /// <summary>
        /// Returns a list of assets from all selected contents in MainMenu for example
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetTag"></param>
        /// <param name="defaultValues"></param>
        /// <returns></returns>
        public List<ContentAsset<T>> GetAssets<T>(string assetTag, List<ContentAsset<T>> defaultValues = null)
        {
            if (string.IsNullOrEmpty(assetTag) || CurrentContentBundles.Count == 0)
                return new List<ContentAsset<T>>();

            List<ContentAsset<T>> assets = new List<ContentAsset<T>>();
            Type assetType = typeof(T);

            assets.AddRange(GetContentAssetsFromBundles<T>(CurrentContentBundles, assetTag, assetType));

            return assets;
        }

        private List<ContentAsset<T>> GetContentAssetsFromBundles<T>(List<BundleData> bundles, string assetTag, Type assetType)
        {
            List<ContentAsset<T>> assets = new List<ContentAsset<T>>();
            for (int i = 0; i < bundles.Count; i++)
            {
                for (int i2 = bundles[i].Bundles.Count - 1; i2 >= 0; i2--)
                {
                    object obj = bundles[i].Bundles[i2].LoadAsset(assetTag, assetType);

                    // We dont add same objects found in same bundles
                    object objTemp = assets.Find((o) => (o.Asset.ToString() == obj.ToString()) && (o.BundleName == bundles[i].Name));
                    if (objTemp == null)
                        assets.Add(new ContentAsset<T>(bundles[i], (T)obj));
                }
            }
            return assets;
        }
    }
}