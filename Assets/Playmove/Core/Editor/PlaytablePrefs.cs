using System.Collections.Generic;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using UnityEditor.SceneManagement;

namespace Playmove.Core.Editor
{
    [InitializeOnLoad]
    public class PlaytablePrefs
    {
        public static event Action<string, object> OnPrefChanged;

        public static string EDITOR_PREFS_KEY
        {
            get
            {
                return $"{DevKit.ProjectName}/PlaytablePrefs";
            }
        }
        public static Dictionary<string, object> Prefs = new Dictionary<string, object>();

        static PlaytablePrefs()
        {
            EditorApplication.quitting += EditorApplication_quitting;
            EditorApplication.hierarchyChanged += EditorApplication_hierarchyChanged;
            EditorApplication.projectChanged += EditorApplication_projectChanged;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            DeserializePrefs();
        }

        public static void Set(string key, object value)
        {
            if (!Prefs.ContainsKey(key))
                Prefs.Add(key, value);
            else
                Prefs[key] = value;

            OnPrefChanged?.Invoke(key, value);
        }
        public static T Get<T>(string key, T defaultValue = default)
        {
            if (!Prefs.ContainsKey(key)) return defaultValue;
            return (T)Prefs[key];
        }
        public static List<T> GetList<T>(string key, List<T> defaultValue = default)
        {
            if (!Prefs.ContainsKey(key)) return defaultValue;
            if (!(Prefs[key] is JArray)) return defaultValue;

            List<T> items = new List<T>(
                ((JArray)Prefs[key]).Select(item => item.Value<T>())
                );

            return items;
        }

        public static void SavePrefs()
        {
            SerializePrefs();
        }
        
        private static void EditorApplication_quitting()
        {
            OnPrefChanged = null;
            SerializePrefs();
        }
        private static void EditorApplication_hierarchyChanged()
        {
            SerializePrefs();
        }
        private static void EditorApplication_projectChanged()
        {
            SerializePrefs();
        }
        private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
        {
            if (GameSettings.PlayFromPlaytableBootstrap)
                EditorSceneManager.playModeStartScene = DevKit.PlaytableBootstrapScene;
            else
                EditorSceneManager.playModeStartScene = null;
            SerializePrefs();
        }

        private static void DeserializePrefs()
        {
            Prefs = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                EditorPrefs.GetString(EDITOR_PREFS_KEY, "{}"));
        }

        private static void SerializePrefs()
        {
            Prefs = Prefs.Where(pref =>
            {
                if (pref.Value is string value)
                {
                    if (value.Contains(":/") && value.Contains("."))
                        return File.Exists(value);
                    else
                        return true;
                }
                return true;
            }).ToDictionary(pref => pref.Key, pref => pref.Value);
            EditorPrefs.SetString(EDITOR_PREFS_KEY, JsonConvert.SerializeObject(Prefs));
        }
    }
}
