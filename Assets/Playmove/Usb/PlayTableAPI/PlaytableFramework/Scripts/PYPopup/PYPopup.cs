using System;
using UnityEngine;

namespace Playmove
{
    public class PYPopup : PYOpenable
    {
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

        [Serializable]
        public struct AnimationData
        {
            public PYAnimation Animation;
            public string Tag;
        }

        public bool IsOpen { get { return (State == OpenableState.Opened || State == OpenableState.Opening); } }

        [Header("PYPopup")]
        public AnimationData EnterAnimation;

        public AnimationData ExitAnimation;

        [Tooltip("Se vai fazer a versão reversa da animação de entrada no lugar da animação de saida")]
        protected bool useEnterAnimationAsExit = false;

        [Tooltip("Usará Fader")]
        public bool UseFader;

        [Tooltip("Fechar Popup quando clicar no Fader")]
        public bool ClosePopupByFader;

        [Tooltip("Fechar Fader quando fechar a Popup")]
        public bool CloseFaderByPopup = true;

        private bool _calledOpenBeforeStart = false;

        protected virtual void Start()
        {
            useEnterAnimationAsExit = EnterAnimation.Animation != null && ExitAnimation.Animation == null;

            if (UseFader && ClosePopupByFader)
                FaderManager.GameFade.OnFaderClick += Close;
            if (!_calledOpenBeforeStart)
            {
                if (!IsOpen)
                    OwnGameObject.SetActive(false);
                else
                {
                    Opened();
                    if (UseFader)
                        FaderManager.GameFade.Initialize();
                }
            }
        }

        public override void Open()
        {
            if (State != OpenableState.Closed) return;
            _calledOpenBeforeStart = true;

            base.Open();
            OpenAnimation();
        }

        public virtual void Open(float timeOpened)
        {
            if (State != OpenableState.Closed) return;

            Open();
            Invoke("Close", timeOpened);
        }

        public virtual void Open(Action callback, float timeOpened)
        {
            if (State != OpenableState.Closed) return;

            Open(callback);
            Invoke("Close", timeOpened);
        }

        public override void HardOpen()
        {
            base.HardOpen();
            OpenAnimation();
        }

        public void OpenWithoutAnimation()
        {
            base.HardOpen();
            Opened();
        }

        public override void Close()
        {
            if (State != OpenableState.Opened) return;

            base.Close();
            CloseAnimation();
        }

        public virtual void Close(float timeToClose)
        {
            if (State != OpenableState.Opened) return;

            Invoke("Close", timeToClose);
        }

        public virtual void Close(Action callback, float timeToClose)
        {
            if (State != OpenableState.Closed) return;

            _callback = callback;
            Invoke("Close", timeToClose);
        }

        public override void HardClose()
        {
            base.HardClose();
            CloseAnimation();
        }

        public void CloseWithoutAnimation()
        {
            base.HardClose();
            Closed();
        }

        protected virtual void OpenAnimation()
        {
            OwnGameObject.SetActive(true);

            if (UseFader)
                FaderManager.FadeInGame();

            if (EnterAnimation.Animation != null)
            {
                if (EnterAnimation.Animation is PYAnimator)
                    ((PYAnimator)EnterAnimation.Animation).Play(EnterAnimation.Tag, Opened);
                else
                    EnterAnimation.Animation.Play(Opened);
            }
            else
            {
                OwnGameObject.SetActive(true);
                Opened();
            }
        }

        protected virtual void CloseAnimation()
        {
            if (CloseFaderByPopup)
                FaderManager.FadeOutGame();

            if (useEnterAnimationAsExit)
            {
                if (EnterAnimation.Animation is PYAnimator)
                    ((PYAnimator)EnterAnimation.Animation).Reverse(EnterAnimation.Tag, Closed);
                else
                    EnterAnimation.Animation.Reverse(Closed);
            }
            else if (ExitAnimation.Animation != null)
            {
                if (ExitAnimation.Animation is PYAnimator)
                    ((PYAnimator)ExitAnimation.Animation).Play(ExitAnimation.Tag, Closed);
                else
                    ExitAnimation.Animation.Play(Closed);
            }
            else
            {
                if (DeactiveOnClosed)
                    OwnGameObject.SetActive(false);
                Closed();
            }
        }
    }
}