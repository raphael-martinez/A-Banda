using Playmove.Core.Bundles;
using Playmove.Core.Datas;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Playmove.Core.Editor
{
    [InitializeOnLoad]
    public class DevKit
    {
        static DevKit()
        {
            string[] pathsOfGameSettings = Directory.GetFiles(Application.dataPath, "*GameSettings.asset", SearchOption.AllDirectories);
            if (pathsOfGameSettings.FirstOrDefault(path => path.EndsWith("GameSettings.asset")) == "")
                GameSettings.CreateOrFindGameSettings();
            else
            {
                // Delete any duplicated GameSettings
                for (int i = 1; i < pathsOfGameSettings.Length; i++)
                    File.Delete(pathsOfGameSettings[0]);
            }
        }

        public static string ProjectName
        {
            get { return Directory.GetParent(Application.dataPath).Name; }
        }

        private static SceneAsset _playtableBootstrapScene;
        public static SceneAsset PlaytableBootstrapScene
        {
            get
            {
                if (_playtableBootstrapScene == null)
                {
                    string path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PlaytableBootstrap t:Scene")[0]);
                    _playtableBootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                }
                return _playtableBootstrapScene;
            }
        }

        private static AssetsCatalog _assetsCatalog;
        public static AssetsCatalog AssetsCatalog
        {
            get
            {
                if (_assetsCatalog == null)
                    _assetsCatalog = GetScriptableAsset<AssetsCatalog>("AssetsCatalog", "AssetsCatalog");
                return _assetsCatalog;
            }
        }

        private static IntValue _buildVersion;
        public static IntValue BuildVersion
        {
            get
            {
                if (_buildVersion == null)
                    _buildVersion = GetScriptableAsset<IntValue>("BuildVersion", "IntValue");
                return _buildVersion;
            }
            set { _buildVersion = value; }
        }

        private static IntValue _version;
        public static IntValue Version
        {
            get
            {
                if (_version == null)
                    _version = GetScriptableAsset<IntValue>("DevKitVersion", "IntValue");
                return _version;
            }
        }

        public static T GetScriptableAsset<T>(string assetName, string assetType)
            where T : ScriptableObject
        {
            T asset = default;
            string[] gameSettingsGUIDs = AssetDatabase.FindAssets($"{assetName} t:{assetType}");
            if (gameSettingsGUIDs.Length > 0)
                asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(gameSettingsGUIDs[0]));
            return asset;
        }

    }
}
