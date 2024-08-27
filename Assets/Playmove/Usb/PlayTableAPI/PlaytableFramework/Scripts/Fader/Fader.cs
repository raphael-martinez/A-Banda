using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Playmove;

namespace Old { public class Fader : MonoBehaviour
    {
        public event Action OnFaderClick;

        #region props

        private Fader _myFader;
        public Fader MyFader
        {
            get
            {
                if (_myFader == null)
                    _myFader = GetComponent<Fader>();
                return _myFader;
            }
        }

        [SerializeField]
        private bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
            private set { _isOpen = value; }
        }


        public PYTweenAnimation FadeAnimation { get; set; }

        public SpriteRenderer FadeSpriteRenderer { get; set; }

        public float ZPosition { get; set; }
        #endregion

        private bool _setInCamera;
        private PYButton _button;

        #region Unity
        private void Start()
        {
            _button = GetComponent<PYButton>();
            _button.onClick.AddListener(Button_OnReleasedOver);
        }
        #endregion

        public void FadeIn(float duration = 1, float from = 0, float to = 0.5f, float delay = 0, Action callback = null)
        {
            OpenState();
            Fade(duration, from, to, delay, callback);
        }

        public void FadeOut(float duration = 1, float from = 1, float to = 0, float delay = 0, Action callback = null)
        {
            callback += CloseState;
            Fade(duration, from, to, delay, callback);
        }

        public void Fade(float duration, float from, float to, float delay = 0, Action callback = null)
        {
            FadeAnimation.Stop();
            FadeAnimation.SetDuration(duration)
                .SetDelay(delay)
                .SetEaseType(Ease.Type.Linear)
                .SetAlpha(from, to);
            FadeAnimation.Play(() =>
            {
                if (callback != null)
                {
                    callback();
                    callback = null;
                }
            });
        }

        public void ResetPosition(bool onCamera = false)
        {
            _setInCamera = onCamera;
            OpenCameraState(_setInCamera);
        }

        internal void Initialize(bool isOpen = false)
        {
            MyFader.IsOpen = IsOpen;
        }

        private void OpenState()
        {
            MyFader.GetComponent<Renderer>().enabled = true;

            // Gets collider 2D
            if (MyFader.GetComponent<Collider2D>())
                MyFader.GetComponent<Collider2D>().enabled = true;
            // Or try to get collider 3D
            else if (MyFader.GetComponent<Collider>())
                MyFader.GetComponent<Collider>().enabled = true;

            IsOpen = true;
        }

        private void CloseState()
        {
            FadeSpriteRenderer.color = Color.clear;
            MyFader.GetComponent<Renderer>().enabled = false;

            // Gets collider 2D
            if (MyFader.GetComponent<Collider2D>())
                MyFader.GetComponent<Collider2D>().enabled = false;
            // Or try to get collider 3D
            else if (MyFader.GetComponent<Collider>())
                MyFader.GetComponent<Collider>().enabled = false;

            IsOpen = false;
        }

        private void OpenCameraState(bool setInCamera)
        {
            _setInCamera = setInCamera;
            if (setInCamera)
            {
                MyFader.transform.localPosition = Vector3.forward * TagManager.BLACK_FADER_IN_CAMERA;
                FadeSpriteRenderer.sortingLayerName = TagManager.SortingLayerNames.GUI.ToString();
                FadeSpriteRenderer.sortingOrder = 999;
            }
            else
            {
                MyFader.transform.localPosition = Vector3.forward * TagManager.BLACK_FADER_IN_GAME;
                FadeSpriteRenderer.sortingLayerName = TagManager.SortingLayerNames.GUI.ToString();
                FadeSpriteRenderer.sortingOrder = 0;
            }
        }

        private void Button_OnReleasedOver(PYButton sender)
        {
            if (OnFaderClick != null) OnFaderClick();
        }
    }
}