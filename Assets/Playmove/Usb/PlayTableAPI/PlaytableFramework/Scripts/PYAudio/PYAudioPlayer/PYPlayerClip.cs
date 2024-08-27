using UnityEngine;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

namespace Playmove
{
    [Serializable]
    public class PYPlayerClip : PYPlayer
    {
        public PYBundleAssetTag AssetTag = new PYBundleAssetTag();
        public PYComponentBundleData UpdateData;

        public AudioClip Clip;
        public PYGroupTag Group = PYGroupTag.Master;

        public PYPlayerClip() : base("Default") { }
        public PYPlayerClip(string name, AudioClip clip, PYGroupTag group)
            : base(name)
        {
            Clip = clip;
            Group = group;
        }

        public override PYAudioSource StartAudio()
        {
            if (Clip != null)
                PYSource = PYAudioManager.Instance.StartAudio(Clip, Group);
            return base.StartAudio();
        }

#if UNITY_EDITOR
        private bool _isShowingUpdateData = false;
        private bool _isShowingBundlesArray = false;

        public override void DrawInspector()
        {
            Name = EditorGUILayout.TextField("Name", Name);
            EditorGUILayout.Separator();

            _isShowingUpdateData = EditorGUILayout.Foldout(_isShowingUpdateData, "Update Data");
            if (_isShowingUpdateData)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                GUILayout.BeginVertical();

                UpdateData.UpdateFromBundle = EditorGUILayout.Toggle("Update From Bundle", UpdateData.UpdateFromBundle);

                GUILayout.BeginHorizontal();
                _isShowingBundlesArray = EditorGUILayout.Foldout(_isShowingBundlesArray, "BundlesToCheck");
                if (GUILayout.Button("+"))
                    AddPYBundleType(PYBundleType.Data);
                else if (GUILayout.Button("-"))
                    RemovePYBundleType();
                GUILayout.EndHorizontal();

                if (UpdateData != null)
                {
                    if (_isShowingBundlesArray)
                    {
                        for (int x = 0; x < UpdateData.BundlesToCheck.Length; x++)
                            UpdateData.BundlesToCheck[x] = (PYBundleType)EditorGUILayout.EnumPopup("Element " + x, UpdateData.BundlesToCheck[x]);
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            Clip = (AudioClip)EditorGUILayout.ObjectField("Clip", Clip, typeof(AudioClip), true);
            Group = (PYGroupTag)EditorGUILayout.EnumPopup("Group", Group);
            base.DrawInspector();
        }

        private void AddPYBundleType(PYBundleType type)
        {
            List<PYBundleType> temp = new List<PYBundleType>(UpdateData.BundlesToCheck);
            temp.Add(type);
            UpdateData.BundlesToCheck = temp.ToArray();
        }
        private void RemovePYBundleType()
        {
            List<PYBundleType> temp = new List<PYBundleType>(UpdateData.BundlesToCheck);
            temp.RemoveAt(temp.Count - 1);
            UpdateData.BundlesToCheck = temp.ToArray();
        }
#endif
    }
}