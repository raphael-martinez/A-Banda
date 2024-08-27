using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PYDragMouse : PYDrag
{
    public bool IsDragging = false;

    public bool UseSmoothDamp = false;
    public float Smooth = 45;
    public DragType DragOnPlane;
    public bool UseLimitArea;
    public Vector2 LimitArea;
    public float DeadArea = 0.5f;
    public Transform TargetToMove;

    public Vector2 Direction
    {
        get;
        private set;
    }
    public bool HasMovedOutsideDeadArea { get; private set; }

    private Transform _target;
    private Vector2 _pointerWorldPosition;
    private Vector2 _deltaPos;
    private bool _insideDeadArea;

    private Vector3 _velocity;
    private Vector3 _targetPosition;
    private Vector3 _lastTargetPosition;

    protected override void Start()
    {
        base.Start();

        _target = TargetToMove == null ? OwnTransform : TargetToMove;
        UseLimitArea = DragOnPlane != DragType.Free && UseLimitArea;

        _lastTargetPosition = _targetPosition = _target.position;
    }

    void Update()
    {
        if (!UseSmoothDamp || !IsDragging) return;

        UpdateDrag(Vector3.SmoothDamp(_target.position, _targetPosition, ref _velocity, Time.deltaTime * Smooth * 0.1f, Smooth));
    }

    public void MoveBy(float moveAmount, float duration)
    {
        StartCoroutine(MoveByRoutine(moveAmount, duration));
    }
    IEnumerator MoveByRoutine(float moveAmount, float duration)
    {
        IsDragging = true;
        Vector3 initialPos = _target.position;
        Vector3 targetPos = Vector3.zero;

        if (DragOnPlane == DragType.Horizontal)
            targetPos = initialPos + Vector3.right * moveAmount;
        else if (DragOnPlane == DragType.Vertical)
            targetPos = initialPos + Vector3.up * moveAmount;

        float timer = 0;
        while (timer < 1)
        {
            _targetPosition = Vector3.Lerp(initialPos, targetPos, timer / duration);
            UpdateDrag(_targetPosition);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        IsDragging = false;
    }
    
    protected override void DragBeginAction(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == null)
            return;

        _insideDeadArea = true;
        IsDragging = true;
        HasMovedOutsideDeadArea = false;

        _pointerWorldPosition = eventData.pointerCurrentRaycast.worldPosition;
        _deltaPos = new Vector2(_target.position.x - _pointerWorldPosition.x, _target.position.y - _pointerWorldPosition.y);

        _targetPosition = _pointerWorldPosition + _deltaPos;

        if (!UseSmoothDamp)
            UpdateDrag(_pointerWorldPosition + _deltaPos);
    }

    protected override void DraggingAction(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == null)
            return;

        if (_insideDeadArea)
        {
            //Vector3 dragDeadArea = (Vector3)_pointerWorldPosition - eventData.pointerCurrentRaycast.worldPosition;
            if (DragOnPlane == DragType.Horizontal)
            {
                if (Mathf.Abs(eventData.delta.x) < DeadArea)
                    return;
            }

            if (DragOnPlane == DragType.Vertical)
            {
                if (Mathf.Abs(eventData.delta.y) < DeadArea)
                    return;
            }

            HasMovedOutsideDeadArea = true;
            _insideDeadArea = false;
        }

        _pointerWorldPosition = eventData.pointerCurrentRaycast.worldPosition;

        _targetPosition = _pointerWorldPosition + _deltaPos;

        if (!UseSmoothDamp)
            UpdateDrag(_pointerWorldPosition + _deltaPos);
    }

    protected override void DragEndAction(PointerEventData eventData)
    {
        IsDragging = false;
        base.DragEndAction(eventData);
    }

    private void UpdateDrag(Vector3 newPosition)
    {
        switch (DragOnPlane)
        {
            case DragType.Free:
                _target.position = newPosition;
            break;

            case DragType.Horizontal:
                _target.position = OutsideLimitArea(new Vector3(newPosition.x, _target.position.y, _target.position.z));
            break;

            case DragType.Vertical:
                _target.position = OutsideLimitArea(new Vector3(_target.position.x, newPosition.y, _target.position.z));
            break;
        }

        Direction = _target.position - _lastTargetPosition;
        Direction.Normalize();

        _lastTargetPosition = _target.position;
    }

    private Vector3 OutsideLimitArea(Vector3 dragPosition)
    {
        if (!UseLimitArea || DragOnPlane == DragType.Free)
            return dragPosition;
        
        if (DragOnPlane == DragType.Horizontal)
        {
            if (dragPosition.x < LimitArea.x)
                dragPosition.x = LimitArea.x;
            else if (dragPosition.x > LimitArea.y)
                dragPosition.x = LimitArea.y;
        }

        if (DragOnPlane == DragType.Vertical)
        {
            if (dragPosition.y < LimitArea.x)
                dragPosition.y = LimitArea.x;
            else if (dragPosition.y > LimitArea.y)
                dragPosition.y = LimitArea.y;
        }

        return dragPosition;
    }
}
