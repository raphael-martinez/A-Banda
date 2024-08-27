using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class PYButtonColor : PYButton
    {
        public Color Normal = Color.white;
        public Color Pressed = new Color(0.78f, 0.78f, 0.78f, 1);
        public Color Disabled = Color.gray;

        private SpriteRenderer[] renders;
        public SpriteRenderer[] Renders
        {
            get
            {
                if (renders == null)
                    renders = GetComponentsInChildren<SpriteRenderer>();
                return renders;
            }
        }

        private PYText[] _texts;
        public PYText[] Texts
        {
            get
            {
                if (_texts == null)
                    _texts = GetComponentsInChildren<PYText>();

                if (_initialTextsColors == null && _texts.Length > 0)
                {
                    _initialTextsColors = new Color[_texts.Length];
                    for (int x = 0; x < _initialTextsColors.Length; x++)
                        _initialTextsColors[x] = _texts[x].Color;
                }

                return _texts;
            }
        }
        private Color[] _initialTextsColors;

        protected override void Start()
        {
            renders = GetComponentsInChildren<SpriteRenderer>();
            base.Start();
        }

        protected override void DownAction()
        {
            SetColor(Pressed);
        }

        protected override void UpAction()
        {
            SetColor(Normal);
        }

        protected override void EnableAction()
        {
            SetColor(Normal);
        }
        protected override void DisableAction()
        {
            SetColor(Disabled);
        }

        void SetColor(Color color)
        {
            for (int x = 0; x < Renders.Length; x++)
            {
                Renders[x].color = color;
            }

            for (int x = 0; x < Texts.Length; x++)
            {
                Texts[x].Color = _initialTextsColors[x] * color;
            }
        }
    }
}