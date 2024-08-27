using UnityEngine;
using System.Collections;
using Playmove.Core.Bundles;

namespace Playmove
{
    public class PYPrefab : PYComponentBundle
    {
        public PYBundleAssetTag AssetTag;

        [Header("PYPrefab")]
        public GameObject CurrentPrefab;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void UpdateComponent()
        {
            if (UpdateData.UpdateFromBundle)
            {
                UpdateData.DefaultComponentValue = CurrentPrefab;

                GameObject asset = Localization.GetAsset<GameObject>(AssetTag.Tag);
                if (asset != null)
                {
                    if (CurrentPrefab != null)
                    {
#if UNITY_EDITOR
                        CurrentPrefab.SetActive(false);
#else
                    Destroy(CurrentPrefab);
#endif
                    }

                    CurrentPrefab = (GameObject)Instantiate(asset);
                    CurrentPrefab.transform.SetParent(transform);
                    CurrentPrefab.transform.localPosition = asset.transform.localPosition;
                }
            }
        }

        public override void RestoreComponent()
        {
            if (UpdateData.UpdateFromBundle)
            {
#if UNITY_EDITOR
                if (CurrentPrefab != null)
                {
                    DestroyImmediate(CurrentPrefab);

                    CurrentPrefab = (GameObject)UpdateData.DefaultComponentValue;
                    CurrentPrefab.SetActive(true);
                }
#endif
            }
        }
    }
}