using UnityEngine;

namespace Playmove
{
    public class PYButtonToggleColor : PYButtonToggle
    {
        [SerializeField]
        private PYImage _spriteRenderer;

        protected PYImage MySpriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                    _spriteRenderer = GetComponent<PYImage>();
                return _spriteRenderer;
            }
        }

        [SerializeField]
        private Color _color;

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                MySpriteRenderer.Color = _color * (IsSelected ? SelectColor : DeselectColor);
            }
        }

        public Color DeselectColor = Color.gray, SelectColor = Color.white;

        protected override void SelectAction()
        {
            MySpriteRenderer.Color = Color * SelectColor;
        }

        protected override void DeselectAction()
        {
            MySpriteRenderer.Color = Color * DeselectColor;
        }
    }
}