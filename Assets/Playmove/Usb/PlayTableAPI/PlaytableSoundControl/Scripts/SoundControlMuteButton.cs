using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Playmove
{
    public class SoundControlMuteButton : PYButton
    {

        public SpriteRenderer icon;
        public Vector3 startScale = Vector3.one;
        public Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1);
        public string imageName = "mute";

        private PYTweenAnimation _animateHover;
        public PYTweenAnimation AnimateHover
        {
            get
            {
                if (!_animateHover)
                    _animateHover = PYTweenAnimation.Add(icon.gameObject)
                        .SetScale(startScale, hoverScale)
                        .SetDuration(0.5f)
                        .SetEaseType(Ease.Type.OutElastic);
                _animateHover.Stop();
                return _animateHover;
            }
        }

        Action callback;
        public void SetCallback(Action callback)
        {
            this.callback = callback;
        }

        protected override void ClickAction()
        {
            if (callback != null)
                callback();
        }

        protected override void UpAction()
        {
            ClickAction();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled)
                return;

            base.OnPointerDown(eventData);
            EventSystem.current.SetSelectedGameObject(OwnGameObject, eventData);
        }

        protected override void EnterAction()
        {
            if (!SoundControlButton.Instance.IsOpen) return;
            AnimateHover.Play();
        }

        protected override void ExitAction()
        {
            if (!SoundControlButton.Instance.IsOpen) return;
            AnimateHover.Reverse();
        }

        public void UpdateSprite(bool active)
        {
            icon.sprite = Resources.Load<Sprite>(imageName + (active ? "Off" : "On"));
        }
    }
}