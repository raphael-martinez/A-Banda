using UnityEngine;
using System;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Playmove
{
    [Serializable]
    public class PYPlayerTag : PYPlayer
    {
        [FormerlySerializedAs("_tag")]
        [SerializeField]
        private PYAudioTags _tag;
        public PYAudioTags Tag
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(_tagName) &&
                        _tag.ToString() != _tagName)
                        _tag = (PYAudioTags)Enum.Parse(typeof(PYAudioTags), _tagName);
                }
                catch
                {
                    _tag = (PYAudioTags)Enum.Parse(typeof(PYAudioTags), "None");
                    _tagName = "None";
                }
                return _tag;
            }
            set
            {
                _tag = value;
                _tagName = _tag.ToString();
            }
        }

        [SerializeField]
        private string _tagName;

        public PYPlayerTag() : base("Default") { }
        public PYPlayerTag(string name, PYAudioTags tag)
            : base(name)
        {
            Tag = tag;
        }

        public override PYAudioSource StartAudio()
        {
            PYSource = PYAudioManager.Instance.StartAudio(Tag);
            return base.StartAudio();
        }

#if UNITY_EDITOR
        public override void DrawInspector()
        {
            Name = EditorGUILayout.TextField("Name", Name);
            EditorGUILayout.Separator();

            base.DrawInspector();

            if (Tag.ToString() != _tagName)
                _tagName = Tag.ToString();
        }
#endif
    }
}