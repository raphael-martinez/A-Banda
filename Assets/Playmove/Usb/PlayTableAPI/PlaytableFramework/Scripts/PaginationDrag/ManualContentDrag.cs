using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class ManualContentDrag : AbstractContentDrag
    {

        [Header("ManualContentDrag")]
        public Transform Content;
        public Transform ContentUpLimit, ContentDownLimit;

        private float _upLimit = 0;
        protected override float UpLimit
        {
            get
            {
                if (_upLimit == 0 && ContentUpLimit != null)
                {
                    _upLimit = ContentUpLimit.position.y;
                }
                return _upLimit;
            }
        }

        private float _downLimit = 0;
        protected override float DownLimit
        {
            get
            {
                if (_downLimit == 0 && ContentDownLimit != null)
                {
                    _downLimit = ContentDownLimit.position.y;
                }
                return _downLimit;
            }
        }

        protected override Transform ContentTransform
        {
            get
            {
                return Content;
            }
        }

        protected override void DragBeginAction(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.DragBeginAction(eventData);
            eventData.selectedObject = eventData.pointerCurrentRaycast.gameObject;
        }

        protected override void DragEndAction(UnityEngine.EventSystems.PointerEventData eventData)
        {
            base.DragEndAction(eventData);

            if (eventData.selectedObject == eventData.pointerCurrentRaycast.gameObject)
                eventData.selectedObject.GetComponent<PYButton>().OnPointerClick(eventData);
        }
    }
}