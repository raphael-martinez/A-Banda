using Playmove.Core.API;
using Playmove.Core.BasicEvents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Playmove.Core.Controls
{
    public class ControlSlider : Openable
    {
        private const string TRIGGER_OPEN = "Open";
        private const string TRIGGER_CLOSE = "Close";

        public PlaytableEventFloat OnValueChanged = new PlaytableEventFloat();

        [SerializeField] Slider _slider = null;
        [SerializeField] Button _buttonMin = null;
        [SerializeField] Button _buttonMax = null;

        private Animator _animator;
        private Animator Animator
        {
            get
            {
                if (_animator == null)
                    _animator = GetComponent<Animator>();
                return _animator;
            }
        }

        private bool _triedToCloseWhileOpening = false;
        private bool _triedToOpenWhileClosing = false;

        private void Awake()
        {
            _slider.onValueChanged.AddListener(OnChangedSlider);
            _buttonMin.onClick.AddListener(OnClickMin);
            _buttonMax.onClick.AddListener(OnClickMax);
        }

        public void UpdateGraphics(float value)
        {
            Slider.SliderEvent events = _slider.onValueChanged;
            //_slider.onValueChanged = new Slider.SliderEvent();
            _slider.value = value;
            _slider.onValueChanged = events;
        }

        public override void Open()
        {
            if (State == OpenableState.Closing) _triedToOpenWhileClosing = true;
            if (State != OpenableState.Closed) return;

            gameObject.SetActive(true);
            TriggerAnimation(TRIGGER_OPEN);
            base.Open();
        }
        protected override void Opened()
        {
            base.Opened();

            if (_triedToCloseWhileOpening)
            {
                _triedToCloseWhileOpening = false;
                Close();
            }
        }

        public override void Close()
        {
            if (State == OpenableState.Opening) _triedToCloseWhileOpening = true;
            if (State != OpenableState.Opened) return;

            TriggerAnimation(TRIGGER_CLOSE);
            base.Close();
        }
        protected override void Closed()
        {
            gameObject.SetActive(false);
            base.Closed();

            if (_triedToOpenWhileClosing)
            {
                _triedToOpenWhileClosing = false;
                Open();
            }
        }

        private void OnChangedSlider(float value)
        {
            OnValueChanged.Invoke(value);
        }

        private void OnClickMin()
        {
            _slider.value = _slider.minValue;
        }

        private void OnClickMax()
        {
            _slider.value = _slider.maxValue;
        }

        private void TriggerAnimation(string triggerName)
        {
            if (Animator == null) return;

            Animator.ResetTrigger(TRIGGER_OPEN);
            Animator.ResetTrigger(TRIGGER_CLOSE);
            Animator.SetTrigger(triggerName);
        }
    }
}
