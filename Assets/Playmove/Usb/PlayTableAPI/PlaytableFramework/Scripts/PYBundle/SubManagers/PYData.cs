using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

namespace Playmove
{
    /// <summary>
    /// Responsible for all Game Data bundles
    /// </summary>
    public partial class PYData : PYBundleSubManager
    {
        public override int PrepareToLoad()
        {
            _globalBundlesPath = PYBundleFolderScanner.GetGlobalBundlesPath(PYBundleType.Data);
            _localBundlesPath = PYBundleFolderScanner.GetExpansionBundlesPath(PYBundleManager.Instance.ExpansionName, PYBundleType.Data);

            return _globalBundlesPath.Count + _localBundlesPath.Count;
        }

        public override void Load(Action callbackCompleted)
        {
            LoadBundle(callbackCompleted);
        }
    }
}
