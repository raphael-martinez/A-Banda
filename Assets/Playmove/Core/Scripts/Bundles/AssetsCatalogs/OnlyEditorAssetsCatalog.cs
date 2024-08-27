using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Core.Bundles
{
#if UNITY_EDITOR
    public partial class AssetsCatalog : ScriptableObject
    {
        public List<IgnoredPlayAsset> IgnoredAssets = new List<IgnoredPlayAsset>();
        public List<string> IgnoredFolders = new List<string>();

        public bool IsFolderIgnored(string folderPath)
        {
            foreach (var ignored in IgnoredFolders.Where(folder => !string.IsNullOrEmpty(folder)))
            {
                if (folderPath.Contains(ignored))
                    return true;
            }
            return false;
        }

        public bool CanCreateAsset(string assetName, string type)
        {
            return Assets.Find(asset => asset.AssetName == assetName && asset.Type == type) == null &&
                IgnoredAssets.Find(asset => asset.AssetName == assetName && asset.Type == type) == null;
        }

        public PlayAsset GetAsset(string assetName, string type)
        {
            return Assets.Find(asset => asset.AssetName == assetName && asset.Type == type);
        }

        public Dictionary<string, List<PlayAsset>> GetDuplicatedAssets()
        {
            Dictionary<string, List<PlayAsset>> duplicated = new Dictionary<string, List<PlayAsset>>();
            foreach (var asset in Assets.Where(asset => asset != null))
            {
                string assetTag = asset.Type + asset.Tag;
                if (!duplicated.ContainsKey(assetTag))
                    duplicated.Add(assetTag, new List<PlayAsset>());
                duplicated[assetTag].Add(asset);
            }
            return duplicated.Where(group => group.Value.Count > 1).ToDictionary(group => group.Key, group => group.Value);
        }

        public List<PlayAsset> GetAssetsWithoutTag()
        {
            return Assets.Where(asset => asset != null &&  string.IsNullOrEmpty(asset.Tag)).ToList();
        }

        public List<PlayAsset> GetDeletedAssets()
        {
            return Assets.Where(asset => asset != null && !asset.Exists).ToList();
        }

        public bool HasWarning()
        {
            return GetDuplicatedAssets().Count > 0 && GetAssetsWithoutTag().Count > 0;
        }
    }
#endif
}
