using UnityEngine;
using System.Collections;
using Playmove.Core.Bundles;

namespace Playmove
{
    [RequireComponent(typeof(SortingOrder))]
    public abstract class PYText : PYComponentBundle
    {
        public PYBundleAssetTag AssetTag;

        [Header("Formatter")]
        [SerializeField, ContextMenuItem("Apply Formatter", "ApplyFormatterEditor")]
        protected bool _toUpper = false;
        [SerializeField, ContextMenuItem("Apply Formatter", "ApplyFormatterEditor")]
        protected bool _toLower = false;
        [SerializeField, ContextMenuItem("Apply Formatter", "ApplyFormatterEditor")]
        protected string _prefix = "";
        private string _lastPrefix = "";
        [SerializeField, ContextMenuItem("Apply Formatter", "ApplyFormatterEditor")]
        protected string _suffix = "";
        private string _lastSuffix = "";

        protected SortingOrder _sortingOrder;
        public SortingOrder SortingOrder
        {
            get
            {
                if (_sortingOrder == null)
                    _sortingOrder = GetComponent<SortingOrder>();
                return _sortingOrder;
            }
        }

        public abstract string Text { get; set; }
        public abstract Color Color { get; set; }
        public abstract Font Font { get; set; }
        public abstract float FontSize { get; set; }
        public abstract float CharacterSize { get; set; }
        public abstract float LineHeight { get; set; }
        public abstract TextAnchor Anchor { get; set; }
        public abstract TextAlignment Alignment { get; set; }

        public void SetOrder(int order)
        {
            SortingOrder.ChangeSortingOrder(order);
        }
        public void SetOrder(int order, string layerName)
        {
            SortingOrder.ChangeSortingOrder(order, layerName);
        }

        #region Formatter
        public void ClearFormatter()
        {
            _toUpper = _toLower = false;
            _prefix = _suffix = "";
            Text = ApplyFormatter(Text);
        }

        public void SetToUpper(bool toUpper = true)
        {
            _toUpper = toUpper;
            Text = ApplyFormatter(Text);
        }
        public void SetToLower(bool toLower = true)
        {
            _toLower = toLower;
            Text = ApplyFormatter(Text);
        }

        public void SetPrefix(string prefix)
        {
            _prefix = prefix;
            Text = ApplyFormatter(Text);
        }
        public void SetSuffix(string suffix)
        {
            _suffix = suffix;
            Text = ApplyFormatter(Text);
        }
        #endregion

        public override void UpdateComponent()
        {
            UpdateData.DefaultComponentValue = Text;
            if (UpdateData.UpdateFromBundle)
            {
                string assetText = Localization.GetAsset<string>(AssetTag.Tag);
                if (!string.IsNullOrEmpty(assetText))
                    Text = assetText;
            }
        }

        public override void RestoreComponent()
        {
            if (UpdateData.UpdateFromBundle)
                Text = (string)UpdateData.DefaultComponentValue;
        }

        protected string ApplyFormatter(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            if (!string.IsNullOrEmpty(_lastPrefix))
                text = text.Replace(_lastPrefix, "");
            if (!string.IsNullOrEmpty(_lastSuffix))
                text = text.Replace(_lastSuffix, "");

            // Apply toUpper and toLower
            if (_toUpper) text = text.ToUpper();
            else if (_toLower) text = text.ToLower();

            // Add new prefix and suffix
            if (!string.IsNullOrEmpty(_prefix) && !text.StartsWith(_prefix))
                text = text.Insert(0, _prefix);

            if (!string.IsNullOrEmpty(_suffix) && !text.EndsWith(_suffix))
                text += _suffix;

            _lastPrefix = _prefix;
            _lastSuffix = _suffix;

            return text;
        }

#if UNITY_EDITOR
        protected void ApplyFormatterEditor()
        {
            Text = ApplyFormatter(Text);
        }
#endif
    }
}