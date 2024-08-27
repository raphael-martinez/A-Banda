using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace Playmove
{
    public class ContentDrag : PYDrag
    {
        public float DeadAreaMaxDistance = 0.03f;
        public float Velocity = 1;
        public Transform Content;
        public float MaxDistance, StartPosition;

        [Range(0, 100)]
        public float Percentage;
        //public float Percentage { get; private set; }

        #region Events
        [Serializable]
        public class PYContentDragEvent : UnityEvent<ContentDrag> { }

        [SerializeField]
        protected PYContentDragEvent _onContentDrag = new PYContentDragEvent();
        public PYContentDragEvent onContentDrag
        {
            get { return _onContentDrag; }
            set { _onContentDrag = value; }
        }
        #endregion

        private Vector3 _pointerWorldPosition, _aplhaPos;
        private float _initialSwipeTime, _initialDragYPos;
        private float _deadDistance;

        private void Update()
        {
            if (Percentage < 0)
                UpdateContentPosition(0);
            if (Percentage > 100)
                UpdateContentPosition(100);
        }

        protected override void DragBeginAction(PointerEventData eventData)
        {
            base.DragBeginAction(eventData);
            _aplhaPos = eventData.pointerCurrentRaycast.worldPosition;

            _deadDistance = 0;
        }

        protected override void DraggingAction(PointerEventData eventData)
        {
            _deadDistance = Vector3.Distance(eventData.pointerCurrentRaycast.worldPosition, _aplhaPos);
            if (_deadDistance < DeadAreaMaxDistance) return;

            _pointerWorldPosition = Content.position;
            _pointerWorldPosition.y = (Content.position.y + (eventData.pointerCurrentRaycast.worldPosition.y - _aplhaPos.y)) * Velocity;
            Content.position = _pointerWorldPosition;
            _onContentDrag.Invoke(this);
            UpdateBarPercentage();

            _aplhaPos = eventData.pointerCurrentRaycast.worldPosition;
        }

        protected override void DragEndAction(PointerEventData eventData)
        {
            base.DragEndAction(eventData);

            if (_deadDistance < DeadAreaMaxDistance)
            {
                PYButton button = eventData.pointerCurrentRaycast.gameObject.GetComponent<PYButton>();
                if (button != null)
                    button.onClick.Invoke(button);
            }
        }

        public void UpdateContentPosition(float percentage)
        {
            _pointerWorldPosition = Content.position;
            _pointerWorldPosition.y = CalculatePosition(percentage, StartPosition, StartPosition - MaxDistance);
            if (!float.IsNaN(_pointerWorldPosition.y))
                Content.position = _pointerWorldPosition;
            UpdateBarPercentage();
        }

        public void UpdateContentPositionAnimated(float percentage)
        {
            _animationPercentageToGo = percentage;
            _animationTimer = 0;
            StartCoroutine(RoutineUpdateContentAnimated());
        }

        private float _animationTimer, _animationPercentageToGo;

        private IEnumerator RoutineUpdateContentAnimated()
        {
            while (_animationTimer < 1)
            {
                UpdateContentPosition(Mathf.Lerp(Percentage, _animationPercentageToGo, _animationTimer));
                _animationTimer += Time.deltaTime;
                yield return null;
            }
        }

        private void UpdateBarPercentage()
        {
            Percentage = CalculatePercentage(Content.position.y, StartPosition, StartPosition - MaxDistance);
        }

        private float CalculatePercentage(float yPos, float min, float max)
        {
            return (100 * (yPos - min)) / (max - min);
        }

        private float CalculatePosition(float percentage, float min, float max)
        {
            return ((percentage * (max - min)) / 100) + min;
        }

    }
}