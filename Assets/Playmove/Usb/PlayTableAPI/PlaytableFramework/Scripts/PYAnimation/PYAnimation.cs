using UnityEngine;
using System;
using UnityEngine.Events;

namespace Playmove
{
    public abstract class PYAnimation : MonoBehaviour
    {
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

        [Header("PYAnimation")]
        public string Name;
        public bool PlayOnStart;
        public bool PlayOnEnable;

        [SerializeField]
        protected bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
        }

        [SerializeField]
        protected bool _isReversing;
        public bool IsReversing
        {
            get { return _isReversing; }
        }

        public float DelayToStart;

        #region Events
        [Serializable]
        public class PYAnimationEvent : UnityEvent<PYAnimation> { }
        [Header("Events")]
        [SerializeField]
        protected PYAnimationEvent _onStart = new PYAnimationEvent();
        public PYAnimationEvent onStart
        {
            get { return _onStart; }
            set { _onStart = value; }
        }

        [SerializeField]
        protected PYAnimationEvent _onComplete = new PYAnimationEvent();
        public PYAnimationEvent onComplete
        {
            get { return _onComplete; }
            set { _onComplete = value; }
        }
        #endregion

        protected float _delay;
        protected Action _callbackPlay;
        protected Action _callbackReverse;

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void Awake() { }
        protected virtual void Start() { }

        public virtual void Play()
        {
            _isPlaying = true;
            _isReversing = false;
            onStart.Invoke(this);
        }
        public virtual void Play(float delayTemp)
        {
            _delay = DelayToStart;
            DelayToStart = delayTemp;
            Play();
        }
        public virtual void Play(Action callBack)
        {
            _callbackPlay = callBack;
            Play();
        }
        public virtual void Play(Action callback, float delayTemp)
        {
            if (delayTemp != 0)
            {

                _delay = DelayToStart;
                DelayToStart = delayTemp;

                _callbackPlay += () => { DelayToStart = _delay; };
            }

            _callbackPlay += callback;

            Play();
        }

        public virtual void Reverse()
        {
            _isPlaying = _isReversing = true;
            onStart.Invoke(this);
        }
        public virtual void Reverse(float delayTemp)
        {
            _delay = DelayToStart;
            DelayToStart = delayTemp;
            Reverse();
        }
        public virtual void Reverse(Action callBack)
        {
            _callbackReverse = callBack;
            Reverse();
        }
        public virtual void Reverse(Action callback, float delayTemp)
        {
            _callbackReverse += callback;

            if (delayTemp != 0)
            {
                _delay = DelayToStart;
                DelayToStart = delayTemp;

                _callbackReverse += () => { DelayToStart = _delay; };
            }

            Reverse();
        }

        public virtual void Stop()
        {
            _isPlaying = false;
            _isReversing = false;
        }

        protected virtual void CompletedAnimation()
        {
            _isPlaying = false;

            if (!IsReversing)
            {
                if (_callbackPlay != null)
                {
                    _callbackPlay();
                    _callbackPlay = null;
                }
            }
            else if (_callbackReverse != null)
            {
                _callbackReverse();
                _callbackReverse = null;
            }

            onComplete.Invoke(this);
        }
    }
}