using UnityEngine;
using System.Collections;

namespace Playmove
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PYButtonToggleCheck : PYButtonToggle
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
        private GameObject _checkObj;
        private PYAnimation _checkAnim;
        private PYAnimation CheckAnim
        {
            get
            {
                if (_checkAnim == null)
                    _checkAnim = _checkObj.GetComponent<PYAnimation>();
                return _checkAnim;
            }
        }

        public bool IsHighlighted { get; set; }

        protected override void Start()
        {
            base.Start();
            UpdateCheckObj();
        }

        public override void Select()
        {
            base.Select();

            IsHighlighted = false;
            UpdateCheckObj();
        }

        public override void Deselect()
        {
            base.Deselect();

            IsHighlighted = false;
            UpdateCheckObj();
        }

        public void Highlight()
        {
            IsHighlighted = true;
            UpdateCheckObj();
        }
        public void DesHighlight()
        {
            IsHighlighted = false;
            UpdateCheckObj();
        }

        void UpdateCheckObj()
        {
            if (IsSelected)
            {
                _checkObj.SetActive(true);

                if (CheckAnim)
                    CheckAnim.Play();
            }
            else
            {
                if (CheckAnim)
                    CheckAnim.Reverse(() => _checkObj.SetActive(false));
                else
                    _checkObj.SetActive(false);
            }
        }
    }
}