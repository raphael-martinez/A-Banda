using Playmove.Core.Bundles;
using UnityEngine;
using UnityEngine.UI;

namespace Playmove.Framework.Localizers
{
    [RequireComponent(typeof(Image))]
    public class LocalizerImage : Localizer<Image>
    {
        protected override void Localize()
        {
            if (string.IsNullOrEmpty(AssetName)) return;
            Component.sprite = Localization.GetAsset(AssetName, Component.sprite);
        }
    }
}
