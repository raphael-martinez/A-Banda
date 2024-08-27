using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Playmove
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PYButtonSprite : PYButton
    {
        public Sprite SpriteNormal;
        public Sprite SpritePressed;
        public Sprite SpriteDisabled;

        private SpriteRenderer _spriteRender;
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (_spriteRender == null)
                    _spriteRender = GetComponentInChildren<SpriteRenderer>();
                return _spriteRender;
            }
        }

        public override void SetContent(object content)
        {
            SpriteRenderer.sprite = (Sprite)content;
        }

        protected override void DownAction()
        {
            if (SpritePressed != null)
                SpriteRenderer.sprite = SpritePressed;
        }

        protected override void UpAction()
        {
            if (SpriteNormal != null)
                SpriteRenderer.sprite = SpriteNormal;
        }

        protected override void EnableAction()
        {
            if (SpriteNormal != null)
                SpriteRenderer.sprite = SpriteNormal;
        }
        protected override void DisableAction()
        {
            if (SpriteDisabled != null)
                SpriteRenderer.sprite = SpriteDisabled;
        }
    }
}