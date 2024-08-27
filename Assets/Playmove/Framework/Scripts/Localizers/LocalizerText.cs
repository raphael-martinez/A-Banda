using Playmove.Core.Bundles;
using UnityEngine;
using UnityEngine.UI;

namespace Playmove.Framework.Localizers
{
    [RequireComponent(typeof(Text))]
    public class LocalizerText : Localizer<Text>
    {
        protected override void Localize()
        {
            if (string.IsNullOrEmpty(AssetName)) return;
            Component.text = Localization.GetAsset(AssetName, Component.text);
        }
    }
}
