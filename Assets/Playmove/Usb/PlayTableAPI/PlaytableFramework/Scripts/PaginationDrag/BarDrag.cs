using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace Playmove
{
    /// <summary>
    /// Representa a barra para movimentar o conteudo.
    /// Deve estar dentro da 
    /// </summary>
    public class BarDrag : PYDrag
    {
        public float Velocity = 1;
        [Range(1, 100)]
        public float Size = 1;

        [SerializeField]
        private Transform _holder;
        public Transform Holder { get { return _holder; } }
        public Collider2D HolderCollider { get { return Holder.GetComponentInChildren<Collider2D>(); } }

        [SerializeField]
        [Tooltip("Get collider in parent or manual set")]
        private Collider2D _barLimit;
        public Collider2D BarLimit
        {
            get
            {
                if (!_barLimit)
                {
                    _barLimit = GetComponentInParent<Collider2D>();
                }
                return _barLimit;
            }
        }

        public float Percentage { get; private set; }

        #region Events
        [Serializable]
        public class PYBarDragEvent : UnityEvent<BarDrag> { }

        [SerializeField]
        protected PYBarDragEvent _onBarDrag = new PYBarDragEvent();
        public PYBarDragEvent onBarDrag
        {
            get { return _onBarDrag; }
            set { _onBarDrag = value; }
        }
        #endregion

        private Vector3 _pointerWorldPosition;
        private bool _barAnimating;

        #region Unity

        protected override void Start()
        {
            base.Start();
            UpdateBarPercentage();
        }

        private void Update()
        {
            if (Percentage < 0)
                UpdateBarPosition(0);
            if (Percentage > 100)
                UpdateBarPosition(100);
        }

        #endregion

        #region PYDrag

        protected override void DraggingAction(PointerEventData eventData)
        {
            _pointerWorldPosition = _holder.position;
            _pointerWorldPosition.y = Mathf.Clamp(eventData.pointerCurrentRaycast.worldPosition.y * Velocity, BarLimit.bounds.min.y + HolderCollider.bounds.size.y, BarLimit.bounds.max.y);
            _holder.position = _pointerWorldPosition;
            _onBarDrag.Invoke(this);
            UpdateBarPercentage();
        }

        #endregion

        public void UpdateBarPosition(float percentage)
        {
            _pointerWorldPosition = _holder.position;
            _pointerWorldPosition.y = CalculatePosition(percentage, BarLimit.bounds.min.y + HolderCollider.bounds.size.y, BarLimit.bounds.max.y);
            if (!float.IsInfinity(_pointerWorldPosition.y) && !float.IsNaN(_pointerWorldPosition.y))
                _holder.position = _pointerWorldPosition;
            UpdateBarPercentage();
        }

        public void UpdateBarPositionAnimated(float percentage)
        {
            _animationPercentageToGo = percentage;
            _animationTimer = 0;
            StartCoroutine(routineUpdateBarAnimated());
        }

        public void SetSize(float value, bool animated = true)
        {
            if (animated && gameObject.activeInHierarchy)
            {
                if (!_barAnimating)
                    StartCoroutine(RoutineHolderScale(new Vector3(1, 1 / value, 1)));
            }
            else
                Holder.localScale = new Vector3(1, 1 / value, 1);
        }

        private IEnumerator RoutineHolderScale(Vector3 scaleTo)
        {
            _barAnimating = true;
            float timer = 0;
            Vector3 original = Holder.localScale;
            while (timer < 1)
            {
                Holder.localScale = Vector3.Lerp(original, scaleTo, timer);
                timer += Time.deltaTime * 2f;
                yield return null;
            }

            Holder.localScale = scaleTo;
            _barAnimating = false;
        }

        private float _animationTimer, _animationPercentageToGo;

        private IEnumerator routineUpdateBarAnimated()
        {
            while (_animationTimer < 1)
            {
                UpdateBarPosition(Mathf.Lerp(Percentage, _animationPercentageToGo, _animationTimer));
                _animationTimer += Time.deltaTime;
                yield return null;
            }
        }

        private void UpdateBarPercentage()
        {
            Percentage = CalculatePercentage(_holder.position.y, BarLimit.bounds.min.y + HolderCollider.bounds.size.y, BarLimit.bounds.max.y);
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