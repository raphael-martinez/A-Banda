using Playmove.Core.Bundles;
using UnityEngine;

namespace Playmove.Framework.Localizers
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LocalizerSpriteRenderer : Localizer<SpriteRenderer>
    {
        protected override void Localize()
        {
            if (string.IsNullOrEmpty(AssetName)) return;
            Component.sprite = Localization.GetAsset(AssetName, Component.sprite);
        }
    }
}
