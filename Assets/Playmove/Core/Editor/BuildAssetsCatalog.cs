using Playmove.Core.Bundles;
using Playmove.Core.Editor.Bundles;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Playmove.Core.Editor
{
    public static class BuildAssetsCatalog
    {
        private static string _assetsCatalogScriptPath;
        private static string AssetsCatalogScriptPath
        {
            get
            {
                if (string.IsNullOrEmpty(_assetsCatalogScriptPath))
                    _assetsCatalogScriptPath = Directory.GetFiles(Application.dataPath, "AssetsCatalog.cs", SearchOption.AllDirectories)[0];
                return _assetsCatalogScriptPath;
            }
        }

        [MenuItem("Playmove/Build Assets Catalog")]
        public static void Build()
        {
            string catalogPath = AssetDatabase.FindAssets("AssetsCatalog").Select(guid =>
                AssetDatabase.GUIDToAssetPath(guid)).Where(path => path.EndsWith(".asset")).FirstOrDefault();
            AssetsCatalog catalog = AssetDatabase.LoadAssetAtPath<AssetsCatalog>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<AssetsCatalog>();
                if (!AssetDatabase.IsValidFolder("Assets/AssetsCatalog"))
                    AssetDatabase.CreateFolder("Assets", "AssetsCatalog");
                AssetDatabase.CreateAsset(catalog, "Assets/AssetsCatalog/AssetsCatalog.asset");
            }

            catalog.Assets = catalog.Assets.Where(asset => asset != null).ToList();
            catalog.IgnoredAssets = catalog.IgnoredAssets.Where(asset => asset != null).ToList();

            // Verify if we have any asset that is already created but itn's in the catalog
            var playAssetGUIDs = AssetDatabase.FindAssets("t:PlayAsset", new[] { "Assets/AssetsCatalog" });
            foreach (var guid in playAssetGUIDs)
            {
                PlayAsset asset = AssetDatabase.LoadAssetAtPath<PlayAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null && catalog.Assets.Find(item => item.AssetName == asset.AssetName && item.Type == asset.Type) == null)
                    catalog.Assets.Add(asset);
            }

            // Create new assets if necessary
            CreatePlayAssetsFromUnityObjects(catalog);
            CreatePlayAssetsFromLocalizationStrings(catalog);

            // Remove assets that it's folder is ignored
            var assetsThatAreInIgnoredFolders = catalog.Assets.Where(asset => catalog.IsFolderIgnored(asset.RelativePath)).ToList();
            foreach (var asset in assetsThatAreInIgnoredFolders)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
                catalog.Assets.Remove(asset);
            }

            BuildCatalogScript(catalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (AssetsCatalogEditor.HasWarningInCatalog(catalog))
            {
                EditorUtility.DisplayDialog("Assets Catalog Warning", "Your catalog has some assets that need's your attention.", "Fix");
                Selection.activeObject = catalog;
            }
            else
                EditorUtility.DisplayDialog("Assets Catalog", "Your catalog has been build successfully!", "Ok");
        }

        public static void BuildCatalogScript(AssetsCatalog assetsCatalog)
        {
            string catalogContent = "using System.Collections.Generic;\n" +
                "using UnityEngine;\n\nnamespace Playmove.Core.Bundles\n{\n" +
                "\tpublic partial class AssetsCatalog : ScriptableObject\n\t{\n" +
                "\t\tpublic List<PlayAsset> Assets = new List<PlayAsset>();\n";

            List<PlayAsset> assets = assetsCatalog.Assets.Where(asset => !asset.Ignore).OrderBy(asset => asset.Type).ToList();
            List<string> fixedTags = new List<string>();
            foreach (var asset in assets)
            {
                if (string.IsNullOrEmpty(asset.Tag)) continue;
                string assetTag = $"{asset.Type}_{asset.Tag}";
                int tagRepeatedAmount = 0;
                string fixedAssetTag = assetTag;
                while (fixedTags.Contains(fixedAssetTag))
                {
                    tagRepeatedAmount++;
                    fixedAssetTag = assetTag + tagRepeatedAmount;
                }

                catalogContent += $"\t\tpublic const string {fixedAssetTag} = \"{asset.AssetName}\";\n";
                fixedTags.Add(fixedAssetTag);
            }

            catalogContent += "\t}\n}";
            File.WriteAllText(AssetsCatalogScriptPath, catalogContent);
            AssetDatabase.Refresh();
        }

        public static PlayAsset CreatePlayAsset(string assetTag, string assetName, string assetType, string unityRelativePath)
        {
            PlayAsset asset = ScriptableObject.CreateInstance<PlayAsset>();
            asset.name = $"{assetType}_{assetName}".Replace("/", "-");
            asset.Tag = assetTag;
            asset.AssetName = assetName;
            asset.Type = assetType;
            asset.RelativePath = unityRelativePath.Replace("\\", "/").Replace(Application.dataPath, string.Empty);
            return asset;
        }

        private static void CreatePlayAssetsFromUnityObjects(AssetsCatalog catalog)
        {
            List<string> unityAssetsPath = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Select(path => path.Replace(Application.dataPath, "Assets").Replace(@"\", "/"))
                .Where(path => (!path.Contains("/.git/") && 
                !path.Contains("TextMesh") && 
                !path.EndsWith(".meta")) && 
                (path.Contains("/Core/Resources/") || path.Contains("/BundlesAssets/")))
                .ToList();

            foreach (var path in unityAssetsPath)
            {
                string assetType = AssetDatabase.GetMainAssetTypeAtPath(path).FullName.Split('.').LastOrDefault();
                string assetTag = path.Split('/').LastOrDefault().Split('.').FirstOrDefault();
                if (catalog.IsFolderIgnored(path)) continue;

                PlayAsset asset = null;
                if (path.Contains("/Resources/"))
                {
                    string assetName = path.Substring(path.IndexOf("/Resources/") + 11).Split('.').FirstOrDefault();
                    if (!catalog.CanCreateAsset(assetName, assetType))
                    {
                        UpdateUnityPlayAsset(catalog, assetName, assetType, path.Substring(6));
                        continue;
                    }
                    asset = CreatePlayAsset(assetTag, assetName, assetType, path.Substring(6));
                }
                else
                {
                    if (!catalog.CanCreateAsset(assetTag, assetType))
                    {
                        UpdateUnityPlayAsset(catalog, assetTag, assetType, path.Substring(6));
                        continue;
                    }
                    asset = CreatePlayAsset(assetTag, assetTag, assetType, path.Substring(6));
                }
                
                AssetDatabase.CreateAsset(asset, $"Assets/AssetsCatalog/{asset.name}.asset");
                catalog.Assets.Add(asset);
            }

            unityAssetsPath.Clear();
        }
        private static void CreatePlayAssetsFromLocalizationStrings(AssetsCatalog catalog)
        {
            foreach (var keyValue in Localization.EDITOR_GetLocalizedStrings())
            {
                if (catalog.IsFolderIgnored(keyValue.Value)) continue;
                if (!catalog.CanCreateAsset(keyValue.Key, "string"))
                {
                    UpdateStringPlayAsset(catalog, keyValue.Key, keyValue.Value);
                    continue;
                }

                PlayAsset asset = CreatePlayAsset(keyValue.Key, keyValue.Key, "string", keyValue.Value);
                AssetDatabase.CreateAsset(asset, $"Assets/AssetsCatalog/{asset.name}.asset");
                catalog.Assets.Add(asset);
            }
        }

        private static void UpdateStringPlayAsset(AssetsCatalog catalog, string assetName, string unityRelativePath)
        {
            PlayAsset pAsset = catalog.GetAsset(assetName, "string");
            if (pAsset != null)
                pAsset.RelativePath = unityRelativePath.Replace("\\", "/").Replace(Application.dataPath, string.Empty);
        }
        private static void UpdateUnityPlayAsset(AssetsCatalog catalog, string assetName, string assetType, string unityRelativePath)
        {
            PlayAsset pAsset = catalog.GetAsset(assetName, assetType);
            if (pAsset != null)
                pAsset.RelativePath = unityRelativePath;
        }
    }
}