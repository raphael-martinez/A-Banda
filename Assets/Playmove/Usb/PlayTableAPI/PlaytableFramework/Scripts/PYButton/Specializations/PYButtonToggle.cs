using System.Collections.Generic;
using UnityEngine;

namespace Playmove
{
    public class PYButtonToggle : PYButton
    {
        protected static Dictionary<string, PYButtonToggle> _currentButtonSelected;
        protected static Dictionary<string, PYButtonToggle> _lastButtonSelected;

        [Header("PYButtonToggle")]
        public PYButtonEvent onSelected = new PYButtonEvent();

        public PYButtonEvent onDeselected = new PYButtonEvent();

        [SerializeField]
        protected bool _automaticSelectButtonOnRelease = true;

        [SerializeField]
        protected bool _clickWhenSelectedDeseletec = false;

        public bool GenerateUniqueTag = false;

        [SerializeField]
        private string _tag;

        public string Tag
        {
            get
            {
                if (GenerateUniqueTag && string.IsNullOrEmpty(_tag))
                    _tag = System.Guid.NewGuid().ToString();
                return _tag;
            }
            set { _tag = value; }
        }

        [SerializeField]
        private bool startSelected = false;

        public bool IsSelected
        {
            get { return _currentButtonSelected[Tag] == this; }
        }

        protected override void Awake()
        {
            base.Awake();
            if (_currentButtonSelected == null)
                _currentButtonSelected = new Dictionary<string, PYButtonToggle>();

            if (_lastButtonSelected == null)
                _lastButtonSelected = new Dictionary<string, PYButtonToggle>();

            if (!_currentButtonSelected.ContainsKey(Tag))
                _currentButtonSelected.Add(Tag, null);

            if (startSelected)
                Select();
            else
                DeselectAction();
        }

        #region Static Methods

        public static void Select(string tag, PYButtonToggle button)
        {
            Debug.LogError("Still not implemented!");
        }

        public static void Deselect(string tag)
        {
            if (!_currentButtonSelected.ContainsKey(tag))
                return;

            if (_currentButtonSelected[tag] != null)
            {
                _currentButtonSelected[tag].Deselect();
                return;
            }
        }

        public static PYButtonToggle GetSelected(string tag)
        {
            return _currentButtonSelected[tag];
        }

        public static PYButtonToggle GetLastSelected(string tag)
        {
            if (!_lastButtonSelected.ContainsKey(tag))
                return null;

            return _lastButtonSelected[tag];
        }

        #endregion Static Methods

        protected virtual void SelectAction()
        {
        }

        protected virtual void DeselectAction()
        {
        }

        public virtual void Select()
        {
            // This is need because the button could start inactive
            // and if someone tries to select the inactive button
            // NullException occurs
            if (!_currentButtonSelected.ContainsKey(Tag))
                _currentButtonSelected.Add(Tag, null);

            // Deselect the button just if it is different from the last
            if (_currentButtonSelected[Tag] != null)
            {
                if (_currentButtonSelected[Tag] != this ||
                    _clickWhenSelectedDeseletec)
                    _currentButtonSelected[Tag].Deselect();

                if (_clickWhenSelectedDeseletec)
                    return;
            }

            // If user clicks in the already select button
            // the event should not trigger again
            if (_currentButtonSelected[Tag] == this)
                return;

            _currentButtonSelected[Tag] = this;
            SelectAction();

            onSelected.Invoke(this);
        }

        public virtual void Deselect()
        {
            if (!_currentButtonSelected.ContainsKey(Tag))
                return;

            if (_currentButtonSelected[Tag] != null)
            {
                if (!_lastButtonSelected.ContainsKey(Tag))
                    _lastButtonSelected.Add(Tag, _currentButtonSelected[Tag]);
                _lastButtonSelected[Tag] = _currentButtonSelected[Tag];

                _currentButtonSelected[Tag] = null;
                DeselectAction();

                onDeselected.Invoke(this);
            }
        }

        protected override void ClickAction()
        {
            if (_automaticSelectButtonOnRelease)
                Select();
        }
    }
}