using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class PercentageTest : MonoBehaviour
    {

        private AbstractContentDrag _contentDrag;
        private AbstractContentDrag ContentDrag
        {
            get
            {
                if (!_contentDrag) _contentDrag = GetComponent<AbstractContentDrag>();
                return _contentDrag;
            }
        }

        private TextMesh _testText;
        public TextMesh TestText
        {
            get
            {
                if (!_testText)
                {
                    _testText = gameObject.AddComponent<TextMesh>();
                    _testText.color = Color.black;
                    Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                    _testText.font = ArialFont;
                    _testText.GetComponent<Renderer>().sharedMaterial = ArialFont.material;
                    _testText.characterSize = 0.3f;

                    _testText.GetComponent<Renderer>().sortingLayerName = "GUI";
                    _testText.GetComponent<Renderer>().sortingOrder = 99;
                }
                return _testText;
            }
        }

        void Update()
        {
            TestText.text = string.Format("Content: {0} \nBar: {1}", ContentDrag.Percentage, ContentDrag.BarDrag.Percentage);
        }
    }
}