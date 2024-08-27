using Playmove.Core.BasicEvents;
using Playmove.Core.Bundles;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Playmove.Core.Controls
{
    public class ControlLanguageSelector : Openable
    {
        [Serializable]
        public class LanguageData
        {
            [Tooltip("Language needs to be in full format pt-BR or en-US")]
            public string Language = "pt-BR";
            public Button Button;
        }

        private const string TRIGGER_OPEN = "Open";
        private const string TRIGGER_CLOSE = "Close";

        public PlaytableEventString OnLanguageChanged = new PlaytableEventString();

        [SerializeField] Image _iconLanguageSelected = null;
        [SerializeField] List<LanguageData> _languageOptions = new List<LanguageData>();

        public string CurrentLanguage { get; private set; }

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

        private List<string> _availableLanguages = new List<string>();
        private bool _triedToCloseWhileOpening = false;
        private bool _triedToOpenWhileClosing = false;

        private void Start()
        {
            if (Playtable.Instance.IsReady)
                Initialize();
            else
                Playtable.Instance.OnPlaytableReady.AddListener(Initialize);
        }
        private void Initialize()
        {
            _availableLanguages = PlaytableBundlesPath.GetAvailableLocalizations();
            foreach (var option in _languageOptions)
            {
                if (_availableLanguages.Find(lang => lang.ToLower().StartsWith(option.Language.ToLower())) != null)
                    RegisterClickEvent(option);
                else
                    option.Button.gameObject.SetActive(false);
            }
        }

        public void UpdateLanguageIcon()
        {
            UpdateLanguageIcon(GameSettings.Language);
        }
        public void UpdateLanguageIcon(string language)
        {
            foreach (var option in _languageOptions)
            {
                if (language.ToLower().StartsWith(option.Language.ToLower()))
                {
                    _iconLanguageSelected.sprite = option.Button.image.sprite;
                    break;
                }
            }
        }

        public override void Open()
        {
            if (State == OpenableState.Closing) _triedToOpenWhileClosing = true;
            if (State != OpenableState.Closed) return;

            gameObject.SetActive(true);
            SetLanguageOptionInteractable(false);
            TriggerAnimation(TRIGGER_OPEN);
            base.Open();
        }
        protected override void Opened()
        {
            SetLanguageOptionInteractable(true);
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

        private void RegisterClickEvent(LanguageData languageOption)
        {
            languageOption.Button.onClick.AddListener(() => OnLanguageSelected(languageOption));
        }

        private void OnLanguageSelected(LanguageData languageOption)
        {
            ControlBoxSubPopup.CloseIfAny();

            CurrentLanguage = _availableLanguages.Find(lang => lang.ToLower().StartsWith(languageOption.Language.ToLower()));

            UpdateLanguageIcon(CurrentLanguage);
            OnLanguageChanged.Invoke(CurrentLanguage);
        }

        private void SetLanguageOptionInteractable(bool interactable)
        {
            foreach (var option in _languageOptions)
                option.Button.interactable = interactable;
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
