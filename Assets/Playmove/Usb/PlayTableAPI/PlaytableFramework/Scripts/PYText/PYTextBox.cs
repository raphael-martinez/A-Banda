using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace Playmove
{
    [RequireComponent(typeof(TextMesh))]
    [ExecuteInEditMode]
    public class PYTextBox : PYText
    {
        private string _wrappedText;

        [Header("PYTextBox")]
        #region Props
        [Multiline]
        [SerializeField]
        private string _text;
        public override string Text
        {
            get
            {
                _text = ApplyFormatter(_text);
                return _text;
            }
            set
            {
                _text = ApplyFormatter(value);
                UpdateText();
            }
        }

        [SerializeField]
        private Font _font;
        public override Font Font
        {
            get
            {
                return TextMesh.font;
            }
            set
            {
                _font = value;
                TextMesh.font = value;
                if (_font != null)
                    GetComponent<Renderer>().sharedMaterial = _font.material;
                GetAverageCharSize();
                UpdateText();
            }
        }

        [SerializeField]
        private Color _color;
        public override Color Color
        {
            get
            {
                return TextMesh.color;
            }
            set
            {
                _color = value;
                TextMesh.color = value;
                //UpdateText();
            }
        }

        [SerializeField]
        private float _characterSize = 1;
        public override float CharacterSize
        {
            get
            {
                return _characterSize;
            }
            set
            {
                _characterSize = value;
                TextMesh.characterSize = _characterSize;
                UpdateText();
            }
        }

        [SerializeField]
        private float _fontSize;
        public override float FontSize
        {
            get
            {
                return TextMesh.fontSize;
            }
            set
            {
                _fontSize = value;
                TextMesh.fontSize = (int)value;
                UpdateText();
            }
        }

        [SerializeField]
        private TextAnchor _anchor;
        public override TextAnchor Anchor
        {
            get
            {
                return TextMesh.anchor;
            }
            set
            {
                _anchor = value;
                TextMesh.anchor = value;
                UpdateText();
            }
        }

        [SerializeField]
        private TextAlignment _alignment;
        public override TextAlignment Alignment
        {
            get
            {
                return TextMesh.alignment;
            }
            set
            {
                _alignment = value;
                TextMesh.alignment = value;
                UpdateText();
            }
        }

        [SerializeField]
        private float _lineHeight;
        public override float LineHeight
        {
            get { return TextMesh.lineSpacing; }
            set
            {
                TextMesh.lineSpacing = value;
                UpdateText();
            }
        }

        [Space(5)]
        [SerializeField]
        private bool _wrap;
        public bool Wrap
        {
            get
            {
                return _wrap;
            }
            set
            {
                _wrap = value;
                UpdateText();
            }
        }

        [SerializeField]
        private float _wordWrap;
        public float WordWrap
        {
            get
            {
                return _wordWrap;
            }
            set
            {
                _wordWrap = value;
                UpdateText();
            }
        }

        [Space(5)]
        [SerializeField]
        private bool _fit;
        public bool Fit
        {
            get
            {
                return _fit;
            }
            set
            {
                _fit = value;
                UpdateText();
            }
        }

        private TextMesh _textMesh;
        public TextMesh TextMesh
        {
            get
            {
                if (_textMesh == null)
                {
                    _textMesh = GetComponent<TextMesh>();
                    if (_textMesh == null)
                        _textMesh = gameObject.AddComponent<TextMesh>();
                }

                _textMesh.hideFlags = HideFlags.HideInInspector;
                return _textMesh;
            }
        }

        [SerializeField]
        private Vector2 _maxSize;
        public Vector2 MaxSize
        {
            get
            {
                return _maxSize;
            }
            set
            {
                _maxSize = value;
                UpdateText();
            }
        }

        private int _averageCharSize;
        private int AverageCharSize
        {
            get
            {
                if (_averageCharSize == 0)
                    GetAverageCharSize();
                return _averageCharSize;
            }
        }

        private Transform _myTransform;
        private Transform MyTransform
        {
            get
            {
                if (!_myTransform) _myTransform = transform;
                return _myTransform;
            }
        }
        #endregion

        #region Unity
        void OnEnable()
        {
#if UNITY_EDITOR

            if (_text == null || _text.Trim().Length == 0)
                _text = ApplyFormatter(TextMesh.text);

            if (Font != _font)
                _font = Font;

            if (Color != _color)
                _color = Color;

            if (FontSize != _fontSize)
                _fontSize = FontSize;

            if (Anchor != _anchor)
                _anchor = Anchor;

            if (Alignment != _alignment)
                _alignment = Alignment;

#endif

            UpdateText();
        }

#if UNITY_EDITOR
        private string _prevText;
        private bool _prevWW, _prevFit;
        private float _prevWordWrap;
        private float _prevCharSize;
        private Vector2 _prevMaxSize;
        void Update()
        {
            if (Font != _font)
                Font = _font;

            if (Color != _color)
                Color = _color;

            if (FontSize != _fontSize)
                FontSize = _fontSize;

            if (Anchor != _anchor)
                Anchor = _anchor;

            if (Alignment != _alignment)
                Alignment = _alignment;

            CharacterSize = Mathf.Clamp(CharacterSize, 0, Mathf.Infinity);
            FontSize = (int)Mathf.Clamp(FontSize, 0, Mathf.Infinity);

            WordWrap = Mathf.Clamp(WordWrap, 0, Mathf.Infinity);

            float maxX = Mathf.Clamp(MaxSize.x, 0, Mathf.Infinity);
            float maxY = Mathf.Clamp(MaxSize.y, 0, Mathf.Infinity);
            MaxSize = new Vector2(maxX, maxY);

            if (Text != _prevText || Wrap != _prevWW || Fit != _prevFit || WordWrap != _prevWordWrap || CharacterSize != _prevCharSize || MaxSize != _prevMaxSize)
                UpdateText();

            _prevText = Text;
            _prevWW = Wrap;
            _prevFit = Fit;
            _prevWordWrap = WordWrap;
            _prevCharSize = CharacterSize;
            _prevMaxSize = MaxSize;

            if (Wrap)
                DrawWrap();

            if (Fit)
                DrawFitRectangle();

        }

        void DrawWrap()
        {
            Vector2 multiplierStart, multiplierEnd;

            //left line
            switch (TextMesh.anchor)
            {
                case TextAnchor.LowerCenter:
                    multiplierStart = new Vector2(-0.5f, 1f);
                    multiplierEnd = new Vector2(-0.5f, 0f);
                    break;
                case TextAnchor.LowerLeft:
                    multiplierStart = new Vector2(0f, 1f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.LowerRight:
                    multiplierStart = new Vector2(-1f, 1f);
                    multiplierEnd = new Vector2(-1f, 0f);
                    break;
                case TextAnchor.MiddleCenter:
                    multiplierStart = new Vector2(-0.5f, -0.5f);
                    multiplierEnd = new Vector2(-0.5f, 0.5f);
                    break;
                case TextAnchor.MiddleLeft:
                    multiplierStart = new Vector2(0f, -0.5f);
                    multiplierEnd = new Vector2(0f, 0.5f);
                    break;
                case TextAnchor.MiddleRight:
                    multiplierStart = new Vector2(-1f, -0.5f);
                    multiplierEnd = new Vector2(-1f, 0.5f);
                    break;
                case TextAnchor.UpperCenter:
                    multiplierStart = new Vector2(-0.5f, -1f);
                    multiplierEnd = new Vector2(-0.5f, 0f);
                    break;
                case TextAnchor.UpperLeft:
                    multiplierStart = new Vector2(0f, -1f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.UpperRight:
                    multiplierStart = new Vector2(-1f, -1f);
                    multiplierEnd = new Vector2(-1f, 0f);
                    break;
                default:
                    multiplierStart = new Vector2(-0.5f, 0.5f);
                    multiplierEnd = new Vector2(-0.5f, 0.5f);
                    break;
            }
            DrawWrapLine(multiplierStart, multiplierEnd);

            //rignt line
            switch (TextMesh.anchor)
            {
                case TextAnchor.LowerCenter:
                    multiplierStart = new Vector2(0.5f, 1f);
                    multiplierEnd = new Vector2(0.5f, 0f);
                    break;
                case TextAnchor.LowerLeft:
                    multiplierStart = new Vector2(1f, 1f);
                    multiplierEnd = new Vector2(1f, 0f);
                    break;
                case TextAnchor.LowerRight:
                    multiplierStart = new Vector2(0f, 1f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.MiddleCenter:
                    multiplierStart = new Vector2(0.5f, -0.5f);
                    multiplierEnd = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.MiddleLeft:
                    multiplierStart = new Vector2(1f, -0.5f);
                    multiplierEnd = new Vector2(1f, 0.5f);
                    break;
                case TextAnchor.MiddleRight:
                    multiplierStart = new Vector2(0f, -0.5f);
                    multiplierEnd = new Vector2(0f, 0.5f);
                    break;
                case TextAnchor.UpperCenter:
                    multiplierStart = new Vector2(0.5f, -1f);
                    multiplierEnd = new Vector2(0.5f, 0f);
                    break;
                case TextAnchor.UpperLeft:
                    multiplierStart = new Vector2(1f, -1f);
                    multiplierEnd = new Vector2(1f, 0f);
                    break;
                case TextAnchor.UpperRight:
                    multiplierStart = new Vector2(0f, -1f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                default:
                    multiplierStart = new Vector2(0.5f, 0.5f);
                    multiplierEnd = new Vector2(0.5f, 0.5f);
                    break;
            }
            DrawWrapLine(multiplierStart, multiplierEnd);
        }

        void DrawWrapLine(Vector2 multiplierStart, Vector2 multiplierEnd)
        {
            Vector3 pivot = transform.position;
            Vector3 startPoint, endPoint;
            Vector3 start, end;

            startPoint = endPoint = pivot;

            float charSize = WordWrap * MyTransform.lossyScale.x;
            if (WordWrap > 0)
            {
                startPoint.x = pivot.x + charSize * multiplierStart.x;
                endPoint.x = pivot.x + charSize * multiplierEnd.x;
            }
            else
            {
                startPoint.x = pivot.x + GetComponent<Renderer>().bounds.size.x * multiplierStart.x;
                endPoint.x = pivot.x + GetComponent<Renderer>().bounds.size.x * multiplierEnd.x;
            }

            startPoint.y = pivot.y + GetComponent<Renderer>().bounds.size.y * multiplierStart.y;
            endPoint.y = pivot.y + GetComponent<Renderer>().bounds.size.y * multiplierEnd.y;

            start = transform.rotation * (startPoint - pivot) + pivot;
            start.z = transform.position.z;
            end = transform.rotation * (endPoint - pivot) + pivot;
            end.z = transform.position.z;

            Color color = TextMesh.color;
            color.r += 0.5f;
            color.g -= 0.5f;
            color.b -= 0.5f;

            Debug.DrawLine(start, end, color);
        }

        void DrawFitRectangle()
        {
            Vector2 multiplierStart, multiplierEnd;

            //upper line
            switch (TextMesh.anchor)
            {
                case TextAnchor.LowerCenter:
                    multiplierStart = new Vector2(-0.5f, 1f);
                    multiplierEnd = new Vector2(0.5f, 1f);
                    break;
                case TextAnchor.LowerLeft:
                    multiplierStart = new Vector2(1f, 1f);
                    multiplierEnd = new Vector2(0f, 1f);
                    break;
                case TextAnchor.LowerRight:
                    multiplierStart = new Vector2(-1f, 1f);
                    multiplierEnd = new Vector2(0f, 1f);
                    break;
                case TextAnchor.MiddleCenter:
                    multiplierStart = new Vector2(-0.5f, 0.5f);
                    multiplierEnd = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.MiddleLeft:
                    multiplierStart = new Vector2(1f, 0.5f);
                    multiplierEnd = new Vector2(0f, 0.5f);
                    break;
                case TextAnchor.MiddleRight:
                    multiplierStart = new Vector2(-1f, 0.5f);
                    multiplierEnd = new Vector2(0f, 0.5f);
                    break;
                case TextAnchor.UpperCenter:
                    multiplierStart = new Vector2(-0.5f, 0f);
                    multiplierEnd = new Vector2(0.5f, 0f);
                    break;
                case TextAnchor.UpperLeft:
                    multiplierStart = new Vector2(1f, 0f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.UpperRight:
                    multiplierStart = new Vector2(-1f, 0f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                default:
                    multiplierStart = new Vector2(-0.5f, 0.5f);
                    multiplierEnd = new Vector2(0.5f, 0.5f);
                    break;
            }
            DrawFitRectangleLine(multiplierStart, multiplierEnd);

            //lower line
            switch (TextMesh.anchor)
            {
                case TextAnchor.LowerCenter:
                    multiplierStart = new Vector2(-0.5f, 0f);
                    multiplierEnd = new Vector2(0.5f, 0f);
                    break;
                case TextAnchor.LowerLeft:
                    multiplierStart = new Vector2(1f, 0f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.LowerRight:
                    multiplierStart = new Vector2(-1f, 0f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.MiddleCenter:
                    multiplierStart = new Vector2(-0.5f, -0.5f);
                    multiplierEnd = new Vector2(0.5f, -0.5f);
                    break;
                case TextAnchor.MiddleLeft:
                    multiplierStart = new Vector2(1f, -0.5f);
                    multiplierEnd = new Vector2(0f, -0.5f);
                    break;
                case TextAnchor.MiddleRight:
                    multiplierStart = new Vector2(-1f, -0.5f);
                    multiplierEnd = new Vector2(0f, -0.5f);
                    break;
                case TextAnchor.UpperCenter:
                    multiplierStart = new Vector2(-0.5f, -1f);
                    multiplierEnd = new Vector2(0.5f, -1f);
                    break;
                case TextAnchor.UpperLeft:
                    multiplierStart = new Vector2(1f, -1f);
                    multiplierEnd = new Vector2(0f, -1f);
                    break;
                case TextAnchor.UpperRight:
                    multiplierStart = new Vector2(-1f, -1f);
                    multiplierEnd = new Vector2(0f, -1f);
                    break;
                default:
                    multiplierStart = new Vector2(-0.5f, -0.5f);
                    multiplierEnd = new Vector2(0.5f, -0.5f);
                    break;
            }
            DrawFitRectangleLine(multiplierStart, multiplierEnd);

            //left line
            switch (TextMesh.anchor)
            {
                case TextAnchor.LowerCenter:
                    multiplierStart = new Vector2(-0.5f, 1f);
                    multiplierEnd = new Vector2(-0.5f, 0f);
                    break;
                case TextAnchor.LowerLeft:
                    multiplierStart = new Vector2(0f, 1f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.LowerRight:
                    multiplierStart = new Vector2(-1f, 1f);
                    multiplierEnd = new Vector2(-1f, 0f);
                    break;
                case TextAnchor.MiddleCenter:
                    multiplierStart = new Vector2(-0.5f, -0.5f);
                    multiplierEnd = new Vector2(-0.5f, 0.5f);
                    break;
                case TextAnchor.MiddleLeft:
                    multiplierStart = new Vector2(0f, -0.5f);
                    multiplierEnd = new Vector2(0f, 0.5f);
                    break;
                case TextAnchor.MiddleRight:
                    multiplierStart = new Vector2(-1f, -0.5f);
                    multiplierEnd = new Vector2(-1f, 0.5f);
                    break;
                case TextAnchor.UpperCenter:
                    multiplierStart = new Vector2(-0.5f, -1f);
                    multiplierEnd = new Vector2(-0.5f, 0f);
                    break;
                case TextAnchor.UpperLeft:
                    multiplierStart = new Vector2(0f, -1f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.UpperRight:
                    multiplierStart = new Vector2(-1f, -1f);
                    multiplierEnd = new Vector2(-1f, 0f);
                    break;
                default:
                    multiplierStart = new Vector2(-0.5f, 0.5f);
                    multiplierEnd = new Vector2(-0.5f, 0.5f);
                    break;
            }
            DrawFitRectangleLine(multiplierStart, multiplierEnd);

            //rignt line
            switch (TextMesh.anchor)
            {
                case TextAnchor.LowerCenter:
                    multiplierStart = new Vector2(0.5f, 1f);
                    multiplierEnd = new Vector2(0.5f, 0f);
                    break;
                case TextAnchor.LowerLeft:
                    multiplierStart = new Vector2(1f, 1f);
                    multiplierEnd = new Vector2(1f, 0f);
                    break;
                case TextAnchor.LowerRight:
                    multiplierStart = new Vector2(0f, 1f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                case TextAnchor.MiddleCenter:
                    multiplierStart = new Vector2(0.5f, -0.5f);
                    multiplierEnd = new Vector2(0.5f, 0.5f);
                    break;
                case TextAnchor.MiddleLeft:
                    multiplierStart = new Vector2(1f, -0.5f);
                    multiplierEnd = new Vector2(1f, 0.5f);
                    break;
                case TextAnchor.MiddleRight:
                    multiplierStart = new Vector2(0f, -0.5f);
                    multiplierEnd = new Vector2(0f, 0.5f);
                    break;
                case TextAnchor.UpperCenter:
                    multiplierStart = new Vector2(0.5f, -1f);
                    multiplierEnd = new Vector2(0.5f, 0f);
                    break;
                case TextAnchor.UpperLeft:
                    multiplierStart = new Vector2(1f, -1f);
                    multiplierEnd = new Vector2(1f, 0f);
                    break;
                case TextAnchor.UpperRight:
                    multiplierStart = new Vector2(0f, -1f);
                    multiplierEnd = new Vector2(0f, 0f);
                    break;
                default:
                    multiplierStart = new Vector2(0.5f, 0.5f);
                    multiplierEnd = new Vector2(0.5f, 0.5f);
                    break;
            }
            DrawFitRectangleLine(multiplierStart, multiplierEnd);

        }

        void DrawFitRectangleLine(Vector2 multiplierStart, Vector2 multiplierEnd)
        {
            Vector3 pivot = transform.position;
            Vector3 startPoint, endPoint;
            Vector3 start, end;

            startPoint = endPoint = pivot;

            Vector2 size = new Vector2(MaxSize.x * MyTransform.lossyScale.x, MaxSize.y * MyTransform.lossyScale.y);

            if (MaxSize.x > 0)
            {
                startPoint.x = pivot.x + size.x * multiplierStart.x;
                endPoint.x = pivot.x + size.x * multiplierEnd.x;
            }
            else
            {
                startPoint.x = pivot.x + GetComponent<Renderer>().bounds.extents.x * 2 * multiplierStart.x;
                endPoint.x = pivot.x + GetComponent<Renderer>().bounds.extents.x * 2 * multiplierEnd.x;
            }

            if (MaxSize.y > 0)
            {
                startPoint.y = pivot.y + size.y * multiplierStart.y;
                endPoint.y = pivot.y + size.y * multiplierEnd.y;
            }
            else
            {
                startPoint.y = pivot.y + GetComponent<Renderer>().bounds.extents.y * 2 * multiplierStart.y;
                endPoint.y = pivot.y + GetComponent<Renderer>().bounds.extents.y * 2 * multiplierEnd.y;
            }

            start = transform.rotation * (startPoint - pivot) + pivot;
            start.z = transform.position.z;
            end = transform.rotation * (endPoint - pivot) + pivot;
            end.z = transform.position.z;

            Color color = TextMesh.color;
            color.a = 0.5f;

            Debug.DrawLine(start, end, color);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TextMesh.hideFlags = HideFlags.None;
        }
#endif
        #endregion

        private void UpdateText()
        {
            float lastCharSize = TextMesh.characterSize;
            WrapText();
            Resize();

            if (TextMesh.characterSize < lastCharSize && Wrap)
                TextMesh.characterSize = lastCharSize;
        }

        private void GetAverageCharSize()
        {
            if (Font == null)
                return;
#if UNITY_5
            _averageCharSize = (int)Font.characterInfo.Average((c) => c.advance);
#else
        _averageCharSize = (int)Font.characterInfo.Average((c) => c.width);
#endif
        }

        #region Wrap
        private void WrapText()
        {
            TextMesh.text = Text;

            if (!Wrap)
                return;

            if (WordWrap <= 0)
                return;

            string finalText = "";
            string[] stringPart = Text != null ? Text.Split(' ') : new string[0];
            string line = "";

            for (int i = 0; i < stringPart.Length; i++)
            {
                string[] paragraph = stringPart[i].Trim().Split("\n"[0]);

                for (int j = 0; j < paragraph.Length; j++)
                {
                    string newLine = line;
                    newLine += paragraph[j].Trim();

                    CharacterInfo charInfo = new CharacterInfo();
                    int lineLength = newLine.Sum((c) =>
                    {
                        if (Font.dynamic)
                            Font.GetCharacterInfo(c, out charInfo, (int)FontSize);
                        else
                            Font.GetCharacterInfo(c, out charInfo);
#if UNITY_5
                        return charInfo.advance;
#else
                    return (int)charInfo.width;
#endif
                    });

                    float lineSize = lineLength * TextMesh.characterSize * MyTransform.lossyScale.x * 0.1f;

                    if (lineSize > WordWrap * MyTransform.lossyScale.x || j > 0)
                    {
                        finalText += line.Trim();
                        finalText += Environment.NewLine;
                        newLine = paragraph[j].Trim();
                        line = "";
                    }

                    line = newLine;
                }

                line += " ";
            }

            finalText += line;

            TextMesh.text = finalText.Trim();

            if (!Fit)
                return;

            Vector2 size = new Vector2(MaxSize.x * MyTransform.lossyScale.x, MaxSize.y * MyTransform.lossyScale.y);
            Bounds bounds = GetComponent<Renderer>().bounds;

            if ((bounds.size.x > size.x && size.x > 0) || (bounds.size.y > size.y && size.y > 0))
                Resize();
        }
        #endregion

        #region Resize
        public void Resize()
        {
            TextMesh.characterSize = CharacterSize;

            if (!Fit)
                return;

            Quaternion rotation = MyTransform.rotation;
            MyTransform.rotation = Quaternion.identity;

            Vector2 size = new Vector2(MaxSize.x * MyTransform.lossyScale.x, MaxSize.y * MyTransform.lossyScale.y);
            Bounds bounds = GetComponent<Renderer>().bounds;
            float resizedCharSizeX = CharacterSize;
            float resizedCharSizeY = CharacterSize;

            if (bounds.size.x > size.x && size.x > 0)
                resizedCharSizeX *= size.x / (bounds.size.x);
            else if (Wrap && size.x <= 0)
            {
                float wrapSize = WordWrap * transform.lossyScale.x;
                if (WordWrap > 0 && bounds.size.x > wrapSize)
                    resizedCharSizeX *= wrapSize / (bounds.size.x);
            }

            if (bounds.size.y > size.y && size.y > 0)
                resizedCharSizeY *= size.y / (bounds.size.y);

            if (resizedCharSizeX < resizedCharSizeY)
                TextMesh.characterSize = resizedCharSizeX;
            else
                TextMesh.characterSize = resizedCharSizeY;

            MyTransform.rotation = rotation;

        }
        #endregion
    }
}