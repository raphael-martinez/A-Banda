using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Playmove
{
    public class PYSwipe : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Serializable]
        public class PYSwipeEvent : UnityEvent<PYSwipe> { }

        #region Enums

        public enum SwipeState
        {
            Idle,
            Dragging,
            Disabled
        }

        public enum SwipeType
        {
            Horizontal,
            Vertical,
            Free
        }

        public enum SwipeDirection
        {
            Up,
            Down,
            Left,
            Right,
            Free
        }

        #endregion

        #region Events

        [SerializeField]
        protected PYSwipeEvent _onSwipe = new PYSwipeEvent();
        public PYSwipeEvent onSwipe
        {
            get { return _onSwipe; }
            set { _onSwipe = value; }
        }

        [SerializeField]
        protected PYSwipeEvent _onHorizontalSwipe = new PYSwipeEvent();
        public PYSwipeEvent onHorizontalSwipe
        {
            get { return _onHorizontalSwipe; }
            set { _onHorizontalSwipe = value; }
        }

        [SerializeField]
        protected PYSwipeEvent _onHorizontalSwipeLeft = new PYSwipeEvent();
        public PYSwipeEvent onHorizontalSwipeLeft
        {
            get { return _onHorizontalSwipeLeft; }
            set { _onHorizontalSwipeLeft = value; }
        }

        [SerializeField]
        protected PYSwipeEvent _onHorizontalSwipeRight = new PYSwipeEvent();
        public PYSwipeEvent onHorizontalSwipeRight
        {
            get { return _onHorizontalSwipeRight; }
            set { _onHorizontalSwipeRight = value; }
        }

        [SerializeField]
        protected PYSwipeEvent _onVerticalSwipe = new PYSwipeEvent();
        public PYSwipeEvent onVerticalSwipe
        {
            get { return _onVerticalSwipe; }
            set { _onVerticalSwipe = value; }
        }

        [SerializeField]
        protected PYSwipeEvent _onVerticalSwipeUp = new PYSwipeEvent();
        public PYSwipeEvent onVerticalSwipeUp
        {
            get { return _onVerticalSwipeUp; }
            set { _onVerticalSwipeUp = value; }
        }

        [SerializeField]
        protected PYSwipeEvent _onVerticalSwipeDown = new PYSwipeEvent();
        public PYSwipeEvent onVerticalSwipeDown
        {
            get { return _onVerticalSwipeDown; }
            set { _onVerticalSwipeDown = value; }
        }

        [SerializeField]
        protected PYSwipeEvent _onActivated = new PYSwipeEvent();
        public PYSwipeEvent onActivated
        {
            get { return _onActivated; }
            set { _onActivated = value; }
        }

        [SerializeField]
        protected PYSwipeEvent _onDeactivated = new PYSwipeEvent();
        public PYSwipeEvent onDeactivated
        {
            get { return _onDeactivated; }
            set { _onDeactivated = value; }
        }
        #endregion

        #region Properties

        [SerializeField]
        protected SwipeState _state;
        public SwipeState State { get { return _state; } }

        private GameObject _ownGameObject;
        public GameObject OwnGameObject
        {
            get
            {
                if (_ownGameObject == null)
                    _ownGameObject = gameObject;
                return _ownGameObject;
            }
        }

        private Transform _ownTransform;
        public Transform OwnTransform
        {
            get
            {
                if (_ownTransform == null)
                    _ownTransform = transform;
                return _ownTransform;
            }
        }

        public bool IsPointerInside { get; protected set; }
        public bool IsPointerDown { get; protected set; }
        public bool IsEnabled
        {
            get { return _state != SwipeState.Disabled; }
            set
            {
                if (value)
                {
                    _state = SwipeState.Idle;
                    EnableAction();
                    SendOnActivated();
                }
                else
                {
                    _state = SwipeState.Disabled;
                    DisableAction();
                    SendOnDeactivated();
                }
            }
        }
        public bool CanBeClicked
        {
            get
            {
                return IsEnabled && _state == SwipeState.Idle;
            }
        }

        #endregion

        #region Unity Functions

        protected override void Start()
        {
            IsEnabled = _state != SwipeState.Disabled;
        }

        #endregion

        #region Send Events

        private void SendOnActivated()
        {
            _onActivated.Invoke(this);
        }

        private void SendOnDeactivated()
        {
            _onDeactivated.Invoke(this);
        }

        private void SendOnSwipeCompleted(SwipeType type, SwipeDirection direction)
        {
            if (!IsActive() || !IsEnabled)
                return;

            _onSwipe.Invoke(this);

            if (type == SwipeType.Vertical)
            {
                _onVerticalSwipe.Invoke(this);

                if (direction == SwipeDirection.Up)
                    _onVerticalSwipeUp.Invoke(this);

                if (direction == SwipeDirection.Down)
                    _onVerticalSwipeDown.Invoke(this);
            }

            if (type == SwipeType.Horizontal)
            {
                _onHorizontalSwipe.Invoke(this);

                if (direction == SwipeDirection.Left)
                    _onHorizontalSwipeLeft.Invoke(this);

                if (direction == SwipeDirection.Right)
                    _onHorizontalSwipeRight.Invoke(this);
            }
        }

        #endregion

        #region Actions
        protected virtual void EnableAction() { }
        protected virtual void DisableAction() { }
        protected virtual void EnterAction() { }
        protected virtual void ExitAction() { }
        #endregion

        public float SwipeMaxDuration = 0.25f;
        public float SwipeDeadArea = 0.25f;

        private Vector2 _initialSwipePoint;
        private Vector2 _finalSwipePoint;
        private Vector2 _diffPoint;
        private float _initialSwipeTime;
        private float _finalSwipeTime;
        private SwipeType _type;
        private SwipeDirection _direction;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled) return;

            _initialSwipePoint = eventData.pointerCurrentRaycast.worldPosition;
            _initialSwipeTime = Time.time;
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            _finalSwipeTime = Time.time;

            if (Mathf.Abs(_finalSwipeTime - _initialSwipeTime) > SwipeMaxDuration)
                return;

            _finalSwipePoint = eventData.pointerCurrentRaycast.worldPosition;
            _diffPoint = _finalSwipePoint - _initialSwipePoint;
            _diffPoint = new Vector2(Mathf.Abs(_diffPoint.x), Mathf.Abs(_diffPoint.y));

            if (_diffPoint.x <= SwipeDeadArea && _diffPoint.y <= SwipeDeadArea)
                return;

            _type = SwipeType.Free;
            _direction = SwipeDirection.Free;

            if (_diffPoint.x > _diffPoint.y)
            {
                _type = SwipeType.Horizontal;
                _direction = _finalSwipePoint.x > _initialSwipePoint.x ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                _type = SwipeType.Vertical;
                _direction = _finalSwipePoint.y > _initialSwipePoint.y ? SwipeDirection.Up : SwipeDirection.Down;
            }

            SendOnSwipeCompleted(_type, _direction);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled) return;

            IsPointerInside = true;
            EnterAction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled) return;

            IsPointerInside = false;
            ExitAction();
        }
    }
}