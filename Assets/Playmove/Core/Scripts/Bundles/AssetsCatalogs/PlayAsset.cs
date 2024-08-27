using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Playmove.Core.Bundles
{
    [Serializable]
    public class IgnoredPlayAsset
    {
        public string AssetName;
        public bool IsFromDevKit;
        public string Tag;
        public string Type;
        public string RelativePath;
    }

    [CreateAssetMenu(menuName = "Playmove/Play Asset")]
    public class PlayAsset : ScriptableObject
    {
        /// <summary>
        /// Full asset name
        /// </summary>
        public string AssetName;
        /// <summary>
        /// Indicates if this asset is from DevKit or not
        /// </summary>
        public bool IsFromDevKit;
        [SerializeField] private string _tag;
        /// <summary>
        /// This tag should follow variable naming rules
        /// </summary>
        public string Tag
        {
            get { return _tag; }
            set { _tag = ForceVariablePattern(value); }
        }
        /// <summary>
        /// Type of this asset
        /// </summary>
        public string Type;
        
        public override string ToString()
        {
            return AssetName;
        }

        private string ForceVariablePattern(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Trim().Replace(' ', '_');
            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            text = new string(chars).Normalize(NormalizationForm.FormC);
            text = Regex.Replace(text, @"[^0-9A-Za-z_]", string.Empty);
            return text;
        }

#if UNITY_EDITOR
        public bool Ignore;
        public string RelativePath;

        public bool Exists
        {
            get
            {
                string absolutePath = Application.dataPath + RelativePath;
                if (Type == "string")
                    return File.Exists(absolutePath) && Regex.IsMatch(File.ReadAllText(absolutePath), $"\\b{AssetName}\\b");
                return File.Exists(absolutePath);
            }
        }

        public IgnoredPlayAsset ToIgnoredAsset()
        {
            return new IgnoredPlayAsset()
            {
                AssetName = AssetName,
                IsFromDevKit = IsFromDevKit,
                Tag = Tag,
                Type = Type,
                RelativePath = RelativePath
            };
        }
#endif
    }
}
