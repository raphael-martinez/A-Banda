using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Playmove.Core.Bundles
{
    /// <summary>
    /// Class responsable to hold a reference to AssetBundles.
    /// Our bundle may have more than one AssetBundle
    /// </summary>
    public class Bundle
    {
        /// <summary>
        /// Name of the bundle
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Absolute paths for all AssetBundles
        /// </summary>
        public List<string> AbsolutePaths { get; set; }
        /// <summary>
        /// All AssetBundles for this bundle
        /// </summary>
        public List<AssetBundle> AssetBundlesPlusSceneBundles { get; set; }

        public List<AssetBundle> AssetBundles
        {
            get { return AssetBundlesPlusSceneBundles.Where(bundle => !bundle.name.EndsWith("_scenes.bundle")).ToList(); }
        }

        /// <summary>
        /// Gets only the first AssetBundle
        /// </summary>
        public AssetBundle AssetBundle
        {
            get
            {
                if (AssetBundles.Count > 0)
                    return AssetBundles[0];
                else
                    return null;
            }
        }

        public Bundle(string name)
        {
            Name = name;
            AbsolutePaths = new List<string>();
            AssetBundlesPlusSceneBundles = new List<AssetBundle>();
        }

        /// <summary>
        /// Verify if the asset exists in bundle 
        /// </summary>
        /// <param name="assetName">Name of the asset without extension</param>
        /// <returns>True if asset is in the bundle False otherwise</returns>
        public bool Contains(string assetName)
        {
            foreach (var bundle in AssetBundles)
            {
                if (bundle.Contains(assetName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get asset from this bundle
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetName">Name of the asset without extension</param>
        /// <returns>Return the first founded asset</returns>
        public T GetAsset<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return default;

            object asset = default(T);
            foreach (var bundle in AssetBundles)
            {
                asset = bundle.LoadAsset(assetName, typeof(T));
                if (asset != null) break;
            }
            if (asset == null)
                Debug.LogWarning($"Couldn't find asset {assetName} at bundle {Name}");
            return (T)asset;
        }

        /// <summary>
        /// Get all assets that has the same name
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetName">Name of the asset without extension</param>
        /// <returns>Return a list of the found assets or an empty list</returns>
        public List<T> GetAssets<T>(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return default;

            List<T> assets = new List<T>();
            foreach (var bundle in AssetBundles)
            {
                object asset = bundle.LoadAsset(assetName, typeof(T));
                if (asset != null)
                    assets.Add((T)asset);
            }
            if (assets == null || assets.Count == 0)
                Debug.LogWarning($"Couldn't find asset {assetName} at Bundle {Name}");
            return assets;
        }

        public List<T> GetAssets<T>()
        {
            List<T> assets = new List<T>();
            foreach (var bundle in AssetBundles)
            {
                object[] tempAssets = bundle.LoadAllAssets(typeof(T));
                if (tempAssets != null)
                    assets.AddRange(tempAssets.Select(asset => (T)asset));
            }
            return assets;
        }

        /// <summary>
        /// Unload this bundle with all its objects that was loaded
        /// </summary>
        public void Release()
        {
            foreach (var bundle in AssetBundlesPlusSceneBundles)
                bundle.Unload(true);
            AssetBundlesPlusSceneBundles.Clear();
            Resources.UnloadUnusedAssets();
        }

        public void Reload(UnityAction completed)
        {
            Release();
            PlaytableBundle.Instance.StartCoroutine(ReloadRoutine(completed));
        }

        private IEnumerator ReloadRoutine(UnityAction completed)
        {
            foreach (var bundlePath in AbsolutePaths)
            {
                AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return bundleRequest;
                AssetBundlesPlusSceneBundles.Add(bundleRequest.assetBundle);
            }
            completed?.Invoke();
        }
    }
}
