using Playmove.Core;
using Playmove.Core.Bundles;
using UnityEngine;

namespace Playmove.Framework.Localizers
{
    public abstract class Localizer<T> : MonoBehaviour
        where T : Object
    {
        [SerializeField] string _assetName = string.Empty;
        [SerializeField] PlayAsset _asset = null;

        protected T _component;
        protected T Component
        {
            get
            {
                if (_component == null)
                    _component = GetComponent<T>();
                return _component;
            }
        }

        public string AssetName
        {
            get
            {
                if (!string.IsNullOrEmpty(_assetName)) return _assetName;
                if (_asset != null) return _asset.AssetName;
                return string.Empty;
            }
        }

        protected virtual void OnEnable()
        {
            Localize();
        }

        protected abstract void Localize();
    }
}
