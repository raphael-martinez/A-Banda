using Playmove.Avatars;
using Playmove.Avatars.API;
using Playmove.Avatars.API.Models;
using Playmove.Core.API;
using Playmove.Core.Audios;
using Playmove.Core.Bundles;
using Playmove.Framework;
using Playmove.Framework.Popups;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Playmove.Core.Controls
{
    [Serializable]
    public class HideButtonsInSceneData
    {
        public string SceneName;
        public List<Transform> Buttons;
    }
    public class ControlCenter : Openable, IPointerDownHandler
    {
        private static ControlCenter _instance = null;
        public static ControlCenter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<ControlCenter>();
                return _instance;
            }
        }

        private const string TRIGGER_OPEN = "Open";
        private const string TRIGGER_CLOSE = "Close";
        
        [SerializeField] GameObject _waitAvatarToCloseObj = null;
        public GameObject WaitAvatarToCloseObj { get { return _waitAvatarToCloseObj; } }
        [SerializeField] ControlLanguageSelector _languageSelector = null;
        [SerializeField] ControlSlider _microphoneSlider = null;
        [SerializeField] ControlSlider _volumeSlider = null;
        [SerializeField] List<Transform> _buttons = new List<Transform>();
        [Header("Buttons to be hidden in each Scene")]
        [SerializeField] List<HideButtonsInSceneData> _buttonsToHideInScenes = new List<HideButtonsInSceneData>();

        private Animator _animator = null;
        private Animator Animator
        {
            get
            {
                if (_animator == null)
                    _animator = GetComponent<Animator>();
                return _animator;
            }
        }

        private AvatarNotification _avatarNotification = null;
        private AvatarNotification AvatarNotification
        {
            get
            {
                if (_avatarNotification == null)
                    _avatarNotification = GetComponent<AvatarNotification>();
                return _avatarNotification;
            }
        }

        private void Start()
        {
            if (Playtable.Instance.IsReady)
                Initialize();
            else
                Playtable.Instance.OnPlaytableReady.AddListener(Initialize);

            _languageSelector.OnLanguageChanged.AddListener(language => GameSettings.Language = language);
            _volumeSlider.OnValueChanged.AddListener(volume => GameSettings.Volume = volume);
            _microphoneSlider.OnValueChanged.AddListener(volume => GameSettings.MicVolume = volume);

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private void Initialize()
        {
            _languageSelector.UpdateLanguageIcon();
            _volumeSlider.UpdateGraphics(GameSettings.Volume);
            _microphoneSlider.UpdateGraphics(GameSettings.MicVolume);

            AvatarNotification.CheckAvatarSlots();
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            Animator animator = gameObject.GetComponent<Animator>();

            if (!animator.enabled)
                animator.enabled = true;

            // Active all buttons
            foreach (var button in _buttons)
                button.gameObject.SetActive(true);

            // Hide any button that needs to be hidden
            HideButtonsInSceneData hideButtons = _buttonsToHideInScenes.Find(item => item.SceneName == scene.name);
            if (hideButtons != null)
            {
                foreach (var button in hideButtons.Buttons)
                {
                    if (string.Equals(button.name, "ButtonOpenBar", StringComparison.OrdinalIgnoreCase))
                        animator.enabled = false;

                    button.gameObject.SetActive(false);
                }
            }
        }

        private void SceneManager_sceneUnloaded(Scene scene)
        {
            if (IsOpen)
                Close();
        }
        
        public override void Open()
        {
            if (State != OpenableState.Closed) return;
            
            TriggerAnimation(TRIGGER_OPEN);
            base.Open();
        }

        public override void Close()
        {
            if (State != OpenableState.Opened) return;

            if (ControlBoxSubPopup.CurrentSubPopup != null)
            {
                ControlBoxSubPopup.CurrentSubPopup.OnClosed.AddListener(WaitSubPopupToClose);
                ControlBoxSubPopup.CloseIfAny();
            }
            else
                TriggerAnimation(TRIGGER_CLOSE);
                       
            base.Close();
        }
        private void WaitSubPopupToClose(Openable openable)
        {
            openable.OnClosed.RemoveListener(WaitSubPopupToClose);
            TriggerAnimation(TRIGGER_CLOSE);
        }

        #region Methods connected to Unity Inspector
        public void CloseGame()
        {
            ControlBoxSubPopup.CloseIfAny();
            Fader.FadeTo(0.75f, 0.5f);
            Popup.Open(Localization.GetAsset<string>(AssetsCatalog.string_Attention), Localization.GetAsset<string>(AssetsCatalog.string_WantToClose),
                new PopupButton(Localization.GetAsset<string>(AssetsCatalog.string_Cancel)),
                new PopupButton(Localization.GetAsset<string>(AssetsCatalog.string_Continue)) { Action = _ => Playtable.Instance.ForceExit() },
                _ => Fader.FadeTo(0, 0.5f));
        }

        public void OpenAvatar()
        {
            ControlBoxSubPopup.CloseIfAny();
            AvatarAPI.Open(null);
        }

        public void RotateScreen()
        {
            ControlBoxSubPopup.CloseIfAny();
            Fader.FadeTo(1, 0.5f, () =>
                PlaytableAPI.RotateScreen(result =>
                {
                    Fader.FadeTo(0, 0.5f);
                })
            );
        }
        #endregion

        private void TriggerAnimation(string triggerName)
        {
            if (Animator == null) return;
            
            Animator.ResetTrigger(TRIGGER_OPEN);
            Animator.ResetTrigger(TRIGGER_CLOSE);
            Animator.SetTrigger(triggerName);
        }

        // Trick to Close when user clicks outside of the bar
        private void LateUpdate()
        {
            if (State == OpenableState.Opened)
            {
                GameObject eventObj = EventSystem.current.currentSelectedGameObject;
                if (eventObj == null)
                {
                    Close();
                    return;
                }

                if (!eventObj.transform.IsChildOf(transform))
                    Close();
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(eventData.pointerCurrentRaycast.gameObject);
        }
    }
}
