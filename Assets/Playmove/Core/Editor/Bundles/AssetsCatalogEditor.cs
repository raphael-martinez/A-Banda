using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Playmove.Core.Bundles;
using System.IO;

namespace Playmove.Core.Editor.Bundles
{
    [CustomEditor(typeof(AssetsCatalog))]
    public class AssetsCatalogEditor : UnityEditor.Editor
    {
        [MenuItem("Playmove/Select Assets Catalog", priority = 1)]
        private static void SelectGameSettings()
        {
            Selection.activeObject = DevKit.AssetsCatalog;
        }

        public static bool HasWarningInCatalog(AssetsCatalog catalog)
        {
            return !PlaytablePrefs.Get<bool>($"{DevKit.ProjectName}/AssetCatalogIgnoreWarnings") &&
                catalog.HasWarning();
        }

        public static void IgnoreAssetsIfNeeded()
        {
            AssetsCatalog catalog = DevKit.AssetsCatalog;
            var assetsToBeIgnored = catalog.Assets.Where(asset => asset != null && asset.Ignore);
            foreach (var asset in assetsToBeIgnored)
            {
                catalog.IgnoredAssets.Add(asset.ToIgnoredAsset());
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
            }
            catalog.Assets = catalog.Assets.Where(asset => asset != null && !asset.Ignore).ToList();

            if (assetsToBeIgnored.Count() > 0)
                BuildAssetsCatalog.Build();
        }

        private AssetsCatalog _catalog = null;
        private Dictionary<string, List<PlayAsset>> _duplicatedAssets = new Dictionary<string, List<PlayAsset>>();
        private List<PlayAsset> _assetsWithoutTag = new List<PlayAsset>();
        private List<PlayAsset> _deletedAssets = new List<PlayAsset>();
        private bool _showDevKitAssets = false;

        private void OnEnable()
        {
            _catalog = DevKit.AssetsCatalog;
            _duplicatedAssets = _catalog.GetDuplicatedAssets();
            _assetsWithoutTag = _catalog.GetAssetsWithoutTag();
            _deletedAssets = _catalog.GetDeletedAssets();

            IgnoreAssetsIfNeeded();
        }

        public override void OnInspectorGUI()
        {
            if (!AssetDatabase.IsValidFolder("Assets/AssetsCatalog") && GUILayout.Button("Unpack"))
            {
                AssetDatabase.CreateFolder("Assets", "AssetsCatalog");

                foreach (var asset in _catalog.Assets)
                {
                    AssetDatabase.RemoveObjectFromAsset(asset);
                    AssetDatabase.CreateAsset(asset, $"Assets/AssetsCatalog/{asset.name.Replace("/", "-")}.asset");
                }
                AssetDatabase.MoveAsset("Assets/AssetsCatalog.asset", "Assets/AssetsCatalog/AssetsCatalog.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GUILayout.Label("Amount of Assets: " + _catalog.Assets.Count, EditorStyles.boldLabel);
            PlaytablePrefs.Set($"{DevKit.ProjectName}/AssetCatalogIgnoreWarnings", EditorGUILayout.Toggle("Ignore warnings",
                PlaytablePrefs.Get<bool>($"{DevKit.ProjectName}/AssetCatalogIgnoreWarnings")));
            _showDevKitAssets = EditorGUILayout.Toggle("Show DevKit Assets", _showDevKitAssets);

            if (DrawFoldout("IgnoredFolders", "Ignored Folders: " + _catalog.IgnoredFolders.Count))
            {
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Screen.width - 40));
                GUILayout.Label("You can use just relative path like: /Global/Content/ this will ignore all content assets", EditorStyles.miniBoldLabel);
                int i = 0;
                foreach (var ignoredFolder in _catalog.IgnoredFolders.GetRange(0, _catalog.IgnoredFolders.Count))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(i.ToString("00"), GUILayout.Width(30));
                    _catalog.IgnoredFolders[i] = GUILayout.TextField(ignoredFolder);
                    if (GUILayout.Button("x", GUILayout.Width(30)))
                    {
                        _catalog.IgnoredFolders.Remove(ignoredFolder);
                        i--;
                    }
                    GUILayout.EndHorizontal();
                    i++;
                }
                if (GUILayout.Button("Add"))
                    _catalog.IgnoredFolders.Add(string.Empty);
                GUILayout.EndVertical();
            }

