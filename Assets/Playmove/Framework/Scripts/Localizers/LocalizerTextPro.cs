using Playmove.Core.Bundles;
using TMPro;
using UnityEngine;

namespace Playmove.Framework.Localizers
{
    [RequireComponent(typeof(TextMeshPro))]
    public class LocalizerTextPro : Localizer<TextMeshPro>
    {
        protected override void Localize()
        {
            if (string.IsNullOrEmpty(AssetName)) return;
            Component.text = Localization.GetAsset(AssetName, Component.text);
        }
    }
}
