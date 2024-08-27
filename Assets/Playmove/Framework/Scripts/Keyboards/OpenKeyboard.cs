using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Playmove.Framework.Keyboards
{
    /// <summary>
    /// Utility to open keyboard when you click on a InputField.
    /// This keeps track of the current focused InputField to update
    /// the correct one
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class OpenKeyboard : MonoBehaviour,
        IPointerDownHandler
    {
        public static InputField CurrentInputField { get; private set; }

        [SerializeField] KeyboardProperties _properties = new KeyboardProperties();

        private InputField _ownInput;
        public InputField OwnInput
        {
            get
            {
                if (_ownInput == null)
                    _ownInput = GetComponent<InputField>();
                return _ownInput;
            }
        }

        private bool _changedInputTextFromKeyboard = false;

        void Start()
        {
            OwnInput.onValueChanged.AddListener(text =>
            {
                if (!_changedInputTextFromKeyboard)
                    Keyboard.SetText(text);
                _changedInputTextFromKeyboard = false;
            });

            Keyboard.OnTextChanged.AddListener(OnKeyboardTextChanged);
            Keyboard.OnConfirm.AddListener(_ => CurrentInputField = null);
            Keyboard.OnCancel.AddListener(() => CurrentInputField = null);
        }

        /// <summary>
        /// When this InputField receives the Down event open the keyboard
        /// with the current text that this input has
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            CurrentInputField = OwnInput;
            _properties.Text = OwnInput.text;
            Keyboard.Open(_properties);
        }
        
        /// <summary>
        /// Updates the current focused InputField when the keyboard text changes
        /// </summary>
        /// <param name="text">Current text in keyboard</param>
        private void OnKeyboardTextChanged(string text)
        {
            if (OwnInput != CurrentInputField) return;
            _changedInputTextFromKeyboard = true; 
            OwnInput.text = text;
        }
    }
}
