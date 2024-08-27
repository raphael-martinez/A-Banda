using Playmove.Core.Editor.Bundles;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Playmove.Core.Editor
{
    public enum BuildType
    {
        Test,
        Release
    }

    [CustomEditor(typeof(GameSettings))]
    public class GameSettingsEditor : UnityEditor.Editor
    {
        [MenuItem("Playmove/Select Game Settings", priority = 1)]
        static void SelectGameSettings()
        {
            Selection.activeObject = GameSettings.Instance;
        }

        [MenuItem("Playmove/Build Game Test")]
        static void BuildTest()
        {
            PlaytableBuildPipeline.BuildPlayer(BuildType.Test);
        }
        [MenuItem("Playmove/Build Game Release")]
        static void BuildRelease()
        {
            PlaytableBuildPipeline.BuildPlayer(BuildType.Release);
        }

        GameSettings _target = null;
        Vector2 _scrollPosition = Vector2.zero;

        private void ChangeInitialScene()
        {
            GameSettings.PlayFromPlaytableBootstrap = serializedObject.FindProperty("_playFromPlaytableBootstrap").boolValue;
        }

        private void OnEnable()
        {
            _target = (GameSettings)target;
            EditorApplication.playModeStateChanged += (state) => {ChangeInitialScene();};
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();

            // GUILayout.BeginArea(new Rect(0, 45, Screen.width, Screen.height - 80), EditorStyles.inspectorDefaultMargins);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Build Version: " + DevKit.BuildVersion, EditorStyles.boldLabel);
            if (GUILayout.Button("Reset Build Version"))
            {
                DevKit.BuildVersion.Value = 0;
                EditorUtility.SetDirty(DevKit.BuildVersion);
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("DevKit Version: " + DevKit.Version, EditorStyles.boldLabel);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_playFromPlaytableBootstrap"));
            
            //SopVirtual
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("SopVirtual Port", EditorStyles.boldLabel);
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_port"));
            GUILayout.EndVertical();
            
            // Playmove Settings
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Playmove Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_GUID"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_executableName"));
            PlayerSettings.productName = GameSettings.ExecutableName;
            GUILayout.EndVertical();

            // Playtable Settings
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Playtable Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_expansion"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_language"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_mute"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_volume"));
            GUILayout.EndVertical();

            // Avatar Settings
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Avatar Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_totalSlots"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_minSlots"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxPlayersPerSlot"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_hasAI"));
            GUILayout.EndVertical();

            // Unity Settings
            GUILayout.Space(5);
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Unity Settings", EditorStyles.boldLabel);

            PlayerSettings.companyName = EditorGUILayout.TextField("Company Name", PlayerSettings.companyName);
            GUILayout.Space(5);
            if (GUILayout.Button("Open Build Settings"))
                EditorApplication.ExecuteMenuItem("File/Build Settings...");
            if (GUILayout.Button("Open Project Settings"))
                EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            GUILayout.EndVertical();

            // Playtable Default Settings
            GUILayout.Space(5);
            GUI.enabled = false;
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Playtable Default Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("This settings is used when building the game for Playtable, " +
                "when build finishes this settings is restored", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(5);

            EditorGUILayout.EnumPopup("Display Resolution Dialog", ResolutionDialogSetting.Disabled);
            EditorGUILayout.EnumPopup("Fullscreen Mode", FullScreenMode.FullScreenWindow);
            EditorGUILayout.Toggle("Allow Fullscreen Switch", false);
            GUILayout.EndVertical();
            GUI.enabled = true;

            // Build buttons
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build Test"))
            {
                BuildTest();
                GUIUtility.ExitGUI();
            }
            GUILayout.Space(15);
            if (GUILayout.Button("Build Release"))
            {
                BuildRelease();
                GUIUtility.ExitGUI();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (GUILayout.Button("Open Bundle Builder"))
                BundleBuilderWindow.Init();
            GUILayout.Space(2);
            if (GUILayout.Button("Build Assets Catalog"))
                BuildAssetsCatalog.Build();
            GUILayout.Space(2);
            if (GUILayout.Button("Update localization files"))
            {
                _updateLocalizationLogs.Clear();
                DownloadLocalization.UpdateLocalization(null, (log) => _updateLocalizationLogs.Add(log));
            }

            DrawLogsForUpdateLocalization();

            GUILayout.EndScrollView();
            // GUILayout.EndArea();
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
                ChangeInitialScene();
        }

        private List<string> _updateLocalizationLogs = new List<string>();
        private Vector2 _updateLocalizationScroll = Vector2.zero;
        private void DrawLogsForUpdateLocalization()
        {
            if (_updateLocalizationLogs.Count == 0) return;
            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width * 0.25f / 2);
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(Screen.width * 0.75f), GUILayout.MinHeight(120), GUILayout.MaxHeight(250));

            _updateLocalizationScroll = GUILayout.BeginScrollView(_updateLocalizationScroll);
            int counter = 0;
            foreach (var log in _updateLocalizationLogs)
            {
                if (log.StartsWith("Success:"))
                    GUI.color = Color.green;
                else if (log.StartsWith("Warning:"))
                    GUI.color = Color.yellow;
                else if (log.StartsWith("Error:"))
                    GUI.color = Color.red;
                else
                    GUI.color = Color.white;
                GUILayout.Label(string.Format("{0}: {1}", ++counter, log), EditorStyles.wordWrappedMiniLabel);
            }

            GUI.color = Color.white;
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width * 0.25f / 2);
            if (GUILayout.Button("Clear log", GUILayout.Width(Screen.width * 0.75f)))
                _updateLocalizationLogs.Clear();
            GUILayout.EndHorizontal();
        }
    }
}
