using Playmove.Core.Bundles;
using Playmove.Core.Editor.Bundles;
using Playmove.Core.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Playmove.Core.Editor
{
    public class PlaytableBuildPipeline : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 1; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            DevKit.BuildVersion.Value++;
            EditorUtility.SetDirty(GameSettings.Instance);
            EditorUtility.SetDirty(DevKit.BuildVersion);
            AssetDatabase.SaveAssets();
        }

        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string exeFinalPath)
        {
            if (!File.Exists(exeFinalPath)) return;

            FileInfo dir = new FileInfo(exeFinalPath);
            const string BUILD_FILE_NAME = "build_version_";

            // Delete old build_version file
            try
            {
                FileInfo[] filesInfo = dir.Directory.GetFiles("*.txt", SearchOption.TopDirectoryOnly)
                    .Where(file => file.FullName.Contains(BUILD_FILE_NAME)).ToArray();
                foreach (var file in filesInfo)
                    File.Delete(file.FullName);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }

            // Create new build_version file
            try
            {
                int buildVersion = DevKit.BuildVersion.Value;
                File.WriteAllText(string.Format("{0}/{1}{2}.txt", dir.DirectoryName, BUILD_FILE_NAME, buildVersion),
                    "build_version: " + buildVersion);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
        }

        public static void BuildPlayer(BuildType type)
        {
            string[] catalogPaths = AssetDatabase.FindAssets("t:AssetsCatalog");
            if (catalogPaths.Length > 0)
            {
                AssetsCatalog catalog = AssetDatabase.LoadAssetAtPath<AssetsCatalog>(AssetDatabase.GUIDToAssetPath(catalogPaths[0]));
                if (AssetsCatalogEditor.HasWarningInCatalog(catalog))
                {
                    if (EditorUtility.DisplayDialog("Assets Catalog Warning", "Your catalog has some assets that need's your attention.", "Fix"))
                        Selection.activeObject = catalog;
                    return;
                }
            }

            // Ask where to save build
            string executableName = GameSettings.ExecutableName;
            string lastBuildPath = PlaytablePrefs.Get<string>($"{DevKit.ProjectName}PlaymoveLastBuildPath");
            lastBuildPath = EditorUtility.SaveFolderPanel("Choose build folder", lastBuildPath, string.Empty);
            if (string.IsNullOrEmpty(lastBuildPath)) return;
            PlaytablePrefs.Set($"{DevKit.ProjectName}PlaymoveLastBuildPath", lastBuildPath);

            lastBuildPath += $"/{executableName}.exe";

            // Save user values
            var displayResolutionDialog = PlayerSettings.displayResolutionDialog;
            var fullscreenMode = PlayerSettings.fullScreenMode;
            var allowFullscreenSwitch = PlayerSettings.allowFullscreenSwitch;

            // Set default playtable configs
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.allowFullscreenSwitch = false;
            PlayerSettings.SplashScreen.show = false;

            // Configure build
            List<string> scenes = new List<string>(EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path));
            scenes.Insert(0, AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PlaytableBootstrap t:Scene")[0]));
            scenes = scenes.Distinct().Where(scenePath => !string.IsNullOrEmpty(scenePath)).ToList();

            BuildPlayerOptions buildOptions = new BuildPlayerOptions()
            {
                target = EditorUserBuildSettings.activeBuildTarget,
                targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup,
                locationPathName = lastBuildPath,
                scenes = scenes.ToArray(),
                options = BuildOptions.None
            };
            switch (type)
            {
                case BuildType.Test:
                    buildOptions.options = BuildOptions.Development;
                    break;
            }

            var dirtyBundles = DetectChangesInBundles.GetBundlesPathDirty();
            if (dirtyBundles.Count > 0 &&
                EditorUtility.DisplayDialog("Dirty bundles", $"You have {dirtyBundles.Count} dirty bundles thats need to be build.", "Build Now", "Build Later"))
            {
                // Before building the game we build all dirty bundles
                foreach (var bundlePath in dirtyBundles)
                    BuildBundle(bundlePath);
            }

            PlaytablePrefs.SavePrefs();

            // Build game
            BuildPipeline.BuildPlayer(buildOptions);

            // Restore user values
            PlayerSettings.displayResolutionDialog = displayResolutionDialog;
            PlayerSettings.fullScreenMode = fullscreenMode;
            PlayerSettings.allowFullscreenSwitch = allowFullscreenSwitch;
            PlayerSettings.runInBackground = false;

            FileInfo exeInfo = new FileInfo(lastBuildPath);
            if (File.Exists(exeInfo.FullName))
            {
                Debug.Log($"Game builded successfully at {lastBuildPath}");
                Storage.CopyDir(PlaytableBundlesPath.BundlesLoadPathEditor, exeInfo.Directory.FullName, (result) =>
                {
                    Debug.Log("Bundles successfully copied to build path!");
                    EditorUtility.RevealInFinder(lastBuildPath);
                }, null);
            }
        }

        public static void BuildBundle(string bundleAssetPath)
        {
            UnityDirInfo buildDir = PlaytableBundlesPath.GetBuildDirectory(bundleAssetPath);
            DirectoryInfo dirInfo = new DirectoryInfo(bundleAssetPath);
            BuildBundle(bundleAssetPath, buildDir.FullName, dirInfo.Name);
        }
        public static void BuildBundle(string bundleAssetsPath, string bundleBuildPath, string bundleName)
        {
            bundleAssetsPath = bundleAssetsPath.Replace(@"\", "/");
            bundleBuildPath = bundleBuildPath.Replace(@"\", "/");
            
            DirectoryInfo directory = new DirectoryInfo(bundleBuildPath);
            string[] assetsPath = GetAssetsPathFromFolder(bundleAssetsPath);
            // Check if we have scenes files inside this bundle
            string[] scenesPath = null;
            if (!string.IsNullOrEmpty(assetsPath.FirstOrDefault(path => path.EndsWith(".unity"))))
            {
                scenesPath = assetsPath.Where(path => path.EndsWith(".unity")).ToArray();
                assetsPath = assetsPath.Where(path => !path.EndsWith(".unity")).ToArray();
            }

            string expansionOrGlobal = bundleBuildPath.Contains("/Expansions/") ? "Expansions" : "Global";
            if (string.IsNullOrEmpty(bundleName))
                bundleName = directory.Name;
            List<AssetBundleBuild> bundlesToBuild = new List<AssetBundleBuild>
            {
                new AssetBundleBuild()
                {
                    assetBundleName = $"{expansionOrGlobal}_{bundleName}.bundle",
                    assetNames = assetsPath
                }
            };
            // If we have scenes we build a different bundle just with scenes
            if (scenesPath != null)
            {
                bundlesToBuild.Add(new AssetBundleBuild()
                {
                    assetBundleName = $"{expansionOrGlobal}_{bundleName}_scenes.bundle",
                    assetNames = scenesPath
                });
            }

            if (!Directory.Exists(bundleBuildPath))
                Directory.CreateDirectory(bundleBuildPath);
            
            BuildPipeline.BuildAssetBundles(bundleBuildPath, bundlesToBuild.ToArray(),
                BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
            AssetDatabase.Refresh();

            DetectChangesInBundles.SetBundleDirty(bundleAssetsPath, false);
            Debug.Log($"Bundle builded successfully at {bundleBuildPath}");
        }

        private static string[] GetAssetsPathFromFolder(string path)
        {
            string[] assetsPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            assetsPath = assetsPath.Where(p => !p.EndsWith(".meta")).ToArray();

            for (int i = 0; i < assetsPath.Length; i++)
            {
                assetsPath[i] = assetsPath[i].Replace("\\", "/");
                List<string> splitted = new List<string>(assetsPath[i].Split('/'));
                int startIndex = splitted.IndexOf("Assets");
                assetsPath[i] = string.Join("/", splitted.GetRange(startIndex, splitted.Count - startIndex));
            }
            
            return assetsPath;
        }
    }
}
