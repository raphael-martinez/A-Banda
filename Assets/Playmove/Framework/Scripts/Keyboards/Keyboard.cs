using Newtonsoft.Json;
using Playmove.Core.Audios;
using Playmove.Core.BasicEvents;
using Playmove.Core.Bundles;
using Playmove.Framework.Popups;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Playmove.Framework.Keyboards
{
    public enum KeyboardPosition
    {
        Bottom,
        Top
    }

    public enum KeyboardPage
    {
        Alphabet,
        Accents,
        Numeric
    }

    [Serializable]
    public class KeyboardProperties
    {
        public AnimatorOverrideController AnimatorController = null;
        public bool CloseOnConfirm = true;
        public bool CloseOnCancel = true;
        public string Text = string.Empty;
        public string ConfirmLabel = string.Empty;
        public string CancelLabel = string.Empty;
        public bool IsCapsLock = true;
        public bool IsMuted = false;
        public bool UseDisplay = true;
        public bool ValidateText = true;
        public bool ShowPopupInvalidText = true;
        public KeyboardPage Page = KeyboardPage.Alphabet;
        public KeyboardPosition Position = KeyboardPosition.Bottom;
        public bool hideFader = true;
    }

    public class Keyboard : MonoBehaviour
    {
        private static Keyboard _instance;
        public static Keyboard Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Keyboard>();

                    if (_instance == null)
                    {
                        var playtableCanvas = GameObject.Find("PlaytableCanvasPopup").transform;
                        var asset = Instantiate(Data.GetAsset<GameObject>("Keyboard"), playtableCanvas, false);
                        asset.transform.SetAsLastSibling();
                        _instance = asset.GetComponent<Keyboard>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Open keyboard with the default settings
        /// Text = string.Empty, Page = Alphabet, UseDisplay = true,
        /// ValidateWord = true, Position = Bottom
        /// </summary>
        public static void Open(UnityAction<string> onConfirm = null, UnityAction onCancel = null, bool hideFader = true)
        {
            Open(new KeyboardProperties(), onConfirm, onCancel, hideFader);
        }

        /// <summary>
        /// Open keyboard and you can configure the settings
        /// </summary>
        /// <param name="properties"></param>
        public static void Open(KeyboardProperties properties, UnityAction<string> onConfirm = null, UnityAction onCancel = null, bool hideFader = true)
        {
            if (IsOpen) return;
            Instance.Properties = properties;
            Instance.Properties.hideFader = hideFader;
            IsOpen = true;
            SetText(properties.Text);
            Instance.IsCapsLock = properties.IsCapsLock;
            Instance.IsMuted = properties.IsMuted;
            Instance.UseDisplay = properties.UseDisplay;
            Instance.ValidateText = properties.ValidateText;

            RectTransform rect = Instance.GetComponent<RectTransform>();
            switch (properties.Position)
            {
                case KeyboardPosition.Top:
                    rect.pivot = Vector2.one / 2;
                    rect.localRotation = new Quaternion(0, 0, 180, 0);
                    break;
                default:
                    rect.pivot = Vector2.one / 2;
                    rect.localRotation = Quaternion.identity;
                    break;
            }

            // One exec events
            if (onConfirm != null)
            {
                Instance._onOneExecConfirm = onConfirm;
                OnConfirm.AddListener(onConfirm);
            }
            if (onCancel != null)
            {
                Instance._onOneExecCancel = onCancel;
                OnCancel.AddListener(onCancel);
            }

            // If display is not being used we active all keys
            if (!properties.UseDisplay)
            {
                foreach (var key in Instance.KeysLetter)
                    key.Interactable = true;
                foreach (var key in Instance.KeysAction.Values)
                    key.Interactable = true;
            }

            Instance.LoadPage(properties.Page);
            Instance.gameObject.SetActive(true);

            if (properties.AnimatorController != null)
                Instance.Animator.runtimeAnimatorController = properties.AnimatorController;
            OnOpen.Invoke();
            Instance.Animator.SetTrigger("Open");
        }

        /// <summary>
        /// Close the keyboard
        /// </summary>
        public static void Close()
        {
            if (!IsOpen) return;
            
            if(Instance.Properties.hideFader)
                Fader.FadeOut();

            if (Instance._onOneExecConfirm != null)
                OnConfirm.RemoveListener(Instance._onOneExecConfirm);
            if (Instance._onOneExecCancel != null)
                OnCancel.RemoveListener(Instance._onOneExecCancel);

            OnClose.Invoke();
            Instance.Animator.SetTrigger("Close");
        }

        /// <summary>
        /// Event fired when keyboard method Open is called
        /// </summary>
        public static PlaytableEvent OnOpen = new PlaytableEvent();
        /// <summary>
        /// Event fired when keyboard method Close is called
        /// </summary>
        public static PlaytableEvent OnClose = new PlaytableEvent();
        /// <summary>
        /// Event fired when button Confirm is pressed
        /// </summary>
        public static PlaytableEventString OnConfirm = new PlaytableEventString();
        /// <summary>
        /// Event fired when button Cancel is pressed
        /// </summary>
        public static PlaytableEvent OnCancel = new PlaytableEvent();
        /// <summary>
        /// Event fired when any Text on the keyboard is changed
        /// </summary>
        public static PlaytableEventString OnTextChanged = new PlaytableEventString();
        /// <summary>
        /// Event fired when cursor position is changed
        /// </summary>
        public static PlaytableEventInt OnCursorChangedPosition = new PlaytableEventInt();
        /// <summary>
        /// Event fired when user is using ValidText = true and user tries to confirm an invalid text
        /// </summary>
        public static PlaytableEventString OnTriedToConfirmBadText = new PlaytableEventString();

        /// <summary>
        /// Set text in keyboard
        /// </summary>
        /// <param name="text">Text to be applied in keyboard</param>
        public static void SetText(string text)
        {
            Instance.Text = text;
            Instance.CursorPosition = Instance.Text.Length;
        }

        /// <summary>
        /// Indicates wheter keyboard is open or not
        /// </summary>
        public static bool IsOpen { get; private set; }

        [SerializeField] private GameObject _display = null;

        [Header("Caps Lock")]
        [SerializeField] private Color _capsLockActivated = Color.white;
        [SerializeField] private Color _capsLockDeactivated = new Color(0.72f, 0.47f, 0.12f);

        [Header("Mute")]
        [SerializeField] private Sprite _muteActivated = null;
        [SerializeField] private Sprite _muteDeactivated = null;

        #region Public Properties
        public KeyboardProperties Properties { get; private set; }

        private bool _isCapsLock = true;
        /// <summary>
        /// Indicates if the CapsLock is on/off
        /// </summary>
        public bool IsCapsLock
        {
            get { return _isCapsLock; }
            set
            {
                _isCapsLock = value;
                // Update graphics
                KeysAction[KeyType.CapsLock].transform.GetChild(0).GetComponent<Image>().color =
                    _isCapsLock ? _capsLockActivated : _capsLockDeactivated;
                foreach (var key in KeysLetter)
                {
                    key.Letter = _isCapsLock ?
                        key.Letter.ToUpper() : key.Letter.ToLower();
                }
            }
        }

        private bool _isMuted = false;
        /// <summary>
        /// Indicates if the keyboard will narrate or not
        /// </summary>
        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                _isMuted = value;
                // Update graphics
                KeysAction[KeyType.Mute].transform.GetChild(0).GetComponent<Image>().sprite =
                    _isMuted ? _muteActivated : _muteDeactivated;
            }
        }

        private bool _useDisplay;
        /// <summary>
        /// Indicates if the Display is being used
        /// </summary>
        public bool UseDisplay
        {
            get { return _useDisplay; }
            set
            {
                _useDisplay = value;
                _display.SetActive(_useDisplay);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ValidateText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ShowPopupInvalidText { get; set; }

        private string _text = string.Empty;
        /// <summary>
        /// Current text
        /// </summary>
        public string Text
        {
            get { return _text; }
            private set
            {
                _text = value;
                if (UseDisplay && _text.Length > Display.DISPLAY_LENGTH)
                    _text = _text.Substring(0, Display.DISPLAY_LENGTH);

                DisableKeysAction(Text, CursorPosition);
                OnTextChanged.Invoke(_text);
            }
        }

        private int _cursorPosition = 0;
        /// <summary>
        /// Current cursor position
        /// </summary>
        public int CursorPosition
        {
            get { return _cursorPosition; }
            private set
            {
                int max = Text.Length;
                if (UseDisplay)
                    max = Mathf.Min(max, Display.DISPLAY_LENGTH);

                _cursorPosition = Mathf.Clamp(value, 0, max);
                DisableKeysAction(Text, CursorPosition);
                OnCursorChangedPosition.Invoke(_cursorPosition);
            }
        }
        #endregion

        private UnityAction<string> _onOneExecConfirm;
        private UnityAction _onOneExecCancel;

        private List<Key> _keysLetter;
        /// <summary>
        /// All keys letter references
        /// </summary>
        private List<Key> KeysLetter
        {
            get
            {
                if (_keysLetter == null)
                {
                    _keysLetter = new List<Key>(GetComponentsInChildren<Key>()
                        .Where(key => key.Type == KeyType.Letter));
                }
                return _keysLetter;
            }
        }

        private Dictionary<KeyType, Key> _keysAction;
        /// <summary>
        /// All keys action references
        /// </summary>
        private Dictionary<KeyType, Key> KeysAction
        {
            get
            {
                if (_keysAction == null)
                {
                    _keysAction = new Dictionary<KeyType, Key>();
                    foreach (var key in GetComponentsInChildren<Key>())
                    {
                        if (key.Type == KeyType.Letter) continue;
                        _keysAction.Add(key.Type, key);
                    }
                }
                return _keysAction;
            }
        }

        private KeyboardConfiguration _configuration; 
        /// <summary>
        /// Current configuration localized
        /// </summary>
        private KeyboardConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    TextAsset configAsset = Localization.GetAsset<TextAsset>("keyboardConfiguration");
                    if (configAsset == null) return null;
                    _configuration = JsonConvert.DeserializeObject<KeyboardConfiguration>(configAsset.text);
                }
                return _configuration;
            }
        }

        private Animator _animator;
        /// <summary>
        /// Unity animator reference
        /// </summary>
        private Animator Animator
        {
            get
            {
                if (_animator == null)
                    _animator = GetComponent<Animator>();
                return _animator;
            }
        }

        private KeyboardPage _currentPage = 0;

        private void Awake()
        {
            _instance = this;

            foreach (var key in KeysLetter)
                key.OnClick.AddListener(ProccessKey);
            foreach (var keyAction in KeysAction.Values)
                keyAction.OnClick.AddListener(ProccessKey);

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Proccess any clicked key
        /// </summary>
        /// <param name="key">Actual key that was clicked</param>
        private void ProccessKey(Key key)
        {
            switch (key.Type)
            {
                case KeyType.Letter:
                    Text = Text.Insert(CursorPosition, key.Letter);
                    CursorPosition++;
                    if (key.Letter == ".") PlayAudio(AssetsCatalog.AudioClip_ponto);
                    else PlayAudio(key.Letter.ToLower());
                    break;
                case KeyType.CapsLock:
                    IsCapsLock = !IsCapsLock;
                    // TODO: Missing audio
                    break;
                case KeyType.Backspace:
                    if (--CursorPosition < 0) return;
                    Text = Text.Remove(CursorPosition, 1);
                    PlayAudio(AssetsCatalog.AudioClip_apagar);
                    break;
                case KeyType.Mute:
                    IsMuted = !IsMuted;
                    break;
                case KeyType.NextPage:
                    _currentPage++;
                    if ((int)_currentPage >= Configuration.Pages.Count)
                        _currentPage = 0;
                    LoadPage(_currentPage);

                    switch (_currentPage)
                    {
                        case KeyboardPage.Alphabet:
                            PlayAudio(AssetsCatalog.AudioClip_alfabeto);
                            break;
                        case KeyboardPage.Accents:
                            PlayAudio(AssetsCatalog.AudioClip_acentos);
                            break;
                        case KeyboardPage.Numeric:
                            // TODO: Missing audio
                            break;
                    }
                    break;
                case KeyType.EraseAll:
                    SetText(string.Empty);
                    PlayAudio(AssetsCatalog.AudioClip_apagar_tudo);
                    break;
                case KeyType.Space:
                    Text = Text.Insert(CursorPosition, " ");
                    CursorPosition++;
                    PlayAudio(AssetsCatalog.AudioClip_espaco);
                    break;
                case KeyType.CursorBack:
                    CursorPosition--;
                    PlayAudio(AssetsCatalog.AudioClip_seta_esquerda);
                    break;
                case KeyType.CursorForward:
                    CursorPosition++;
                    PlayAudio(AssetsCatalog.AudioClip_seta_direita);
                    break;
                case KeyType.Cancel:
                    OnCancel.Invoke();
                    if (Properties.CloseOnCancel) Close();
                    break;
                case KeyType.Confirm:
                    if (ValidateText)
                    {
                        if (BannedWords.IsValid(Text))
                        {
                            OnConfirm.Invoke(Text);
                            if (Properties.CloseOnConfirm) Close();
                        }
                        else
                        {
                            OnTriedToConfirmBadText.Invoke(Text);
                            if (Properties.ShowPopupInvalidText)
                            {
                                AudioManager.StartAudio(AudioChannel.Voice, AssetsCatalog.AudioClip_BlockedFiltersMessage).Play();
                                Fader.FadeTo(0.75f, 0.5f);
                                Popup.Open(Localization.GetAsset<string>(AssetsCatalog.string_Attention), 
                                    Localization.GetAsset<string>(AssetsCatalog.string_BlockedFiltersMessage),
                                    new PopupButton("Ok"), 
                                    _ =>
                                        {
                                            Fader.FadeTo(0, 0.5f);
                                            AudioManager.StopAudio(AssetsCatalog.AudioClip_BlockedFiltersMessage, 0.5f);
                                        }
                                    );
                            }
                        }
                    }
                    else
                    {
                        OnConfirm.Invoke(Text);
                        if (Properties.CloseOnConfirm) Close();
                    }
                    break;
            }
        }

        /// <summary>
        /// Load specified page
        /// </summary>
        /// <param name="page">Page id to be loaded</param>
        private void LoadPage(KeyboardPage page)
        {
            page = (KeyboardPage)Mathf.Clamp((int)page, 0, Configuration.Pages.Count);
            int keyIndex = 0;
            foreach (var line in Configuration.Pages[(int)page].Lines)
            {
                string[] splitted = line.Split(' ');
                for (int i = 0; i < splitted.Length; i++)
                {
                    KeysLetter[keyIndex].Letter = splitted[i];
                    KeysLetter[keyIndex].Interactable = true;
                    keyIndex++;
                }
            }

            // Disable keys that dont have value
            for (int i = keyIndex; i < KeysLetter.Count; i++)
            {
                KeysLetter[i].Letter = string.Empty;
                KeysLetter[i].Interactable = false;
            }

            KeysAction[KeyType.NextPage].Letter = Configuration.Pages[(int)page].NextPage;
            KeysAction[KeyType.Confirm].Letter = string.IsNullOrEmpty(Properties.ConfirmLabel) ? Configuration.Confirm : Properties.ConfirmLabel;
            KeysAction[KeyType.Cancel].Letter = string.IsNullOrEmpty(Properties.CancelLabel) ? Configuration.Cancel : Properties.CancelLabel;
            KeysAction[KeyType.Backspace].Letter = Configuration.Backspace;
            KeysAction[KeyType.EraseAll].Letter = Configuration.ClearAll;
            DisableKeysAction(Text, CursorPosition);
        }

        /// <summary>
        /// Disable keys when certain conditions are match, this for now is only being used
        /// when display is being used
        /// </summary>
        /// <param name="text">Text to be valid</param>
        /// <param name="cursorPosition">CursorPosition to be valid</param>
        private void DisableKeysAction(string text, int cursorPosition)
        {
            if (!UseDisplay) return;
            if (text.Length == 0)
            {
                KeysAction[KeyType.EraseAll].Interactable =
                    KeysAction[KeyType.Space].Interactable =
                    KeysAction[KeyType.CursorBack].Interactable =
                    KeysAction[KeyType.CursorForward].Interactable =
                    KeysAction[KeyType.Confirm].Interactable = false;
            }
            else
            {
                KeysAction[KeyType.EraseAll].Interactable =
                    KeysAction[KeyType.Space].Interactable =
                    KeysAction[KeyType.CursorBack].Interactable =
                    KeysAction[KeyType.CursorForward].Interactable =
                    KeysAction[KeyType.Confirm].Interactable = true;

                KeysAction[KeyType.Space].Interactable = text.LastOrDefault() != ' ';
                KeysAction[KeyType.CursorForward].Interactable = cursorPosition < text.Length;
            }

            KeysAction[KeyType.CursorBack].Interactable = cursorPosition != 0;

            if (cursorPosition == 0)
            {
                KeysAction[KeyType.Space].Interactable = false;
                KeysAction[KeyType.Backspace].Interactable = false;
                string specialChars = "-,.";
                for (int i = 0; i < specialChars.Length; i++)
                {
                    Key keyLetter = KeysLetter.Find(key => key.Letter == specialChars[i].ToString());
                    if (keyLetter == null) continue;
                    keyLetter.Interactable = false;
                }
            }
            else
            {
                KeysAction[KeyType.Backspace].Interactable = true;
                string specialChars = "-,.";
                for (int i = 0; i < specialChars.Length; i++)
                {
                    Key keyLetter = KeysLetter.Find(key => key.Letter == specialChars[i].ToString());
                    if (keyLetter == null) continue;
                    keyLetter.Interactable = true;
                }
                
                if ((cursorPosition - 1 < Text.Length && Text[cursorPosition - 1] == ' ') || 
                    (cursorPosition < Text.Length && Text[cursorPosition] == ' '))
                    KeysAction[KeyType.Space].Interactable = false;
                else
                    KeysAction[KeyType.Space].Interactable = true;
            }
        }

        /// <summary>
        /// Play audio for keyboard
        /// </summary>
        /// <param name="clipName">Audio clip name to be played</param>
        private void PlayAudio(string clipName)
        {
            if (IsMuted) return;

            AudioClip clip = Localization.GetAsset<AudioClip>(clipName);
            if (clip == null) return;
            AudioManager.StartAudio(AudioChannel.Voice, clip).Play();
        }

        /// <summary>
        /// Called from Unity Animation and set the state to not open
        /// </summary>
        private void Closed()
        {
            gameObject.SetActive(false);
            IsOpen = false;
        }
    }
}
