using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class TextMeshContentDrag : AbstractContentDrag
    {
        [Header("TextMeshContentDrag")]
        public PYText TextContent;

        protected override Transform ContentTransform
        {
            get
            {
                return TextContent.transform;
            }
        }

        protected override float UpLimit
        {
            get
            {
                return TextContent.GetComponent<Renderer>().bounds.max.y;
            }
        }

        protected override float DownLimit
        {
            get
            {
                return TextContent.GetComponent<Renderer>().bounds.min.y;
            }
        }

    }
}