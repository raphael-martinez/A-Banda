using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Playmove
{
    public class PYButtonContentImage : PYButtonContent
    {

        public Image ButtonImage;

        public override void SetContent(object content)
        {
            ButtonImage.sprite = (Sprite)content;
        }
    }
}