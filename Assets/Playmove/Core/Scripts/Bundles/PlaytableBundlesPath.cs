using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Playmove.Core.Bundles
{
    public class UnityDirInfo
    {
        public string Name { get; private set; }
        public string FullName { get; private set; }

        public UnityDirInfo() { }
        public UnityDirInfo(string path) : this(new DirectoryInfo(path)) { }
        public UnityDirInfo(DirectoryInfo directory)
        {
            Name = directory.Name;
            FullName = directory.FullName.Replace(@"\", "/");
        }

        public override string ToString()
        {
            return FullName;
        }
    }

    /// <summary>
    /// Finds all bundles path for the project runtime and editor
    /// </summary>
    public static class PlaytableBundlesPath
    {
        /// <summary>
        /// Path to bundles when game is run by .exe
        /// </summary>
        public static string BundlesLoadPath
        {
            get { return Directory.GetParent(Application.dataPath).FullName.Replace(@"\", "/") + "/Bundles"; }
        }
        /// <summary>
        /// Path to bundles when game is run by Unity Editor
        /// </summary>
        public static string BundlesLoadPathEditor
        {
            get { return Application.dataPath + "/Bundles"; }
        }
        /// <summary>
        /// Path to bundles assets in Unity Editor
        /// </summary>
        public static string BundlesAssetsPath
        {
            get { return Application.dataPath + "/BundlesAssets"; }
        }

        public static UnityDirInfo GetBuildDirectory(string bundleAssetsPath)
        {
            string bundleAssetsDirName = new DirectoryInfo(bundleAssetsPath).Name;
            string buildPath = bundleAssetsPath.Replace("BundlesAssets", "Bundles");
            try
            {
                if (buildPath.LastIndexOf('_') > -1)
                    buildPath = buildPath.Replace(bundleAssetsDirName, bundleAssetsDirName.Substring(0, bundleAssetsDirName.IndexOf('_')));
            }
            catch { }
            return new UnityDirInfo(buildPath);
        }

        /// <summary>
        /// Get all expansions paths
        /// </summary>
        /// <returns>List with expansion paths</returns>
        public static List<string> GetExpansionPaths()
        {
            List<string> paths = new List<string>();
            try
            {
                foreach (string path in Directory.GetDirectories(GetContextPath(BundleContext.Expansions)))
                    paths.Add(path.Replace(@"\", "/"));
            }
            catch { }
            return paths;
        }
        /// <summary>
        /// Get all expansions names
        /// </summary>
        /// <returns>List with expansion names</returns>
        public static List<string> GetExpansionNames()
        {
            List<string> expansionPaths = GetExpansionPaths();
            for (int i = 0; i < expansionPaths.Count; i++)
                expansionPaths[i] = new DirectoryInfo(expansionPaths[i]).Name;
            return expansionPaths;
        }

        /// <summary>
        /// Get contents from specified expansion
        /// </summary>
        /// <param name="expansionPath">Expansion path to get contents</param>
        /// <returns>List with all contents from the expansion</returns>
        public static List<string> GetContentsPath(string expansionPath)
        {
            List<string> paths = new List<string>();
            try
            {
                foreach (string path in Directory.GetDirectories(GetExpansionBundlePath(expansionPath, BundleType.Content)))
                    paths.Add(path.Replace(@"\", "/"));
            }
            catch { }
            return paths;
        }
        /// <summary>
        /// Get global contents
        /// </summary>
        /// <returns>List with all global contents</returns>
        public static List<string> GetContentsPath()
        {
            List<string> paths = new List<string>();
            try
            {
                foreach (string path in Directory.GetDirectories(GetGlobalBundlePath(BundleType.Content)))
                    paths.Add(path.Replace(@"\", "/"));
            }
            catch { }
            return paths;
        }
        /// <summary>
        /// Get data from specified expansion
        /// </summary>
        /// <param name="expansionPath">Expansion path to get data</param>
        /// <returns>List with all data from the expansion</returns>
        public static List<string> GetDatasPath(string expansionPath)
        {
            List<string> paths = new List<string>();
            try
            {
                foreach (string path in Directory.GetDirectories(GetExpansionBundlePath(expansionPath, BundleType.Data)))
                    paths.Add(path.Replace(@"\", "/"));
            }
            catch { }
            return paths;
        }
        /// <summary>
        /// Get global data
        /// </summary>
        /// <returns>List with all global data</returns>
        public static List<string> GetDatasPath()
        {
            List<string> paths = new List<string>();
            try
            {
                foreach (string path in Directory.GetDirectories(GetGlobalBundlePath(BundleType.Data)))
                    paths.Add(path.Replace(@"\", "/"));
            }
            catch { }
            return paths;
        }
        /// <summary>
        /// Get localization from specified localization
        /// </summary>
        /// <param name="expansionPath">Expansion path to get localization</param>
        /// <returns>List with all localization from the expansion</returns>
        public static List<string> GetLocalizationsPath(string expansionPath)
        {
            List<string> paths = new List<string>();
            try
            {
                foreach (string path in Directory.GetDirectories(GetExpansionBundlePath(expansionPath, BundleType.Localization)))
                    paths.Add(path.Replace(@"\", "/"));
            }
            catch { }
            return paths;
        }
        /// <summary>
        /// Get global localization
        /// </summary>
        /// <returns>List with all global localization</returns>
        public static List<string> GetLocalizationsPath()
        {
            List<string> paths = new List<string>();
            try
            {
                foreach (string path in Directory.GetDirectories(GetGlobalBundlePath(BundleType.Localization)))
                    paths.Add(path.Replace(@"\", "/"));
            }
            catch { }
            return paths;
        }

        public static List<string> GetAvailableLocalizations()
        {
            return GetLocalizationsPath().Select(path =>
                {
                    string[] languageSplit = path.Split('/').LastOrDefault().Split('-');
                    languageSplit[languageSplit.Length - 1] = languageSplit[languageSplit.Length - 1].ToUpper();
                    return string.Join("-", languageSplit);
                }).Distinct().ToList();
        }
        
        /// <summary>
        /// Get bundle type path from an expansion
        /// </summary>
        /// <param name="expansionName">Expansion name</param>
        /// <param name="type">Type of bundle</param>
        /// <returns>Path to bundle type from an expansion</returns>
        public static string GetExpansionBundlePath(string expansionName, BundleType type)
        {
            return $"{GetContextPath(BundleContext.Expansions)}/{expansionName}/{type}";
        }
        /// <summary>
        /// Get bundle type path
        /// </summary>
        /// <param name="type">Type of bundle</param>
        /// <returns>Path to bundle type from global</returns>
        public static string GetGlobalBundlePath(BundleType type)
        {
            return $"{GetContextPath(BundleContext.Global)}/{type}";
        }

        /// <summary>
        /// Get path to the context you want
        /// </summary>
        /// <param name="context">Context Global or Expansion</param>
        /// <returns>Path to the context</returns>
        public static string GetContextPath(BundleContext context)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return $"{BundlesLoadPathEditor}/{context}";
            else
                return $"{BundlesAssetsPath}/{context}";
#else
            return $"{BundlesLoadPath}/{context}";
#endif
        }
    }
}
