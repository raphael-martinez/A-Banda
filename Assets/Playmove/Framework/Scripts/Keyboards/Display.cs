using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Playmove.Framework.Keyboards
{
    /// <summary>
    /// Responsible to handle the display part of the keyboard, it's not necessary for
    /// the keyboard to work and it has a fixed limit of chars 20
    /// </summary>
    public class Display : MonoBehaviour
    {
        /// <summary>
        /// Display char limit to be used
        /// </summary>
        public const int DISPLAY_LENGTH = 20;

        [SerializeField] private Keyboard _keyboard = null;
        [SerializeField] private RectTransform _cursor = null;
        [SerializeField] private List<TextMeshProUGUI> _characters = new List<TextMeshProUGUI>();

        private void Start()
        {
            Keyboard.OnTextChanged.AddListener(OnTextChanged);
            Keyboard.OnCursorChangedPosition.AddListener(OnCursorChangedPosition);
        }

        private void OnEnable()
        {
            foreach (var character in _characters)
                character.gameObject.SetActive(false);

            // Sync state with keyboard
            SetText(_keyboard.Text);
            OnCursorChangedPosition(_keyboard.Text.Length);
        }

        /// <summary>
        /// Set text to display
        /// </summary>
        /// <param name="text">Text to be shown in display</param>
        public void SetText(string text)
        {
            if (!_keyboard.UseDisplay) return;
            if (text.Length > DISPLAY_LENGTH)
                Debug.LogWarning("Keyboard display dont support texts that exceeds 20 chars the text will be truncated!");

            for (int i = 0; i < DISPLAY_LENGTH; i++)
            {
                if (i < text.Length)
                {
                    _characters[i].gameObject.SetActive(true);
                    _characters[i].text = text[i].ToString();
                }
                else
                {
                    _characters[i].text = string.Empty;
                    _characters[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Event from Keyboard when text changes, used to sync display with keyboard
        /// </summary>
        /// <param name="text">Text that keyboard has</param>
        void OnTextChanged(string text)
        {
            SetText(text);
        }
        /// <summary>
        /// Event from Keyboard when cursor changes, used to sync cursor with keyboard
        /// </summary>
        /// <param name="cursorPosition">Cursor position from keyboard</param>
        void OnCursorChangedPosition(int cursorPosition)
        {
            if (cursorPosition < DISPLAY_LENGTH)
            {
                _cursor.anchoredPosition = _characters[cursorPosition]
                    .GetComponent<RectTransform>().anchoredPosition;
            }
            else
            {
                RectTransform rect = _characters[_characters.Count - 1].GetComponent<RectTransform>();
                _cursor.anchoredPosition = rect.anchoredPosition + Vector2.right * (rect.rect.max.x + 5);
            }
        }
    }
}
