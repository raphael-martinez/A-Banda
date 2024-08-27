using UnityEngine;
using System.Collections;

namespace Playmove
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PYButtonToggleSprite : PYButtonToggle
    {
        private SpriteRenderer render;
        public SpriteRenderer Render
        {
            get
            {
                if (render == null)
                    render = GetComponent<SpriteRenderer>();
                return render;
            }
        }

        [SerializeField]
        private Sprite _spriteDeselected;
        [SerializeField]
        private Sprite _spriteSelected;

        public bool IsHighlighted { get; set; }

        protected override void Start()
        {
            base.Start();

            if (IsSelected)
                Render.sprite = _spriteSelected;
            else
                Render.sprite = _spriteDeselected;
        }

        public override void Select()
        {
            base.Select();

            IsHighlighted = false;
            if (IsSelected)
                Render.sprite = _spriteSelected;
        }

        public override void Deselect()
        {
            base.Deselect();

            IsHighlighted = false;
            Render.sprite = _spriteDeselected;
        }

        public void Highlight()
        {
            IsHighlighted = true;
            Render.sprite = _spriteSelected;
        }
        public void DesHighlight()
        {
            IsHighlighted = false;
            Render.sprite = _spriteDeselected;
        }
    }
}