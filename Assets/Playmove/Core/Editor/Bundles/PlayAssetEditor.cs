using Playmove.Core.Bundles;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Playmove.Core.Editor.Bundles
{
    [CustomEditor(typeof(PlayAsset))]
    public class PlayAssetEditor : UnityEditor.Editor
    {
        private PlayAsset _target;

        private SerializedProperty _isFromDevKit;
        private SerializedProperty _tagProperty;
        private SerializedProperty _typeProperty;
        private SerializedProperty _assetNameProperty;
        private SerializedProperty _relativePathProperty;
        private SerializedProperty _ignoredProperty;

        private bool _wasIgnored = false;
        private GUIStyle _redBoldLabel = null;

        private void OnEnable()
        {
            _target = (PlayAsset)target;
            _isFromDevKit = serializedObject.FindProperty("IsFromDevKit");
            _tagProperty = serializedObject.FindProperty("_tag");
            _typeProperty = serializedObject.FindProperty("Type");
            _assetNameProperty = serializedObject.FindProperty("AssetName");
            _relativePathProperty = serializedObject.FindProperty("RelativePath");
            _ignoredProperty = serializedObject.FindProperty("Ignore");
            _wasIgnored = false;
        }

        public override void OnInspectorGUI()
        {
            // Ignore all asset that is from DevKit
            if (!_target.IsFromDevKit)
            {
                if (_redBoldLabel == null)
                {
                    _redBoldLabel = new GUIStyle(EditorStyles.boldLabel);
                    _redBoldLabel.normal.textColor = new Color(0.5f, 0.05f, 0.05f);
                }

                bool assetExists = _target.Exists;
                GUI.color = assetExists ? Color.white : Color.red;
                if (!assetExists)
                    GUILayout.Label("The asset was DELETED from project or it's name changed!", _redBoldLabel);
            }

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_tagProperty);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_typeProperty);
            EditorGUILayout.PropertyField(_assetNameProperty);
            EditorGUILayout.PropertyField(_relativePathProperty);

            GUI.enabled = DevKit.ProjectName.ToLower().StartsWith("devkit");
            EditorGUILayout.PropertyField(_isFromDevKit);
            GUI.enabled = true;

            if (GUILayout.Button("Ignore"))
            {
                _ignoredProperty.boolValue = _wasIgnored = true;
                serializedObject.ApplyModifiedProperties();

                AssetsCatalogEditor.IgnoreAssetsIfNeeded();
            }
            
            if (!_wasIgnored)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
