using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Playmove.Core.Bundles
{
    /// <summary>
    /// Responsable to handle bundles that belong to type Data.
    /// With this you can load assets from Unity Resources folder too.
    /// </summary>
    public static partial class Data
    {
        private static List<Bundle> _bundles;
        /// <summary>
        /// All global data bundles and current expansion data bundles
        /// </summary>
        public static List<Bundle> Bundles
        {
            get
            {
                if (_bundles == null || _bundles.Count == 0)
                    _bundles = PlaytableBundle.Instance.GetDataBundles();
                return _bundles;
            }
            private set { _bundles = value; }
        }

        public static void Initialize() { }

        public static void Reload(UnityAction completed)
        {
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
            foreach (var bundle in Bundles)
            {
                if (bundle.Contains(assetName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get first asset found on Data bundles or fallback to Unity Resources folder
        /// if any asset is found null will be returned
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="asset">PlayAsset</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Asset requested or null</returns>
        public static T GetAsset<T>(PlayAsset asset, T defaultValue = default)
        {
            return GetAsset<T>(asset.AssetName, defaultValue);
        }
        /// <summary>
        /// Get first asset found on Data bundles or fallback to Unity Resources folder
        /// if any asset is found null will be returned
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetName">Asset name or relative path to an asset in Resources folder</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Asset requested or null</returns>
        public static T GetAsset<T>(string assetName, T defaultValue = default)
        {
            object asset = default(T);
            foreach (var bundle in Bundles)
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

            if (asset == null)
            {
                asset = GetResourceAsset<T>(assetName);
                return asset != null ? (T)asset : defaultValue;
            }
            else
                return (T)asset;            
        }

        /// <summary>
        /// Get all assets found on all data bundles if none is found try to get an asset from Unity Resources
        /// </summary>
        /// <typeparam name="T">Asset type, here you can use string type</typeparam>
        /// <param name="asset">PlayAsset</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Assets requested or defaultValue</returns>
        public static List<T> GetAssets<T>(PlayAsset asset, List<T> defaultValue = null)
        {
            return GetAssets<T>(asset.AssetName, defaultValue);
        }
        /// <summary>
        /// Get all assets found on all data bundles if none is found try to get an asset from Unity Resources
        /// </summary>
        /// <typeparam name="T">Asset type, here you can use string type</typeparam>
        /// <param name="assetName">Asset name</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Assets requested or defaultValue</returns>
        public static List<T> GetAssets<T>(string assetName, List<T> defaultValue = null)
        {
            List<T> assets = new List<T>();
            try
            {
                foreach (var bundle in Bundles)
                    assets.AddRange(bundle.GetAssets<T>(assetName) ?? new List<T>());

                if (assets.Count == 0)
                {
                    object asset = GetResourceAsset<T>(assetName);
                    if (asset != null)
                        assets.Add((T)asset);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }

            return assets.Count != 0 ? assets : defaultValue;
        }
        public static List<T> GetAssets<T>(bool includeResourceAssets = false, List<T> defaultValue = null)
        {
            List<T> assets = new List<T>();
            try
            {
                foreach (var bundle in Bundles)
                    assets.AddRange(bundle.GetAssets<T>() ?? new List<T>());

                if (includeResourceAssets)
                    assets.AddRange(GetResourceAssets<T>() ?? new List<T>());
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }

            return assets.Count != 0 ? assets : defaultValue;
        }

        /// <summary>
        /// Get asset from Unity Resources folder
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetName">Asset name, should have the relative path together</param>
        /// <returns>Asset requested or null</returns>
        private static T GetResourceAsset<T>(string assetName)
        {
            object asset = Resources.Load(assetName, typeof(T));
            return (T)asset;
        }

        private static List<T> GetResourceAssets<T>()
        {
            object[] assets = Resources.LoadAll("", typeof(T));
            if (assets == null || assets.Length == 0) return new List<T>();
            return assets.Select(asset => (T)asset).ToList();
        }
    }
}
