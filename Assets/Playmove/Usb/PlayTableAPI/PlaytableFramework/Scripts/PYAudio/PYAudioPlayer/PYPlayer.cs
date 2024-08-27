using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Playmove
{
    [Serializable]
    public class PYPlayer
    {
        public string Name = "Default";

        public float Volume = 1;
        public float Pitch = 1;
        public float Delay = 0;
        public bool Loop = false;

        public PYAudioSource PYSource { get; set; }

        public PYPlayer(string name)
        {
            Name = name;
        }

        public virtual PYAudioSource StartAudio() { return PYSource; }

#if UNITY_EDITOR
        public bool IsShowingElement = true;
        private bool _isShowingProps = true;

        public virtual void InitializeInspector() { }

        public virtual void DrawInspector()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            _isShowingProps = EditorGUILayout.Foldout(_isShowingProps, "PYAudioSource Props");
            if (_isShowingProps)
            {
                Volume = EditorGUILayout.Slider("Volume", Volume, 0, 1);
                Pitch = EditorGUILayout.Slider("Pitch", Pitch, -3, 3);
                Delay = EditorGUILayout.FloatField("Delay", Delay);
                Loop = EditorGUILayout.Toggle("Loop", Loop);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
#endif
    }
}