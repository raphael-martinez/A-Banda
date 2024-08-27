using Playmove.Core.API;
using Playmove.Core.Bundles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Framework
{
    [Serializable]
    public class BannedWordsData
    {
        public double Version;
        public List<string> Words;

        public BannedWordsData()
        {
            Version = 0;
            Words = new List<string>();
        }

        public bool IsValid(string word)
        {
            return !Words.Contains(word.ToLower());
        }
    }

    public static class BannedWords
    {
        private static BannedWordsData _words;
        public static BannedWordsData Words
        {
            get
            {
                if (_words == null)
                {
                    _words = LoadFromBundles();
                    PlaytableAPI.GetBannedWords((result) =>
                    {
                        if (result.HasError) return;
                        // Needed for back compatibility 
                        if (result.Data == null)
                        {
                            PlaytableAPI.SetBannedWords(_words);
                            return;
                        }

                        if (result.Data.Version > _words.Version)
                            _words = result.Data;
                        else
                            PlaytableAPI.SetBannedWords(_words);
                    });
                }
                return _words;
            }
            private set { _words = value; }
        }

        public static bool IsValid(string word)
        {
            return Words.IsValid(word);
        }

        private static BannedWordsData LoadFromBundles()
        {
            BannedWordsData words = new BannedWordsData();
            List<string> lines = new List<string>(Localization.GetAsset<TextAsset>(AssetsCatalog.TextAsset_bannedWords).text.Split('\n'));
            double.TryParse(lines[0], out words.Version);
            lines.RemoveAt(0);
            words.Words = lines.Where(line => !string.IsNullOrEmpty(line))
                .Select(word => word.Replace("\r", string.Empty)).ToList();
            return words;
        }
    }
}