            var ignoredAssets = _catalog.IgnoredAssets.Where(asset => asset.IsFromDevKit == _showDevKitAssets)
                .OrderBy(asset => asset.Tag).ToList();
            if (DrawFoldout("AssetsCatalog/Ignored Assets", "Ignored Assets: " + ignoredAssets.Count))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Screen.width - 40));
                foreach (var asset in ignoredAssets.GetRange(0, ignoredAssets.Count))
                {
                    GUILayout.BeginHorizontal(GUI.skin.box);
                    GUILayout.Label(asset.AssetName);
                    GUILayout.Label(asset.Type, EditorStyles.miniLabel);
                    if (GUILayout.Button("x", GUILayout.Width(30)))
                    {
                        RestoreIgnoredAsset(asset);
                        _catalog.IgnoredAssets.Remove(asset);
                        BuildAssetsCatalog.Build();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            
            if (DrawFoldout("Assets without tag", "Assets without tag: " + _assetsWithoutTag.Count))
            {
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Screen.width - 40));
                foreach (var asset in _assetsWithoutTag)
                {
                    GUI.color = string.IsNullOrEmpty(asset.Tag) ? Color.white : Color.green;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tag", GUILayout.Width(30));
                    asset.Tag = GUILayout.TextField(asset.Tag, GUILayout.Width(150));

                    GUILayout.Space(5);
                    GUILayout.Label("AssetName: " + asset.AssetName);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    GUI.color = Color.white;
                }
                GUILayout.EndVertical();
            }

            if (DrawFoldout("Duplicated assets tags", "Duplicated assets tags: " + _duplicatedAssets.Sum(data => data.Value.Count)))
            {
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Screen.width - 40));
                foreach (var group in _duplicatedAssets)
                {
                    GUILayout.Label(group.Key, EditorStyles.miniBoldLabel);
                    foreach (var asset in group.Value)
                    {
                        GUI.color = asset.Tag == group.Key ? Color.white : Color.green;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Tag", GUILayout.Width(30));
                        asset.Tag = GUILayout.TextField(asset.Tag, GUILayout.Width(150));

                        GUILayout.Space(5);
                        GUILayout.Label("AssetName: " + asset.AssetName);
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5);
                        GUI.color = Color.white;
                    }
                }
                GUILayout.EndVertical();
            }

            var deletedAssets = _deletedAssets.Where(asset => asset.IsFromDevKit == _showDevKitAssets).ToList();
            if (DrawFoldout("Deleted assets", "Deleted assets: " + deletedAssets.Count))
            {
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Screen.width - 40));
                if (deletedAssets.Count > 0)
                {
                    if (GUILayout.Button("Remove all from catalog"))
                    {
                        foreach (var asset in deletedAssets)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
                            _catalog.Assets.Remove(asset);
                        }
                        deletedAssets.Clear();
                        BuildAssetsCatalog.Build();
                        GUIUtility.ExitGUI();
                    }
                    GUILayout.Space(5);
                }

                foreach (var asset in deletedAssets.GetRange(0, deletedAssets.Count))
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("AssetName: " + asset.AssetName);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tag: " + asset.Tag);
                    GUILayout.Label("DevKit: " + asset.IsFromDevKit);
                    GUILayout.EndHorizontal();
                    string path = asset.RelativePath.Replace(Application.dataPath, "Assets");
                    if (path.Length > 70)
                        path = "..." + path.Substring(path.Length - 70);
                    GUILayout.Label("Path: " + path, EditorStyles.centeredGreyMiniLabel);

                    if (GUILayout.Button("Remove from catalog"))
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
                        deletedAssets.Remove(asset);
                        _catalog.Assets.Remove(asset);
                        BuildAssetsCatalog.Build();
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
            }

            if (GUI.changed)
            {
                PlaytablePrefs.SavePrefs();
                EditorUtility.SetDirty(_catalog);
            }
        }

        private bool DrawFoldout(string key, string label)
        {
            bool state = EditorGUILayout.Foldout(GetFoldout(key), label);
            SetFoldout(key, state);
            return state;
        }
        private bool GetFoldout(string key, bool defaultValue = true)
        {
            return PlaytablePrefs.Get($"{DevKit.ProjectName}/{key}", defaultValue);
        }
        private void SetFoldout(string key, bool value)
        {
            PlaytablePrefs.Set($"{DevKit.ProjectName}/{key}", value);
        }

        private void RestoreIgnoredAsset(IgnoredPlayAsset ignoredAsset)
        {
            PlayAsset asset = BuildAssetsCatalog.CreatePlayAsset(ignoredAsset.Tag, ignoredAsset.AssetName,
                ignoredAsset.Type, ignoredAsset.RelativePath);
            asset.IsFromDevKit = ignoredAsset.IsFromDevKit;
            _catalog.Assets.Add(asset);
            AssetDatabase.CreateAsset(asset, $"Assets/AssetsCatalog/{asset.name}.asset");
        }
    }
}
