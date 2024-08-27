using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Playmove
{
    public class PYButtonContentText : PYButtonContent
    {

        public PYText Text;

        public override void SetContent(object content)
        {
            Text.Text = (string)content;
        }
    }
}