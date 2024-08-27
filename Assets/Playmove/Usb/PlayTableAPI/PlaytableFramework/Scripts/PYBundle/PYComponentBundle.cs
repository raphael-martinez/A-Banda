using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Object = UnityEngine.Object;

namespace Playmove
{
    /// <summary>
    /// Interface for all scripts that want to be a ComponentBundle
    /// </summary>
    public interface IPYComponentBundle
    {
        void InitializeComponent();
        void UpdateComponent();
        void RestoreComponent();
    }

    [Serializable]
    public class PYComponentBundleData
    {
        public bool UpdateFromBundle = true;
        public PYBundleType[] BundlesToCheck = new PYBundleType[2] { PYBundleType.Data, PYBundleType.Localization };

        // Serialized values for the _defaultComponentValue
        [SerializeField]
        [HideInInspector]
        private Object _defaultUnityValue;
        [SerializeField]
        [HideInInspector]
        private string _defaultStringValue;

        protected object _defaultComponentValue;
        public object DefaultComponentValue
        {
            get
            {
                // Restore the serialized values
                if (_defaultUnityValue != null)
                    _defaultComponentValue = _defaultUnityValue;
                else if (_defaultStringValue != null && !string.IsNullOrEmpty(_defaultStringValue))
                    _defaultComponentValue = _defaultStringValue;
                else
                    _defaultComponentValue = null;

                return _defaultComponentValue;
            }
            set
            {
                _defaultComponentValue = value;

                // Save _defaultComponentValue in the serialized version
                _defaultUnityValue = null;
                _defaultStringValue = null;
                if (_defaultComponentValue is Object)
                    _defaultUnityValue = (Object)_defaultComponentValue;
                else
                    _defaultStringValue = (string)_defaultComponentValue;
            }
        }
    }

    /// <summary>
    /// Basic class for a script that want to be a BundleComponent.
    /// </summary>
    public abstract class PYComponentBundle : MonoBehaviour, IPYComponentBundle
    {
        [Header("PYComponentBundle")]
        public PYComponentBundleData UpdateData = new PYComponentBundleData();

        protected virtual void Awake()
        {
            if (UpdateData.UpdateFromBundle)
                InitializeComponent();
        }

        protected virtual void OnDestroy()
        {
            if (PYBundleManager.Instance != null)
                PYBundleManager.Instance.onLoadCompleted.RemoveListener(BundleLoadCompletedCallback);
        }

        /// <summary>
        /// Get a asset from the list of bundles type, using its assetTag
        /// and its type
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="bundlesToCheck">List of bundlesType to check(The last on list more priority it has)</param>
        /// <param name="assetTag">Asset tag</param>
        /// <returns>Returns the asset from the Type or null if not found</returns>
        public static T GetAsset<T>(PYBundleType[] bundlesToCheck, string assetTag)
        {
            if (string.IsNullOrEmpty(assetTag) || assetTag == "None")
                return default(T);

            T asset = default(T);

            // We check starting from the last because the last one will always have
            // priority over the first, so if we find the asset in the last we dont
            // need to just the others
            for (int x = bundlesToCheck.Length - 1; x >= 0; x--)
            {
                PYBundleSubManager manager = null;
                switch (bundlesToCheck[x])
                {
                    case PYBundleType.Content: manager = PYBundleManager.Content; break;
                    case PYBundleType.Data: manager = PYBundleManager.Data; break;
                    case PYBundleType.Localization: manager = PYBundleManager.Localization; break;
                }

                if (manager == null)
                    continue;

                asset = manager.GetAsset<T>(assetTag);
                if (asset != null)
                    return asset;
            }

#if UNITY_EDITOR
            if (asset == null && !string.IsNullOrEmpty(assetTag) && assetTag != "None")
                Debug.LogWarning(string.Format("BundleAsset({0}: {1}) was not found!", assetTag, typeof(T)));
#endif

            // In case any asset hasn't been found in the managers
            // we return null
            return asset;
        }

        public T GetAsset<T>(string assetTag)
        {
            return PYComponentBundle.GetAsset<T>(UpdateData.BundlesToCheck, assetTag);
        }

        /// <summary>
        /// Update component based in the PYBundleManager.
        /// It will call the UpdateComponent()
        /// </summary>
        public virtual void InitializeComponent()
        {
            UpdateComponent();
        }

        /// <summary>
        /// Update the component using its bundle asset
        /// </summary>
        public abstract void UpdateComponent();

        /// <summary>
        /// Restore the component to its default value
        /// </summary>
        public abstract void RestoreComponent();

        protected virtual void BundleLoadCompletedCallback(PYBundleManager.PYBundleManagerEventData data)
        {
            UpdateComponent();
        }
    }
}