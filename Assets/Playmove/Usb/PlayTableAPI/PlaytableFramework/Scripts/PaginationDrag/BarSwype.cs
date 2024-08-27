using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;


namespace Playmove
{
    public class BarSwype : PYDrag
    {

        public float SwipeDistance, SwipeDuration;

        /// <summary>
        /// Gets the direction of the swype.
        /// </summary>
        /// <value>
        /// The direction represented by the Y value, positive represent going up, negative going down.
        /// </value>
        public new float Direction { get; private set; }

        private float _initialSwipeTime, _initialDragYPos;

        [Serializable]
        public class PYSwypeEvent : UnityEvent<BarSwype> { }

        [SerializeField]
        protected PYSwypeEvent _onSwype = new PYSwypeEvent();
        public PYSwypeEvent onSwype
        {
            get { return _onSwype; }
            set { _onSwype = value; }
        }

        protected override void DragBeginAction(PointerEventData eventData)
        {
            base.DragBeginAction(eventData);

            _initialSwipeTime = Time.time;
            _initialDragYPos = eventData.pointerCurrentRaycast.worldPosition.y;
        }

        protected override void DragEndAction(PointerEventData eventData)
        {
            base.DragEndAction(eventData);
            if (Mathf.Abs(eventData.pointerCurrentRaycast.worldPosition.y - _initialDragYPos) >= SwipeDistance)
            {
                if (Mathf.Abs(Time.time - _initialSwipeTime) <= SwipeDuration)
                {
                    Direction = _initialDragYPos - eventData.pointerCurrentRaycast.worldPosition.y;
                    onSwype.Invoke(this);
                }
            }
        }
    }
}
