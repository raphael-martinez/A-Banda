using Playmove.Core.BasicEvents;
using UnityEngine;

namespace Playmove.Core
{
    public enum OpenableState
    {
        Opening,
        Opened,
        Closing,
        Closed
    }

    public abstract class Openable : MonoBehaviour
    {
        public PlaytableEvent<Openable> OnOpening = new PlaytableEvent<Openable>();
        public PlaytableEvent<Openable> OnOpened = new PlaytableEvent<Openable>();
        public PlaytableEvent<Openable> OnClosing= new PlaytableEvent<Openable>();
        public PlaytableEvent<Openable> OnClosed = new PlaytableEvent<Openable>();

        public OpenableState State { get; protected set; } = OpenableState.Closed;
        public bool IsOpen { get { return State == OpenableState.Opening || State == OpenableState.Opened; } }
        public bool IsClose { get { return State == OpenableState.Closing || State == OpenableState.Closed; } }

        public virtual void Open()
        {
            State = OpenableState.Opening;
            OnOpening.Invoke(this);
        }
        protected virtual void Opened()
        {
            State = OpenableState.Opened;
            OnOpened.Invoke(this);
        }

        public virtual void Close()
        {
            State = OpenableState.Closing;
            OnClosing.Invoke(this);
        }
        protected virtual void Closed()
        {
            State = OpenableState.Closed;
            OnClosed.Invoke(this);
        }
    }
}
