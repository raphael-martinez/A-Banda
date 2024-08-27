using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Playmove.Framework.Keyboards
{
    public class KeyEvent : UnityEvent<Key> { }

    /// <summary>
    /// Responsible to control each key in Keyboard
    /// </summary>
    public class Key : MonoBehaviour
    {
        /// <summary>
        /// Event when the key gets clicked
        /// </summary>
        public KeyEvent OnClick = new KeyEvent();

        [SerializeField] private bool _interactable = true;
        /// <summary>
        /// Indicates if this key is Interactable
        /// </summary>
        public bool Interactable
        {
            get { return _interactable; }
            set
            {
                Button.interactable = _interactable = value;
                UpdateInteractableVisual();
            }
        }

        [SerializeField] private KeyType _type = KeyType.Letter;
        /// <summary>
        /// Key type
        /// </summary>
        public KeyType Type { get { return _type; } }

        [SerializeField] private string _letter;
        /// <summary>
        /// Letter value if any
        /// </summary>
        public string Letter
        {
            get { return _letter; }
            set
            {
                _letter = value;
                if (TextPro != null) TextPro.text = _letter;
            }
        }

        private TextMeshProUGUI _textPro;
        private TextMeshProUGUI TextPro
        {
            get
            {
                if (_textPro == null)
                    _textPro = GetComponentInChildren<TextMeshProUGUI>();
                return _textPro;
            }
        }

        private CanvasGroup _group;
        private CanvasGroup Group
        {
            get
            {
                if (_group == null)
                {
                    _group = GetComponent<CanvasGroup>();
                    if (_group == null)
                        _group = gameObject.AddComponent<CanvasGroup>();
                }
                return _group;
            }
        }

        private Button _button;
        private Button Button
        {
            get
            {
                if (_button == null)
                    _button = GetComponent<Button>();
                return _button;
            }
        }

        private void Awake()
        {
            Interactable = _interactable;
            Button.onClick.AddListener(() => OnClick.Invoke(this));
        }
        private void Start()
        {
            if (Type == KeyType.Letter)
                Letter = _letter;
        }

        /// <summary>
        /// Update the graphics of this key depending on it's Interactable state
        /// </summary>
        private void UpdateInteractableVisual()
        {
            if (Interactable)
                Group.alpha = 1;
            else
                Group.alpha = 0.5f;
        }
    }
}
