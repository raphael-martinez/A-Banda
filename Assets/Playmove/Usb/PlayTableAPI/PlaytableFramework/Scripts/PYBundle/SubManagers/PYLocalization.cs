using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;

namespace Playmove
{
    /// <summary>
    /// Responsible for the Localization bundles
    /// </summary>
    public partial class PYLocalization : PYBundleSubManager
    {
        public override int PrepareToLoad()
        {
            _globalBundlesPath = PYBundleFolderScanner.GetGlobalLocalizationBundlesPath(PYBundleManager.Instance.Language);
            _localBundlesPath = PYBundleFolderScanner.GetExpansionLocalizedBundlesPath(PYBundleManager.Instance.ExpansionName,
                PYBundleType.Localization, PYBundleManager.Instance.Language);

            return _globalBundlesPath.Count + _localBundlesPath.Count;
        }

        public override void Load(Action callbackCompleted)
        {
            LoadBundle(callbackCompleted);
        }
    }
}