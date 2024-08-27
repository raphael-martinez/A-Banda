using UnityEngine;
using System.Collections;

namespace Playmove
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PYButtonCheckbox : PYButton
    {
        [Header("PYButtonCheckbox")]
        public PYButtonEvent onChecked = new PYButtonEvent();

        public GameObject ToggleSprite;
        private PYAnimation _toggleSpriteAnim;

        public bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                onChecked.Invoke(this);
                UpdateToggleSprite();
            }
        }

        protected override void Start()
        {
            base.Start();
            _toggleSpriteAnim = ToggleSprite.GetComponent<PYAnimation>();

            UpdateToggleSprite();
        }

        protected override void ClickAction()
        {
            base.ClickAction();
            IsChecked = !IsChecked;
        }

        void UpdateToggleSprite()
        {
            if (_isChecked)
                ToggleSprite.SetActive(true);
            else if (_toggleSpriteAnim == null)
                ToggleSprite.SetActive(false);

            if (_toggleSpriteAnim == null)
                return;

            if (_isChecked)
                _toggleSpriteAnim.Play();
            else
                _toggleSpriteAnim.Reverse(() => ToggleSprite.SetActive(false));
        }
    }
}