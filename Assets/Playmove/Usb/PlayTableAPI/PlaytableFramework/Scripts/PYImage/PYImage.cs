using Playmove.Core.Bundles;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Playmove
{
    public abstract class PYImage : PYComponentBundle
    {
        public PYBundleAssetTag AssetTag;

        [Header("PYImage")]
        public bool UseSharedMaterial = false;

        public override void UpdateComponent()
        {
            UpdateData.DefaultComponentValue = Image;
            if (UpdateData.UpdateFromBundle)
            {
                Sprite sprite = Localization.GetAsset<Sprite>(AssetTag.Tag);
                if (sprite != null)
                {
                    if ((Image == null) && (this is PYImageSprite))
                    {
                        PYImageSprite pySprite = (PYImageSprite)this;
                        pySprite.Sprite = sprite;
                    }
                    else
                        Image = sprite.texture;
                }
            }
        }

        public override void RestoreComponent()
        {
            if (UpdateData.UpdateFromBundle)
            {
#if !UNITY_EDITOR
            if (UpdateData.DefaultComponentValue == null)
                Image = null;
            else
                Image = (Texture2D)UpdateData.DefaultComponentValue;
#endif
            }
        }

        public abstract Texture2D Image { get; set; }
        public abstract Material Material { get; set; }

        public Color Color { get { return Material.color; } set { Material.color = value; } }
    }
}