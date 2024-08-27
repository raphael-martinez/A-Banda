using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Playmove
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class AbstractContentDrag : PYDrag
    {
        public float Velocity = 1;
        public bool UseSwipe = true;

        [Header("Buttons")]
        public PYButton ArrowUp;
        public PYButton ArrowDown;
        public PYTweenAnimation ButtonsAnimation;
        [Range(0, 100)]
        public int ButtonPercentagePerPage = 25;
        public float PercentagePerTime = 1,
            IteragionPerSecond = 0.05f,
            ArrowHoldTime = 0.5f;

        #region Props

        protected abstract Transform ContentTransform { get; }
        protected abstract float UpLimit { get; }
        protected abstract float DownLimit { get; }

        private Collider2D _myCollider2D;
        private Collider2D MyCollider2D
        {
            get
            {
                if (!_myCollider2D) _myCollider2D = GetComponent<Collider2D>();
                return _myCollider2D;
            }
        }

        private BarDrag _barDrag;
        public BarDrag BarDrag
        {
            get
            {
                if (!_barDrag)
                    _barDrag = OwnTransform.parent.GetComponentInChildren<BarDrag>();
                return _barDrag;
            }
        }

        private BarSwype _barSwype;
        private BarSwype BarSwype
        {
            get
            {
                if (!_barSwype)
                {
                    _barSwype = GetComponent<BarSwype>();
                    if (!_barSwype && UseSwipe)
                    {
                        _barSwype = OwnGameObject.AddComponent<BarSwype>();
                        _barSwype.SwipeDistance = 1;
                        _barSwype.SwipeDuration = 0.5f;
                    }
                }
                return _barSwype;
            }
        }

        private Collider2D[] _buttonsColliders;
        private Collider2D[] ButtonsColliders
        {
            get
            {
                if (_buttonsColliders == null) _buttonsColliders = ButtonsAnimation.GetComponentsInChildren<Collider2D>();
                return _buttonsColliders;
            }
        }

        public float Percentage { get; private set; }

        #endregion

        #region Events
        [Serializable]
        public class PYContentDragEvent : UnityEvent<AbstractContentDrag> { }

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
        private float _upLimitFixed = 0;
        private float _animationTimer, _animationPercentageToGo;
        private bool _arrowHolded;

        protected override void Start()
        {
            Initialize();
            base.Start();
        }

        private void Update()
        {
            if (Percentage < 0)
                UpdateContentPosition(0);
            if (Percentage > 100)
                UpdateContentPosition(100);
        }

        protected override void DragBeginAction(PointerEventData eventData)
        {
            FixUpLimit();

            base.DragBeginAction(eventData);
            _aplhaPos = eventData.pointerCurrentRaycast.worldPosition;
        }

        protected override void DraggingAction(PointerEventData eventData)
        {
            _pointerWorldPosition = ContentTransform.position;
            _pointerWorldPosition.y = (ContentTransform.position.y + (eventData.pointerCurrentRaycast.worldPosition.y - _aplhaPos.y)) * Velocity;
            ContentTransform.position = _pointerWorldPosition;

            _onContentDrag.Invoke(this);
            UpdateBarPercentage();
            if (BarDrag != null)
                BarDrag.UpdateBarPosition(100 - Percentage);

            _aplhaPos = eventData.pointerCurrentRaycast.worldPosition;
        }

        private void UpdateContent(BarDrag data)
        {
            FixUpLimit();

            UpdateContentPosition(100 - data.Percentage);
        }

        private void FixUpLimit()
        {
            if (_upLimitFixed == 0)
                _upLimitFixed = UpLimit;
        }

        public void Initialize()
        {
            /// NOTE: Desabilitar todo recurso de paginação caso o conteudo for pequeno
            if (DownLimit > MyCollider2D.bounds.min.y)
            {
                BarDrag.enabled = false;
                enabled = false;
            }
            else
            {
                BarDrag.enabled = true;
                enabled = true;

                ArrowUp.onClick.AddListener((o) => ArrowUpClick());
                ArrowUp.onDown.AddListener((o) => ArrowUpStart());
                ArrowUp.onUp.AddListener((o) => ArrowUpStop());

                ArrowDown.onClick.AddListener((o) => ArrowDownClick());
                ArrowDown.onDown.AddListener((o) => ArrowDownStart());
                ArrowDown.onUp.AddListener((o) => ArrowDownStop());

                ButtonsAnimation.Play();
                foreach (Collider2D col in ButtonsColliders)
                    col.enabled = true;

                if (BarSwype != null)
                    BarSwype.onSwype.AddListener(ContentSwyped);

                if (BarDrag != null)
                    BarDrag.onBarDrag.AddListener(UpdateContent);
            }
        }

        public void UpdateContentPosition(float percentage)
        {
            _pointerWorldPosition = ContentTransform.position;
            _pointerWorldPosition.y = CalculatePosition(percentage, _upLimitFixed, UpLimit - DownLimit);
            ContentTransform.position = _pointerWorldPosition;
            UpdateBarPercentage();
        }

        public void UpdateContentPositionAnimated(float percentage)
        {
            _animationPercentageToGo = percentage;
            _animationTimer = 0;
            StartCoroutine("RoutineUpdateContentAnimated");
        }

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
            Percentage = CalculatePercentage(ContentTransform.position.y, _upLimitFixed, UpLimit - DownLimit);
        }

        private float CalculatePercentage(float yPos, float min, float max)
        {
            return (100 * (yPos - min)) / (max - min);
        }

        private float CalculatePosition(float percentage, float min, float max)
        {
            return ((percentage * (max - min)) / 100) + min;
        }

        #region Inspector Control

        private void ContentSwyped(BarSwype swype)
        {
            if (swype.Direction > 0)
                UpPosition();
            else
                DownPosistion();
        }

        public void UpPosition()
        {
            UpdateContentPositionAnimated(0);
            BarDrag.UpdateBarPositionAnimated(100);
        }

        public void DownPosistion()
        {
            UpdateContentPositionAnimated(100);
            BarDrag.UpdateBarPositionAnimated(0);
        }

        public void ArrowDownClick()
        {
            FixUpLimit();

            if (_arrowHolded) return;

            if (Percentage < 100 - ButtonPercentagePerPage)
                UpdateContentPositionAnimated(Percentage + ButtonPercentagePerPage);
            else
                UpdateContentPositionAnimated(100);

            if (BarDrag.Percentage > ButtonPercentagePerPage)
                BarDrag.UpdateBarPositionAnimated(BarDrag.Percentage - ButtonPercentagePerPage);
            else
                BarDrag.UpdateBarPositionAnimated(0);
        }

        public void ArrowUpClick()
        {
            FixUpLimit();

            if (_arrowHolded) return;

            if (Percentage > ButtonPercentagePerPage)
                UpdateContentPositionAnimated(Percentage - ButtonPercentagePerPage);
            else
                UpdateContentPositionAnimated(0);

            if (BarDrag.Percentage < 100 - ButtonPercentagePerPage)
                BarDrag.UpdateBarPositionAnimated(BarDrag.Percentage + ButtonPercentagePerPage);
            else
                BarDrag.UpdateBarPositionAnimated(100);
        }

        public void ArrowUpStart()
        {
            StartCoroutine("RoutineArrowUp");
        }

        public void ArrowUpStop()
        {
            StopCoroutine("RoutineArrowUp");
        }

        public void ArrowDownStart()
        {
            StartCoroutine("RoutineArrowDown");
        }

        public void ArrowDownStop()
        {
            StopCoroutine("RoutineArrowDown");
        }

        private IEnumerator RoutineArrowDown()
        {
            FixUpLimit();

            _arrowHolded = false;

            yield return new WaitForSeconds(ArrowHoldTime);

            _arrowHolded = true;

            while (true)
            {
                if (Percentage < 100)
                    UpdateContentPosition(Percentage + PercentagePerTime);
                if (BarDrag.Percentage > 0)
                    BarDrag.UpdateBarPosition(BarDrag.Percentage - PercentagePerTime);

                yield return new WaitForSeconds(IteragionPerSecond);
            }
        }

        private IEnumerator RoutineArrowUp()
        {
            FixUpLimit();

            _arrowHolded = false;

            yield return new WaitForSeconds(ArrowHoldTime);

            _arrowHolded = true;

            while (true)
            {
                if (Percentage > 0)
                    UpdateContentPosition(Percentage - PercentagePerTime);
                if (BarDrag.Percentage < 100)
                    BarDrag.UpdateBarPosition(BarDrag.Percentage + PercentagePerTime);

                yield return new WaitForSeconds(IteragionPerSecond);
            }
        }


        #endregion
    }
}