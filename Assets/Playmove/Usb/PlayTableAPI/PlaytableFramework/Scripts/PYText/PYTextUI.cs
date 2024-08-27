using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Playmove
{
    [RequireComponent(typeof(Text))]
    public class PYTextUI : PYText
    {
        private Text _textUI;
        public Text TextUI
        {
            get
            {
                if (_textUI == null)
                    _textUI = GetComponent<Text>();
                return _textUI;
            }
        }

        public override string Text
        {
            get
            {
                TextUI.text = ApplyFormatter(TextUI.text);
                return TextUI.text;
            }
            set { TextUI.text = ApplyFormatter(value); }
        }

        public override Color Color
        {
            get { return TextUI.color; }
            set { TextUI.color = value; }
        }

        public override Font Font
        {
            get { return TextUI.font; }
            set { TextUI.font = value; }
        }

        public override float FontSize
        {
            get { return TextUI.fontSize; }
            set { TextUI.fontSize = (int)value; }
        }

        public override float CharacterSize
        {
            get { return TextUI.fontSize; }
            set { TextUI.fontSize = (int)value; }
        }

        public override float LineHeight
        {
            get { return TextUI.lineSpacing; }
            set { TextUI.lineSpacing = value; }
        }

        public override TextAnchor Anchor
        {
            get { return TextUI.alignment; }
            set { TextUI.alignment = value; }
        }

        public override TextAlignment Alignment
        {
            get { return TextAlignment.Center; }
            set { }
        }
    }
}