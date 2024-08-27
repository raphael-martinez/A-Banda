using UnityEngine;
using System.Collections;
using System;

namespace Playmove
{
    public class PYButtonSwitch : PYButton
    {
        [Header("PYButtonSwitch")]
        public PYButtonEvent onSwitch = new PYButtonEvent();

        public PYAnimation SwitchAnimation;
        public bool StartValue;

        [SerializeField]
        private bool _switchValue;
        public bool SwitchValue
        {
            get { return _switchValue; }
            set
            {
                if (_switchValue != value)
                {
                    _switchValue = value;
                    onSwitch.Invoke(this);
                    RunSwitchAnimation();
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            _switchValue = StartValue;
            //RunSwitchAnimation();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ForcePosition();
        }

        protected override void ClickAction()
        {
            base.ClickAction();
            SwitchValue = !SwitchValue;
        }

        private void RunSwitchAnimation()
        {
            if (SwitchValue)
            {
                SwitchAnimation.Stop();
                SwitchAnimation.Play();
            }
            else
            {
                SwitchAnimation.Stop();
                SwitchAnimation.Reverse();
            }
        }

        private void ForcePosition()
        {
            Vector3 finalPos = ((PYTweenAnimation)SwitchAnimation).Position.to;
            if (((PYTweenAnimation)SwitchAnimation).Position.isLocal)
            {
                SwitchAnimation.transform.localPosition = finalPos;
            }
            else
            {
                SwitchAnimation.transform.position = finalPos;
            }

        }
    }
}