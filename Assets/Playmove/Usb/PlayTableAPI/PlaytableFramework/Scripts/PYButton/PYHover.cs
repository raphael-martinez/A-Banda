using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum PYHoverState
{
    Idle,
    Over,
    Disabled
}

namespace Playmove
{
    public class PYHover : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public class PYHoverEvent : UnityEvent<PYHover> { }

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

        #region Events
        [SerializeField]
        protected PYHoverEvent _onEnter = new PYHoverEvent();
        public PYHoverEvent onEnter
        {
            get { return _onEnter; }
            set { _onEnter = value; }
        }

        [SerializeField]
        protected PYHoverEvent _onExit = new PYHoverEvent();
        public PYHoverEvent onExit
        {
            get { return _onExit; }
            set { _onExit = value; }
        }

        protected PYHoverEvent _onActivated = new PYHoverEvent();
        public PYHoverEvent onActivated
        {
            get { return _onActivated; }
            set { _onActivated = value; }
        }

        protected PYHoverEvent _onDeactivated = new PYHoverEvent();
        public PYHoverEvent onDeactivated
        {
            get { return _onDeactivated; }
            set { _onDeactivated = value; }
        }
        #endregion

        [SerializeField]
        protected PYHoverState _state;
        public PYHoverState State
        {
            get { return _state; }
        }

        public bool IsPointerInside { get; protected set; }

        public bool IsEnabled
        {
            get { return _state != PYHoverState.Disabled; }
            set
            {
                if (value)
                {
                    _state = PYHoverState.Idle;
                    EnableAction();
                    SendOnActivated();
                }
                else
                {
                    _state = PYHoverState.Disabled;
                    DisableAction();
                    SendOnDeactivated();
                }
            }
        }
        public bool CanBeClicked
        {
            get
            {
                return IsEnabled && _state == PYHoverState.Idle;
            }
        }

        #region Unity Functions
        protected override void Start()
        {
            IsEnabled = _state != PYHoverState.Disabled;
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

        private void SendOnEnter()
        {
            if (!IsActive() || !IsEnabled) return;
            _onEnter.Invoke(this);
        }

        private void SendOnExit()
        {
            if (!IsActive() || !IsEnabled) return;
            _onExit.Invoke(this);
        }
        #endregion

        public virtual void SetContent(object content) { }

        #region Actions
        protected virtual void EnableAction() { }
        protected virtual void DisableAction() { }
        protected virtual void EnterAction() { }
        protected virtual void ExitAction() { }
        #endregion

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled) return;

            IsPointerInside = true;
            EnterAction();
            _state = PYHoverState.Over;
            SendOnEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled) return;

            IsPointerInside = false;
            ExitAction();
            _state = PYHoverState.Idle;
            SendOnExit();
        }
    }
}