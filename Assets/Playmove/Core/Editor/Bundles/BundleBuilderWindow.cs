using Playmove.Core.Bundles;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Playmove.Core.Editor.Bundles
{
    public class BundleBuilderWindow : EditorWindow
    {
        private GUIStyle _btnStyle = null;
        private GUIStyle _boxGrid3 = null;
        private Vector2 _scroll = Vector2.zero;

        [MenuItem("Playmove/Bundle Builder", priority = 1)]
        public static void Init()
        {
            BundleBuilderWindow window = GetWindow<BundleBuilderWindow>();
            window.titleContent = new GUIContent("Playmove Bundle Builder");
            window.Initialize();
            window.Show();
        }

        private void OnFocus()
        {
            _btnStyle = null;
            _boxGrid3 = null;

            if (!PlaytablePrefs.Get(Application.dataPath, false))
            {
                foreach (var path in DetectChangesInBundles.GetBundlesPathDirty())
                    DetectChangesInBundles.SetBundleDirty(path, false);
                PlaytablePrefs.Set(Application.dataPath, true);
            }
        }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(-6, 0, Screen.width, Screen.height - 21), EditorStyles.inspectorDefaultMargins);
            _scroll = GUILayout.BeginScrollView(_scroll);
            // Global Bundles
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUI.skin.box);
            List<string> contentsPath = PlaytableBundlesPath.GetContentsPath();
            List<string> datasPath = PlaytableBundlesPath.GetDatasPath();
            List<string> localizationsPath = PlaytableBundlesPath.GetLocalizationsPath();
            bool hasGlobalBundles = contentsPath.Count > 0 || datasPath.Count > 0 || localizationsPath.Count > 0;
            if (DrawFoldout("Global Bundles", hasGlobalBundles ? "Global Bundles" : "Global Bundles: None bundle found"))
            {
                DrawBundleTypeBox("global", BundleType.Content, contentsPath);
                DrawBundleTypeBox("global", BundleType.Data, datasPath);
                DrawBundleTypeBox("global", BundleType.Localization, localizationsPath);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // Expansion Bundles
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUI.skin.box);
            List<string> expansionNames = PlaytableBundlesPath.GetExpansionNames();
            if (DrawFoldout("Expansion Bundles", expansionNames.Count > 0 ? "Expansion Bundles" : "Expansion Bundles: None bundle found"))
            {
                foreach (var expansionName in expansionNames)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginVertical();
                    if (DrawFoldout(expansionName, expansionName))
                    {
                        DrawBundleTypeBox("expansion", BundleType.Content, PlaytableBundlesPath.GetContentsPath(expansionName));
                        DrawBundleTypeBox("expansion", BundleType.Data, PlaytableBundlesPath.GetDatasPath(expansionName));
                        DrawBundleTypeBox("expansion", BundleType.Localization, PlaytableBundlesPath.GetLocalizationsPath(expansionName));
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void Initialize()
        {
            _btnStyle = null;
            _boxGrid3 = null;
        }

        private Color GetBundleStateColor(string bundleKey)
        {
            if (DetectChangesInBundles.GetBundleDirty(bundleKey))
                return Color.yellow;
            else
                return Color.green;
        }

        private bool DrawFoldout(string key, string label)
        {
            bool state = EditorGUILayout.Foldout(GetFoldout(key), label);
            SetFoldout(key, state);
            return state;
        }
        private void DrawBundleTypeBox(string globalOrExpansion, BundleType type, List<string> bundlesPath)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            string label = bundlesPath.Count > 0 ? ($"{type.ToString()}: {bundlesPath.Count}") : $"{type.ToString()}: None bundle found";
            if (DrawFoldout(globalOrExpansion + type, label))
                DrawBundlesBuildButtons(bundlesPath);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        private void DrawBundlesBuildButtons(List<string> bundlesPath)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            if (_btnStyle == null)
            {
                _btnStyle = new GUIStyle(GUI.skin.button)
                {
                    fixedHeight = 32,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
            }

            if (_boxGrid3 == null)
            {
                _boxGrid3 = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }

            for (int i = 0; i < bundlesPath.Count; i++)
            {
                GUILayout.BeginHorizontal(_boxGrid3);
                for (int y = 0; y < 3 && i < bundlesPath.Count; y++)
                {
                    DrawBuildButton(bundlesPath[i]);
                    GUILayout.Space(5);
                    i++;
                }
                i--;
                GUILayout.EndHorizontal();
                GUILayout.Space(3);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawBuildButton(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            Color pastGUIColor = GUI.color;
            GUI.color = GetBundleStateColor(path);

            if (GUILayout.Button(new GUIContent(directory.Name, directory.Name), _btnStyle, GUILayout.Width(Screen.width * 0.3f)))
            {
                PlaytableBuildPipeline.BuildBundle(path);
                GUIUtility.ExitGUI();
            }

            GUI.color = pastGUIColor;
        }

        private bool GetFoldout(string key)
        {
            return PlaytablePrefs.Get($"{DevKit.ProjectName}/BundleBuilder/{key}", true);
        }
        private void SetFoldout(string key, bool foldout)
        {
            PlaytablePrefs.Set($"{DevKit.ProjectName}/BundleBuilder/{key}", foldout);
            if (GUI.changed)
                PlaytablePrefs.SavePrefs();
        }
    }
}
