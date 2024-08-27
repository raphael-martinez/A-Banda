using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

/// <summary>
/// Classe responsavel em delegar estados, sendo esses Abrindo, Aberto, Fechando e Fechado.
/// Geralmente usada para popups e cenas.
/// </summary>
namespace Playmove
{
    public class PYOpenable : MonoBehaviour
    {
        public class PYOpenableEventArgs : EventArgs
        {
            public OpenableState State;
        }

        public enum OpenableState
        {
            Opening,
            Opened,
            Closing,
            Closed
        }
        [Header("PYOpenable")]
        public OpenableState State = OpenableState.Closed;
        public bool DeactiveOnClosed = true;

        [Serializable]
        public class OpenableEvent : UnityEvent { }
        public OpenableEvent onOpening = new OpenableEvent();
        public OpenableEvent onOpened = new OpenableEvent();
        public OpenableEvent onClosing = new OpenableEvent();
        public OpenableEvent onClosed = new OpenableEvent();

        protected Action _callback;

        /// <summary>
        /// Irá abrir estando no Fechada, irá alterar o estado e lançar o evento.
        /// </summary>
        public virtual void Open()
        {
            if (State != OpenableState.Closed) return;

            if (DeactiveOnClosed)
                gameObject.SetActive(true);

            State = OpenableState.Opening;
            onOpening.Invoke();
        }

        /// <summary>
        /// Irá abrir se estiver no estado Fechado, alterar o estado e lançar o evento.
        /// </summary>
        /// <param name="callback">Ação a ser executada quando entrar no estado aberto</param>
        public virtual void Open(Action callback)
        {
            if (State != OpenableState.Closed) return;

            _callback = callback;
            Open();
        }
        /// <summary>
        /// Forçar estado aberto lançando evento.
        /// </summary>
        public virtual void HardOpen()
        {
            State = OpenableState.Opening;
            onOpening.Invoke();
        }

        protected virtual void Opened()
        {
            State = OpenableState.Opened;

            if (_callback != null) _callback();
            _callback = null;

            onOpened.Invoke();
        }

        /// <summary>
        /// Irá fechar se estiver no estado Abrir, alterar o estado e lançar o evento.
        /// </summary>
        public virtual void Close()
        {
            if (State != OpenableState.Opened) return;

            State = OpenableState.Closing;
            onClosing.Invoke();           
        }

        /// <summary>
        /// Irá fechar se estiver no estado Abrir, alterar o estado e lançar o evento.
        /// </summary>
        /// <param name="callback">Ação a ser executada quando entrar no estado fechado</param>
        public virtual void Close(Action callback)
        {
            if (State != OpenableState.Opened)
            {
                _callback?.Invoke();
                return;
            }

            _callback = callback;
            Close();
            
        }
        /// <summary>
        /// Forçar estado fechado lançando evento.
        /// </summary>
        public virtual void HardClose()
        {
            State = OpenableState.Closing;
            onClosing.Invoke();
        }

        protected virtual void Closed()
        {
            State = OpenableState.Closed;

           if (_callback != null) _callback();
            _callback = null;

            onClosed.Invoke();

            if (DeactiveOnClosed)
                gameObject.SetActive(false);
        }
    }
}