using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Playmove.Core.Bundles
{
    /// <summary>
    /// Responsable to handle bundles that belong to type Localization
    /// </summary>
    public static partial class Localization
    {
        private static List<Bundle> _bundles;
        /// <summary>
        /// All global localization bundles and current expansion localization bundles
        /// </summary>
        public static List<Bundle> Bundles
        {
            get
            {
                if (_bundles == null || _bundles.Count == 0)
                    _bundles = PlaytableBundle.Instance.GetLocalizationBundle();
                return _bundles;
            }
            set { _bundles = value; }
        }

        /// <summary>
        /// Parsed strings from strings.json file
        /// </summary>
        private static readonly Dictionary<string, string> _localizedStrings = new Dictionary<string, string>();

        public static void Initialize()
        {
            _localizedStrings.Clear();
            ParseStringsFile();
            try
            {
                ParseCustomStringsFile();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Couldn't parse custom strings file!\n" + e.ToString());
            }
        }

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
            if (_localizedStrings.ContainsKey(assetName))
                return true;

            foreach (var bundle in Bundles)
            {
                if (bundle.Contains(assetName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get first asset found on localization or a string asset or defaultValue if none is found
        /// </summary>
        /// <typeparam name="T">Asset type, here you can use string type</typeparam>
        /// <param name="asset">PlayAsset</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Asset requested or defaultValue</returns>
        public static T GetAsset<T>(PlayAsset asset, T defaultValue = default)
        {
            return GetAsset<T>(asset.AssetName, defaultValue);
        }
        /// <summary>
        /// Get first asset found on localization or a string asset or defaultValue if none is found
        /// </summary>
        /// <typeparam name="T">Asset type, here you can use string type</typeparam>
        /// <param name="assetName">Asset name</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Asset requested or defaultValue</returns>
        public static T GetAsset<T>(string assetName, T defaultValue = default)
        {
            object asset = default(T);

            try
            {
                if (_localizedStrings.ContainsKey(assetName))
                {
                    if (typeof(T) == typeof(string))
                        asset = _localizedStrings[assetName];
                }

                if (asset == null)
                {
                    foreach (var bundle in Bundles)
                    {
                        asset = bundle.GetAsset<T>(assetName);
                        if (asset != null) break;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.ToString());
            }

            return asset != null ? (T)asset : defaultValue;
        }

        /// <summary>
        /// Get all assets found on all bundles of the current Playtable localization
        /// </summary>
        /// <typeparam name="T">Asset type, here you can use string type</typeparam>
        /// <param name="asset">PlayAsset</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Assets requested or defaultValue</returns>
        public static List<T> GetAssets<T>(PlayAsset asset, List<T> defaultValue = default)
        {
            return GetAssets<T>(asset.AssetName, defaultValue);
        }
        /// <summary>
        /// Get all assets found on all bundles of the current Playtable localization
        /// </summary>
        /// <typeparam name="T">Asset type, here you can use string type</typeparam>
        /// <param name="assetName">Asset name</param>
        /// <param name="defaultValue">DefaultValue in case none asset is found if not specified null will be default</param>
        /// <returns>Assets requested or defaultValue</returns>
        public static List<T> GetAssets<T>(string assetName, List<T> defaultValue = default)
        {
            List<T> assets = new List<T>();
            try
            {
                if (typeof(T) == typeof(string))
                {
                    if (_localizedStrings.ContainsKey(assetName))
                    {
                        object hack = _localizedStrings[assetName];
                        assets.Add((T)hack);
                        return assets;
                    }
                }
                else
                {
                    foreach (var bundle in Bundles)
                        assets.AddRange(bundle.GetAssets<T>(assetName) ?? new List<T>());
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }

            return assets.Count != 0 ? assets : defaultValue;
        }
        public static List<T> GetAssets<T>(List<T> defaultValue = default)
        {
            List<T> assets = new List<T>();
            try
            {
                if (typeof(T) == typeof(string))
                    return _localizedStrings.Select(keyValue => { object hack = keyValue.Value; return (T)hack; }).ToList();
                else
                {
                    foreach (var bundle in Bundles)
                        assets.AddRange(bundle.GetAssets<T>() ?? new List<T>());
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }

            return assets.Count != 0 ? assets : defaultValue;
        }

        /// <summary>
        /// Parse strings.json file that this localization may have
        /// </summary>
        private static void ParseStringsFile()
        {
            List<TextAsset> stringsAssets = GetAssets<TextAsset>("strings");
            if (stringsAssets == null || stringsAssets.Count == 0) return;

            var dummyType = new[] { new { TagNome = "", Texto = "" } };
            foreach (var stringsAsset in stringsAssets)
            {
                try
                {
                    var localizedTags = JsonConvert.DeserializeAnonymousType(stringsAsset.text, dummyType);
                    foreach (var localized in localizedTags)
                    {
                        if (_localizedStrings.ContainsKey(localized.TagNome))
                        {
                            Debug.LogWarning($"Strings file contains duplicated tags!" +
                                $" Tag1: {localized.TagNome}, Value1: {localized.Texto} |" +
                                $" Value2: {_localizedStrings[localized.TagNome]}");
                            continue;
                        }
                        _localizedStrings.Add(localized.TagNome, localized.Texto);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Couldn't parse strings file!\n" + e.ToString());
                }
            }
        }

        /// <summary>
        /// This should be implemented by a partial class on your own project
        /// Just in case you want to parse your on localized text.
        /// For more information contact Playmove support.
        /// </summary>
        static partial void ParseCustomStringsFile();

#if UNITY_EDITOR
        private static Dictionary<string, string> EDITOR_LocalizedStrings = new Dictionary<string, string>();
        public static Dictionary<string, string> EDITOR_GetLocalizedStrings()
        {
            EDITOR_LocalizedStrings.Clear();
            EDITOR_ParseStringsFile();
            try
            {
                EDITOR_ParseCustomStringsFile();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Couldn't parse custom strings file!\n" + e.ToString());
            }
            return EDITOR_LocalizedStrings;
        }
        private static void EDITOR_ParseStringsFile()
        {
            if (!Directory.Exists(Application.dataPath + "/BundlesAssets/")) return;
            IEnumerable<string> assetsPath = Directory.GetFiles(Application.dataPath + "/BundlesAssets/", "strings.json", SearchOption.AllDirectories);
            foreach (var assetPath in assetsPath)
            {
                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath.Replace(Application.dataPath, "Assets"));
                if (asset == null) continue;
                var dummyType = new[] { new { TagNome = "", Texto = "" } };
                try
                {
                    var localizedTags = JsonConvert.DeserializeAnonymousType(asset.text, dummyType);
                    foreach (var localized in localizedTags)
                    {
                        if (!EDITOR_LocalizedStrings.ContainsKey(localized.TagNome))
                            EDITOR_LocalizedStrings.Add(localized.TagNome, assetPath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Couldn't parse strings file!\n" + e.ToString());
                }
            }
        }
        static partial void EDITOR_ParseCustomStringsFile();
#endif
    }
}
