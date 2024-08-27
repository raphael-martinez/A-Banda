using UnityEngine;
using System;
using System.Collections;

namespace Playmove
{
    [Serializable]
    public class PYBundleAssetTag
    {
        public string UnprocessedTag;

        private string _tag = "";
        public string Tag
        {
            get
            {
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(UnprocessedTag))
#else
            if (string.IsNullOrEmpty(_tag) &&
                !string.IsNullOrEmpty(UnprocessedTag))
#endif
                {
                    _tag = UnprocessedTag.Replace(" ", "").Split(':')[0];
                }
                return _tag;
            }
        }
    }
}