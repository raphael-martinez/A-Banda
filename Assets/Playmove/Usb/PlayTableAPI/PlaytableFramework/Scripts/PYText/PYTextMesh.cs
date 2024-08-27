using UnityEngine;
using System.Collections;

namespace Playmove
{
    [RequireComponent(typeof(TextMesh))]
    public class PYTextMesh : PYText
    {
        private TextMesh _textMesh;
        public TextMesh TextMesh
        {
            get
            {
                if (_textMesh == null)
                    _textMesh = GetComponent<TextMesh>();
                return _textMesh;
            }
        }

        public override string Text
        {
            get
            {
                TextMesh.text = ApplyFormatter(TextMesh.text);
                return TextMesh.text;
            }
            set { TextMesh.text = ApplyFormatter(value); }
        }

        public override Color Color
        {
            get { return TextMesh.color; }
            set { TextMesh.color = value; }
        }

        public override Font Font
        {
            get { return TextMesh.font; }
            set { TextMesh.font = value; }
        }

        public override float FontSize
        {
            get { return TextMesh.fontSize; }
            set { TextMesh.fontSize = (int)value; }
        }

        public override float CharacterSize
        {
            get { return TextMesh.characterSize; }
            set { TextMesh.characterSize = value; }
        }

        public override float LineHeight
        {
            get { return TextMesh.lineSpacing; }
            set { TextMesh.lineSpacing = value; }
        }

        public override TextAnchor Anchor
        {
            get { return TextMesh.anchor; }
            set { TextMesh.anchor = value; }
        }

        public override TextAlignment Alignment
        {
            get { return TextMesh.alignment; }
            set { TextMesh.alignment = value; }
        }
    }
}