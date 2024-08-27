using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Playmove.Core.Bundles
{
    public class ContentAsset<T>
    {
        public Bundle Bundle { get; set; }
        public T Asset { get; set; }

        public ContentAsset(Bundle bundle, T asset)
        {
            Bundle = bundle;
            Asset = asset;
        }
    }

    /// <summary>
    /// Responsable to handle bundles that belong to type Content
    /// </summary>
    public static partial class Content
    {
        private static List<Bundle> _bundles;
        /// <summary>
        /// All global content bundles and current expansion content bundles
        /// </summary>
        public static List<Bundle> Bundles
        {
            get
            {
                if (_bundles == null || _bundles.Count == 0)
                    _bundles = PlaytableBundle.Instance.GetContentBundles();
                return _bundles;
            }
            private set { _bundles = value; }
        }

        /// <summary>
        /// Current selected contents
        /// </summary>
        public static List<Bundle> SelectedContents
        {
            get;
            private set;
        }

        public static void Initialize()
        {
            SelectedContents = new List<Bundle>();
        }

        public static void Reload(UnityAction completed)
        {
            SelectedContents.Clear();
            int amountBundlesToReload = Bundles.Count;
            if (amountBundlesToReload == 0)
            {
                completed?.Invoke();
                return;
            }

            foreach (var bundle in Bundles)
            {
                bundle.Reload(() =>
                {
                    amountBundlesToReload--;
                    if (amountBundlesToReload == 0)
                    {
                        Bundles = null;
                        Initialize();

                        completed?.Invoke();
                    }
                });
            }
        }

        /// <summary>
        /// Verify if the asset exists in bundle 
        /// </summary>
        /// <param name="assetName">Name of the asset without extension</param>
        /// <returns>True if asset is in the bundle False otherwise</returns>
        public static bool Contains(string assetName)
        {
            foreach (var bundle in SelectedContents)
            {
                if (bundle.Contains(assetName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Verify if the asset exists in bundle 
        /// </summary>
        /// <param name="assetName">Name of the asset without extension</param>
        /// <returns>True if asset is in the bundle False otherwise</returns>
        public static bool ContainsInAnyContent(string assetName)
        {
            foreach (var bundle in Bundles)
            {
                if (bundle.Contains(assetName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Select contents
        /// </summary>
        /// <param name="contentsName">Name of the contents to be selected</param>
        public static void SetCurrentContent(params string[] contentsName)
        {
            SetCurrentContent(PlaytableBundle.Instance.GetContentBundles(contentsName).ToArray());
        }
        /// <summary>
        /// Select contents
        /// </summary>
        /// <param name="contentsBundle">Content bundles to be selected</param>
        public static void SetCurrentContent(params Bundle[] contentsBundle)
        {
            if (SelectedContents.Count > 0)
            {
                Debug.LogWarning("You are trying to load more content without Releasing the old ones." +
                    " This could cause a memory problem. Use the method Content.ReleaseContent()");
            }
            SelectedContents = new List<Bundle>(contentsBundle);
        }

        /// <summary>
        /// Get first asset found on SelectedContents or defaultValue will be returned
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="asset">PlayAsset</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Asset requested or defaultValue</returns>
        public static T GetAsset<T>(PlayAsset asset, T defaultValue = default)
        {
            return GetAsset<T>(asset.AssetName, defaultValue);
        }
        /// <summary>
        /// Get first asset found on SelectedContents or defaultValue will be returned
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetName">Asset name</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Asset requested or defaultValue</returns>
        public static T GetAsset<T>(string assetName, T defaultValue = default)
        {
            if (SelectedContents.Count == 0) return defaultValue;

            object asset = default(T);
            foreach (var bundle in SelectedContents)
            {
                try
                {
                    asset = bundle.GetAsset<T>(assetName);
                    if (asset != null) break;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }
            return asset != null ? (T)asset : defaultValue;
        }

        /// <summary>
        /// Get a list of assets from SelectedContents with the same name.
        /// This is valid in case you selected more than one content and you want
        /// files that have the same name but are on different contents
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="asset">PlayAsset</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>List of assets requested or defaultValue</returns>
        public static List<T> GetAssets<T>(PlayAsset asset, List<T> defaultValue = default)
        {
            return GetAssets<T>(asset.AssetName, defaultValue);
        }
        /// <summary>
        /// Get a list of assets from SelectedContents with the same name.
        /// This is valid in case you selected more than one content and you want
        /// files that have the same name but are on different contents
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetName">Asset name</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>List of assets requested or defaultValue</returns>
        public static List<T> GetAssets<T>(string assetName, List<T> defaultValue = default)
        {
            if (SelectedContents.Count == 0) return defaultValue;
            List<T> assets = new List<T>();
            foreach (var bundle in SelectedContents)
                assets.AddRange(bundle.GetAssets<T>(assetName) ?? new List<T>());
            return assets.Count > 0 ? assets : defaultValue;
        }
        public static List<T> GetAssets<T>(List<T> defaultValue = default)
        {
            if (SelectedContents.Count == 0) return defaultValue;
            List<T> assets = new List<T>();
            foreach (var bundle in SelectedContents)
                assets.AddRange(bundle.GetAssets<T>() ?? new List<T>());
            return assets.Count > 0 ? assets : defaultValue;
        }

        /// <summary>
        /// Get a list of asset from all contents that this game have.
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="asset">PlayAsset</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>List of assets requested or defaultValue</returns>
        public static List<T> GetAssetsFromAllContents<T>(PlayAsset asset, List<T> defaultValue = default)
        {
            return GetAssetsFromAllContents(asset.AssetName, defaultValue);
        }
        /// <summary>
        /// Get a list of asset from all contents that this game have.
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetName">Asset name</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>List of assets requested or defaultValue</returns>
        public static List<T> GetAssetsFromAllContents<T>(string assetName, List<T> defaultValue = default)
        {
            List<T> assets = new List<T>();
            foreach (var bundle in PlaytableBundle.Instance.GetContentBundles())
                assets.AddRange(bundle.GetAssets<T>(assetName) ?? new List<T>());
            return assets.Count > 0 ? assets : defaultValue;
        }
        public static List<T> GetAssetsFromAllContents<T>(List<T> defaultValue = default)
        {
            List<T> assets = new List<T>();
            foreach (var bundle in PlaytableBundle.Instance.GetContentBundles())
                assets.AddRange(bundle.GetAssets<T>() ?? new List<T>());
            return assets.Count > 0 ? assets : defaultValue;
        }

        /// <summary>
        /// Get a list of asset from all contents that this game have with a bundle reference.
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="asset">PlayAsset</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>List of assets requested or defaultValue</returns>
        public static List<ContentAsset<T>> GetContentAssetsFromAllContents<T>(PlayAsset asset, List<ContentAsset<T>> defaultValue = default)
        {
            return GetContentAssetsFromAllContents(asset.AssetName, defaultValue);
        }
        /// <summary>
        /// Get a list of asset from all contents that this game have with a bundle reference.
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetName">Asset name</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>List of assets requested or defaultValue</returns>
        public static List<ContentAsset<T>> GetContentAssetsFromAllContents<T>(string assetName, List<ContentAsset<T>> defaultValue = default)
        {
            List<ContentAsset<T>> assets = new List<ContentAsset<T>>();
            foreach (var bundle in PlaytableBundle.Instance.GetContentBundles())
            {
                foreach (var asset in bundle.GetAssets<T>(assetName))
                    assets.Add(new ContentAsset<T>(bundle, asset));
            }
            return assets.Count > 0 ? assets : defaultValue;
        }
        public static List<ContentAsset<T>> GetContentAssetsFromAllContents<T>(List<ContentAsset<T>> defaultValue = default)
        {
            List<ContentAsset<T>> assets = new List<ContentAsset<T>>();
            foreach (var bundle in PlaytableBundle.Instance.GetContentBundles())
            {
                foreach (var asset in bundle.GetAssets<T>())
                    assets.Add(new ContentAsset<T>(bundle, asset));
            }
            return assets.Count > 0 ? assets : defaultValue;
        }

        /// <summary>
        /// Release all SelectedContents
        /// </summary>
        public static void ReleaseContent()
        {
            ReleaseContent(SelectedContents.ToArray());
            SelectedContents.Clear();
        }
        /// <summary>
        /// Release all bundles you specify
        /// </summary>
        /// <param name="contentBundle">Bundles to be released</param>
        public static void ReleaseContent(params Bundle[] contentBundle)
        {
            foreach (var bundle in contentBundle)
                bundle.Release();
        }
    }
}
