using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Playmove
{
    public enum PYButtonState
    {
        Idle,
        Pressed,
        Disabled
    }

    public class PYButton : UIBehaviour,
       IPointerClickHandler, IPointerDownHandler,
       IPointerUpHandler, IPointerEnterHandler,
       IPointerExitHandler, IPYButtonGroup
    {
        [Serializable]
        public class PYButtonEvent : UnityEvent<PYButton> { }

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
        protected PYButtonEvent _onClick = new PYButtonEvent();

        public PYButtonEvent onClick
        {
            get { return _onClick; }
            set { _onClick = value; }
        }

        [SerializeField]
        protected PYButtonEvent _onDown = new PYButtonEvent();

        public PYButtonEvent onDown
        {
            get { return _onDown; }
            set { _onDown = value; }
        }

        [SerializeField]
        protected PYButtonEvent _onUp = new PYButtonEvent();

        public PYButtonEvent onUp
        {
            get { return _onUp; }
            set { _onUp = value; }
        }

        protected PYButtonEvent _onActivated = new PYButtonEvent();

        public PYButtonEvent onActivated
        {
            get { return _onActivated; }
            set { _onActivated = value; }
        }

        protected PYButtonEvent _onDeactivated = new PYButtonEvent();

        public PYButtonEvent onDeactivated
        {
            get { return _onDeactivated; }
            set { _onDeactivated = value; }
        }

        #endregion Events

        [SerializeField]
        protected PYButtonState _state;

        public PYButtonState State
        {
            get { return _state; }
        }

        public bool IsPointerInside { get; protected set; }
        public bool IsPointerDown { get; protected set; }

        public bool IsEnabled
        {
            get
            {
                return _state != PYButtonState.Disabled && Enabled();
            }
            set
            {
                if (value && _state != PYButtonState.Idle)
                {
                    _state = PYButtonState.Idle;
                    EnableAction();
                    SendOnActivated();
                }
                else if (!value && _state != PYButtonState.Disabled)
                {
                    _state = PYButtonState.Disabled;
                    DisableAction();
                    SendOnDeactivated();
                }
            }
        }

        public bool CanBeClicked
        {
            get
            {
                return IsEnabled && _state == PYButtonState.Idle;
            }
        }

        #region Unity Functions

        protected override void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (PYButtonGroupManager.Instance != null)
                PYButtonGroupManager.Instance.EnableGroup(GroupName);
            base.Awake();
        }

        protected override void Start()
        {
            IsEnabled = _state != PYButtonState.Disabled;
        }

        #endregion Unity Functions

        #region Send Events

        private void SendOnActivated()
        {
            _onActivated.Invoke(this);
        }

        private void SendOnDeactivated()
        {
            _onDeactivated.Invoke(this);
        }

        private void SendOnClick()
        {
            if (!IsActive() || !IsEnabled) return;
            _onClick.Invoke(this);
        }

        private void SendOnDown()
        {
            if (!IsActive() || !IsEnabled) return;
            _onDown.Invoke(this);
        }

        private void SendOnUp()
        {
            if (!IsActive() || !IsEnabled) return;
            _onUp.Invoke(this);
        }

        #endregion Send Events

        public virtual void SetContent(object content)
        {
        }

        #region Actions

        protected virtual void EnableAction()
        {
        }

        protected virtual void DisableAction()
        {
        }

        protected virtual void ClickAction()
        {
        }

        protected virtual void DownAction()
        {
        }

        protected virtual void UpAction()
        {
        }

        protected virtual void EnterAction()
        {
        }

        protected virtual void ExitAction()
        {
        }

        #endregion Actions

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled) return;

            ClickAction();
            _state = PYButtonState.Idle;
            SendOnClick();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled) return;

            IsPointerDown = true;
            _state = PYButtonState.Pressed;
            DownAction();
            SendOnDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled) return;

            IsPointerDown = false;
            _state = PYButtonState.Idle;
            UpAction();
            SendOnUp();
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

        [SerializeField]
        private string _groupName = "Default";

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; }
        }

        public bool Enabled()
        {
            if (PYButtonGroupManager.Instance != null)
                return PYButtonGroupManager.Instance.IsEnable(GroupName);
            return true;
        }
    }
}