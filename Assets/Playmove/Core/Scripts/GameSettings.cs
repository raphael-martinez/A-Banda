using Playmove.Core.BasicEvents;
using System.Text.RegularExpressions;
using UnityEngine;
using Playmove.Avatars.API.Models;
using System;
#if UNITY_EDITOR
using UnityEditor;
#else
using Playmove.Core.Bundles;
#endif

namespace Playmove.Core
{
    /// <summary>
    /// Unity scriptable object to hold informations that playtable needs
    /// for this game to work
    /// </summary>
    [ScriptOrder(-200)]
    public class GameSettings : ScriptableObject
    {
        private static GameSettings _instance = null;
        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    _instance = CreateOrFindGameSettings();
#else
                    _instance = Data.GetAsset<GameSettings>("GameSettings");
#endif
                }
                return _instance;
            }
        }

        public static int InactiveTime = 0;
        public static PlaytableEventString OnLanguageChanged = new PlaytableEventString();
        public static PlaytableEventBool OnMuteChanged = new PlaytableEventBool();
        public static PlaytableEventFloat OnVolumeChanged = new PlaytableEventFloat();
        public static PlaytableEventFloat OnMicVolumeChanged = new PlaytableEventFloat();

        public static long ApplicationId { get; set; }

        private void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }
        // Playmove Settings
        [Tooltip("Port used by SopVirtual")]
        [SerializeField] string _port = "8081";
        public static string port
        {
            get { return Instance._port; }
            set { Instance._port = value; }
        }
        

        // Playmove Settings
        [Tooltip("GUID that Playmove sent")]
        [SerializeField] string _GUID = "-";
        public static string GUID
        {
            get { return Instance._GUID; }
            set { Instance._GUID = value; }
        }
        [Tooltip("Executable Name without .exe, accents and spaces")]
        [SerializeField] string _executableName = string.Empty;
        public static string ExecutableName
        {
            get { return Instance._executableName; }
            set { Instance._executableName = value; }
        }

        // Playtable Settings
        [SerializeField] string _expansion = string.Empty;
        public static string Expansion { get { return Instance._expansion; } }
        [SerializeField] string _language = "pt-BR";
        public static string Language
        {
            get { return Instance._language; }
            set
            {
                if (!Regex.IsMatch(value, "[a-z]+-[A-Z]+") && value.Length != 5)
                {
                    Debug.LogWarning("Language is in bad format it should be as follow pt-BR not pt-br!");
                    return;
                }
                Instance._language = value;
                OnLanguageChanged.Invoke(Instance._language);
            }
        }

        [SerializeField] bool _playFromPlaytableBootstrap = false;
        public static bool PlayFromPlaytableBootstrap
        {
            get { return Instance._playFromPlaytableBootstrap; }
            set { 
                Instance._playFromPlaytableBootstrap = value; 
            }
        }

        [SerializeField] bool _mute = false;
        public static bool Mute
        {
            get { return Instance._mute; }
            set
            {
                Instance._mute = value;
                OnMuteChanged.Invoke(Instance._mute);
            }
        }
        [SerializeField, Range(0, 1)] float _volume = 0.5f;
        public static float Volume
        {
            get { return Instance._volume; }
            set
            {
                Instance._volume = Mathf.Clamp01(value);
                OnVolumeChanged.Invoke(Instance._volume);
            }
        }

        [SerializeField, Range(0, 1)] float _micVolume = 0.5f;
        public static float MicVolume
        {
            get { return Instance._micVolume; }
            set
            {
                Instance._micVolume = Mathf.Clamp01(value);
                OnMicVolumeChanged.Invoke(Instance._micVolume);
            }
        }

        public static string FormatData(DateTime date)
        {
            return (Language == "en-US") ? date.ToString("MM/dd/y hh:mm tt") : date.ToString("dd/MM/y HH:mm");
        }

        // Avatar Settings
        [SerializeField, Range(1, 4)] int _minSlots = 1;
        public static int MinSlots { get { return Mathf.Clamp(Instance._minSlots, 1, TotalSlots); } }
        [SerializeField, Range(1, 4)] int _totalSlots = 1;
        public static int TotalSlots { get { return Instance._totalSlots; } }
        [SerializeField, Range(1, 5)] int _maxPlayersPerSlot = 5;
        public static int MaxPlayersPerSlot { get { return Instance._maxPlayersPerSlot; } }
        [SerializeField] bool _hasAI = false;
        public static bool HasAI { get { return Instance._hasAI; } }

        private static SlotsConfig _slotConfigParams = null;
        public static SlotsConfig SlotsConfig
        {
            get
            {
                if (_slotConfigParams == null)
                {
                    _slotConfigParams = new SlotsConfig(TotalSlots, MaxPlayersPerSlot, MinSlots, HasAI);
                }
                return _slotConfigParams;
            }
            set
            {
                _slotConfigParams = value;
            }
        }

        /// <summary>
        /// Used only for Playtable.cs to set the initial language for the game and it dont need to raise the change event
        /// </summary>
        public static void SetLanguageWithoutRaisingEvent(string language)
        {
            Instance._language = language;
        }

#if UNITY_EDITOR
        public static GameSettings CreateOrFindGameSettings()
        {
            GameSettings settings = GetScriptableAsset<GameSettings>("GameSettings", "GameSettings");

            // Create a gameSettings file if any exist in project
            if (settings == null)
            {
                settings = CreateInstance<GameSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Playmove/Core/Resources/GameSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = settings;
                Debug.Log("File GameSettings created at Assets/Playmove/Core/Resources/GameSettings.asset!");
            }
            return settings;
        }

        private static T GetScriptableAsset<T>(string assetName, string assetType)
            where T : ScriptableObject
        {
            T asset = default;
            string[] gameSettingsGUIDs = AssetDatabase.FindAssets($"{assetName} t:{assetType}");
            if (gameSettingsGUIDs.Length > 0)
                asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(gameSettingsGUIDs[0]));
            return asset;
        }
#endif
    }
}
