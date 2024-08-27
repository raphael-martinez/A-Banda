using Playmove.Core.BasicEvents;
using Playmove.Core.Bundles;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Playmove.Framework.Popups
{
    public class Popup : MonoBehaviour
    {
        private static Dictionary<string, Popup> _pool = new Dictionary<string, Popup>();

        public static Transform PopupCanvas
        {
            get { return GameObject.Find("PlaytableCanvasPopup").transform; }
        }

        public static Popup Open(string title, string message, PopupButton positiveButton, PopupButton negativeButton, UnityAction<Popup> onClosed = null)
        {
            return Open(AssetsCatalog.GameObject_Popup2Button, title, message, onClosed, positiveButton, negativeButton);
        }
        public static Popup Open(string title, string message, PopupButton okButton, UnityAction<Popup> onClosed = null)
        {
            return Open(AssetsCatalog.GameObject_Popup1Button, title, message, onClosed, okButton);
        }
        public static Popup Open(string popupAssetName, string title, string message, 
            UnityAction<Popup> onClosed = null, params PopupButton[] buttons)
        {
            Popup popup = null;
            if (_pool.ContainsKey(popupAssetName))
            {
                popup = _pool[popupAssetName];
                _pool.Remove(popupAssetName);
            }
            else
                popup = Instantiate(Data.GetAsset<GameObject>(popupAssetName), PopupCanvas, false).GetComponent<Popup>();

            popup.OnClosed.RemoveAllListeners();
            if (onClosed != null) popup.OnClosed.AddListener(onClosed);
            return popup.Open(title, message, buttons);
        }

        public PlaytableEvent<Popup> OnClosed = new PlaytableEvent<Popup>();

        [SerializeField] TextMeshProUGUI _title = null;
        [SerializeField] TextMeshProUGUI _message = null;
        [SerializeField] Transform[] _buttonsSpot = null;

        public bool IsOpen { get; set; }

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

        public void UpdateMessage(string message)
        {
            _message.text = message;
        }

        public Popup Open(string title, string message, PopupButton[] buttons)
        {
            if (IsOpen) return this;
            IsOpen = true;

            _title.text = title;
            _message.text = message;

            if (buttons == null) return this;
            for (int i = 0; i < Mathf.Min(buttons.Length, _buttonsSpot.Length); i++)
            {
                Button uniButton = _buttonsSpot[i].GetComponentInChildren<Button>();
                if (uniButton == null)
                {
                    uniButton = Instantiate(Data.GetAsset<GameObject>(buttons[i].AssetName),
                        _buttonsSpot[i], false).GetComponent<Button>();
                }
                else
                    uniButton.onClick.RemoveAllListeners();

                uniButton.GetComponentInChildren<TextMeshProUGUI>().text = buttons[i].Text;
                RegisterActionsInButton(uniButton, buttons[i]);
            }

            gameObject.SetActive(true);
            if (Animator != null) Animator.SetTrigger("Open");
            return this;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;

            if (Animator != null) Animator.SetTrigger("Close");
            else Closed();
        }

        private void Closed()
        {
            gameObject.SetActive(false);
            OnClosed.Invoke(this);
            Destroy(gameObject);
        }

        private void RegisterActionsInButton(Button button, PopupButton popupButton)
        {
            button.onClick.AddListener(() =>
            {
                popupButton.Action?.Invoke(this);
                if (popupButton.ClosePopup)
                    Close();
            });
        }
    }
}
