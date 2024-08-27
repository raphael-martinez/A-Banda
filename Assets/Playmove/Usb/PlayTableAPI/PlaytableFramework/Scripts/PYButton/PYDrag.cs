using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class PYDrag : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum DragState
    {
        Idle,
        Dragging,
        Disabled
    }

    public enum DragType
    {
        Horizontal,
        Vertical,
        Free
    }

    [Serializable]
    public class PYDragEvent : UnityEvent<PYDrag> { }

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
    protected PYDragEvent _onDragBegin = new PYDragEvent();
    public PYDragEvent onDragBegin
    {
        get { return _onDragBegin; }
        set { _onDragBegin = value; }
    }

    [SerializeField]
    protected PYDragEvent _onDragging = new PYDragEvent();
    public PYDragEvent onDragging
    {
        get { return _onDragging; }
        set { _onDragging = value; }
    }

    [SerializeField]
    protected PYDragEvent _onDragEnd = new PYDragEvent();
    public PYDragEvent onDragEnd
    {
        get { return _onDragEnd; }
        set { _onDragEnd = value; }
    }

    [SerializeField]
    protected PYDragEvent _onActivated = new PYDragEvent();
    public PYDragEvent onActivated
    {
        get { return _onActivated; }
        set { _onActivated = value; }
    }

    [SerializeField]
    protected PYDragEvent _onDeactivated = new PYDragEvent();
    public PYDragEvent onDeactivated
    {
        get { return _onDeactivated; }
        set { _onDeactivated = value; }
    }
    #endregion

    [SerializeField]
    protected DragState _state;
    public DragState State
    {
        get { return _state; }
    }

    public GameObject OnEndDragObject { get; private set; }
    public GameObject OnBeginDragObject { get; private set; }

    public bool IsPointerInside { get; protected set; }
    public bool IsPointerDown { get; protected set; }


    public bool IsEnabled
    {
        get { return _state != DragState.Disabled; }
        set
        {
            if (value)
            {
                _state = DragState.Idle;
                EnableAction();
                SendOnActivated();
            }
            else
            {
                _state = DragState.Disabled;
                DisableAction();
                SendOnDeactivated();
            }
        }
    }

    public bool CanBeClicked
    {
        get
        {
            return IsEnabled && _state == DragState.Idle;
        }
    }

    #region Unity Functions
    protected override void Start()
    {
        IsEnabled = _state != DragState.Disabled;
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

    private void SendOnDragBegin()
    {
        if (!IsActive() || !IsEnabled) return;
        _onDragBegin.Invoke(this);
    }

    private void SendOnDragging()
    {
        if (!IsActive() || !IsEnabled) return;
        _onDragging.Invoke(this);
    }

    private void SendOnDragEnd()
    {
        if (!IsActive() || !IsEnabled) return;
        _onDragEnd.Invoke(this);
    }
    #endregion

    public virtual void SetContent(object content) { }

    #region Actions
    protected virtual void EnableAction() { }
    protected virtual void DisableAction() { }
    protected virtual void DragBeginAction(PointerEventData eventData) { }
    protected virtual void DraggingAction(PointerEventData eventData) { }
    protected virtual void DragEndAction(PointerEventData eventData) { }
    protected virtual void EnterAction() { }
    protected virtual void ExitAction() { }
    #endregion

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsEnabled) return;

        OnBeginDragObject = eventData.pointerCurrentRaycast.gameObject;

        DragBeginAction(eventData);
        _state = DragState.Dragging;
        SendOnDragBegin();
    }

    public virtual void OnDrag(PointerEventData eventData)
    {       
        if (!IsActive() || !IsEnabled) return;

        IsPointerDown = true;

        DraggingAction(eventData);
        _state = DragState.Dragging;
        SendOnDragging();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsActive() || !IsEnabled) return;

        IsPointerDown = false;
        OnEndDragObject = eventData.pointerCurrentRaycast.gameObject;

        DragEndAction(eventData);
        _state = DragState.Idle;
        SendOnDragEnd();
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
