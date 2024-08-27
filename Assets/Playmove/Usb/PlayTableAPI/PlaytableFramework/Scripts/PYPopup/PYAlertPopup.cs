using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Playmove
{
    public class PYAlertPopup : PYPopup
    {
        [Header("PYAlertPopup")]
        public PYText Title;

        public PYText Text;
        public bool DestroyOnClose = true;
        public List<Transform> ButtonsHolder;

        private List<UIBehaviour> _buttons;
        private List<bool> _buttonsShouldClosePopups = new List<bool>();

        public override void Open()
        {
            base.Open();
            EnableButtons(false);

            for (int x = 0; x < _buttonsShouldClosePopups.Count; x++)
            {
                if (_buttonsShouldClosePopups[x])
                {
                    if (_buttons[x] is Button)
                        ((Button)_buttons[x]).onClick.AddListener(Close);
                    else if (_buttons[x] is PYButton)
                        ((PYButton)_buttons[x]).onClick.AddListener((sender) => Close());
                }
                _buttonsShouldClosePopups[x] = false;
            }
        }

        protected override void Opened()
        {
            base.Opened();
            EnableButtons(true);
        }

        public override void Close()
        {
            base.Close();
            EnableButtons(false);
        }

        protected override void Closed()
        {
            base.Closed();

            if (DestroyOnClose)
                Destroy(gameObject);
        }

        public static PYAlertPopup InvokeAlertPopup(string resourcePopupName = "Popup")
        {
            GameObject popUpGameObject = Resources.Load<GameObject>(TagManager.POPUP_RESOURCES_PATH + resourcePopupName);
            if (popUpGameObject == null) return null;

            popUpGameObject = Instantiate(popUpGameObject);
            popUpGameObject.transform.position = Vector3.forward * TagManager.PYPOPUPS_Z_POSITION;

            return popUpGameObject.GetComponent<PYAlertPopup>();
        }

        public static PYAlertPopup InvokeConfirmPopup(string text, Action confirmAction)
        {
            Playmove.PYAlertPopup popup = Playmove.PYAlertPopup.InvokeAlertPopup("AlertPopup_2b");
            popup.AddButton("PYButtonText", "Cancelar");
            popup.AddButton("PYButtonText", "Confirmar");
            popup.AddButtonAction(1, confirmAction);

            popup.SetTitle("Atenção!");
            popup.SetText(text);

            popup.UseFader = true;
            popup.Open();

            return popup;
        }

        /// <summary>
        /// Colocar popup na posição referente a camera
        /// </summary>
        /// <param name="viewportPosition">new Vector3(0.5f, 0.5f, 5f) padrão de popups</param>
        public PYAlertPopup Setposition(Vector3 viewportPosition)
        {
            OwnTransform.position = Camera.main.ViewportToWorldPoint(viewportPosition);
            return this;
        }

        public PYAlertPopup Setposition(Vector3 position, bool isLocal)
        {
            if (isLocal)
                OwnTransform.localPosition = position;
            else
                OwnTransform.position = position;

            return this;
        }

        public PYAlertPopup SetTitle(string title)
        {
            Title.Text = title;
            return this;
        }

        public PYAlertPopup SetTitle(string title, int order, string layerName = "GUI")
        {
            SetTitle(title);
            Title.SetOrder(order, layerName);
            return this;
        }

        public PYAlertPopup SetText(string text)
        {
            Text.Text = text;
            return this;
        }

        public PYAlertPopup SetText(string text, int order, string layerName = "GUI")
        {
            SetText(text);
            Text.SetOrder(order, layerName);
            return this;
        }

        /// <summary>
        /// Adiciona um botão
        /// </summary>
        /// <param name="buttonName"> Nome do prefab que deve estar tagmanager.POPUP_BUTTON_RESOURCES_PATH </param>
        /// <param name="content"> Pode ser uma string ou uma imagem, dependendo da situação </param>
        /// <param name="closePopup">Se adicionar o comportament de fechar a popup</param>
        /// <returns></returns>
        public PYAlertPopup AddButton(string buttonName, object content, bool closePopup = true)
        {
            if (_buttons == null) _buttons = new List<UIBehaviour>();

            GameObject btn = Resources.Load<GameObject>(TagManager.POPUP_BUTTON_RESOURCES_PATH + buttonName);
            if (btn == null)
                return null;

            btn = (GameObject)Instantiate(btn, Vector3.zero, ButtonsHolder[_buttons.Count].rotation);
            btn.transform.SetParent(ButtonsHolder[_buttons.Count]);
            btn.transform.localPosition = Vector3.zero;
            btn.transform.localScale = Vector3.one;

            PYButtonContent btnRef = btn.GetComponent<PYButtonContent>();
            if (content != null) btnRef.SetContent(content);

            foreach (UIBehaviour button in btn.GetComponents<UIBehaviour>())
                if (button is Button || button is PYButton)
                    _buttons.Add(button);

            _buttonsShouldClosePopups.Add(closePopup);

            return this;
        }

        public PYAlertPopup AddButtonAction(int index, Action action)
        {
            if (_buttons[index] is Button)
                ((Button)_buttons[index]).onClick.AddListener(() => action());
            else if (_buttons[index] is PYButton)
                ((PYButton)_buttons[index]).onClick.AddListener((sender) => action());

            return this;
        }

        public void EnableButtons(bool isEnabled)
        {
            if (_buttons == null)
                return;

            for (int x = 0; x < _buttons.Count; x++)
            {
                if (_buttons[x] is Button)
                    ((Button)_buttons[x]).interactable = isEnabled;
                else if (_buttons[x] is PYButton)
                    ((PYButton)_buttons[x]).IsEnabled = isEnabled;
            }
        }
    }
}