using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

namespace Playmove
{
    /// <summary>
    /// Responsible to have methods that facilitates the bundle search
    /// in the Bundles folder. It just returns the paths for all assetBundles.
    /// </summary>
    public static class PYBundleFolderScanner
    {
        public const string EXPANSION_ASSETS_FOLDERS = "/BundlesAssets/Expansions/{0}/{1}/";
        public const string GLOBAL_ASSETS_FOLDERS = "/BundlesAssets/Global/{0}/";

        public const string EXPANSION_FOLDERS = "/Bundles/Expansions/{0}/{1}/";
        public const string EXPANSION_LOCALIZED_FOLDERS = "/Bundles/Expansions/{0}/{1}/{2}/";
        public const string EXPANSION_CONTENT_FOLDERS = "/Bundles/Expansions/{0}/{1}/{2}/";
        public const string GLOBAL_FOLDERS = "/Bundles/Global/{0}/";
        public const string GLOBAL_LOCALIZED_FOLDERS = "/Bundles/Global/{0}/{1}/";

        public const string EXPANSION_LOCALIZATION_FOLDERS = "/Bundles/Expansions/{0}/Localization/{1}/";
        public const string GLOBAL_LOCALIZATION_FOLDERS = "/Bundles/Global/Localization/{0}/";

        public static List<string> GetGlobalBundlesPath(PYBundleType type)
        {
            string path = GetApplicationPath() + string.Format(GLOBAL_FOLDERS, type);
            return GetBundlesPath(path, "*.unity3d");
        }
        public static List<string> GetGlobalLocalizedBundlesPath(PYBundleType type, string language)
        {
            string path = GetApplicationPath() + string.Format(GLOBAL_LOCALIZED_FOLDERS, type, language);
            return GetBundlesPath(path, "*.unity3d");
        }
        public static List<string> GetGlobalLocalizationBundlesPath(string language)
        {
            string path = GetApplicationPath() + string.Format(GLOBAL_LOCALIZATION_FOLDERS, language);
            return GetBundlesPath(path, "*.unity3d");
        }

        public static List<string> GetExpansionBundlesPath(string expansionName, PYBundleType type)
        {
            string path = GetApplicationPath() + string.Format(EXPANSION_FOLDERS, expansionName, type);
            return GetBundlesPath(path, "*.unity3d");
        }
        public static List<string> GetExpansionLocalizedBundlesPath(string expansionName, PYBundleType type, string language)
        {
            string path = GetApplicationPath() + string.Format(EXPANSION_LOCALIZED_FOLDERS, expansionName, type, language);
            return GetBundlesPath(path, "*.unity3d");
        }

        [Obsolete("")]
        public static List<string> GetAllContentHeadersPath(string expansionName, string language)
        {
            string path = GetApplicationPath() + string.Format(EXPANSION_CONTENT_FOLDERS, expansionName, PYBundleType.Content, language);
            return GetBundlesPath(path, "Header.unity3d");
        }
        [Obsolete("")]
        public static string GetContentPathFromExpansion(string expansionName, string contentName)
        {
            string path = GetApplicationPath() + string.Format(EXPANSION_CONTENT_FOLDERS, expansionName, PYBundleType.Content) + "/" + contentName;
            return GetBundlesPath(path, "Content.unity3d")[0];
        }

        public static string GetApplicationPath()
        {
            string path = Application.dataPath;
#if !UNITY_EDITOR
        path = Directory.GetParent(path).FullName;
#endif
            return path;
        }

        private static List<string> GetBundlesPath(string path, string searchPattern)
        {
            List<string> paths = new List<string>();
            if (Directory.Exists(path))
                paths.AddRange(Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories));
            return paths;
        }
    }
}