using System;
using UnityEngine.Events;

namespace Playmove.Core.BasicEvents
{
    [Serializable] public class PlaytableEvent : UnityEvent { }
    [Serializable] public class PlaytableEvent<T> : UnityEvent<T> { }
    [Serializable] public class PlaytableEventInt : UnityEvent<int> { }
    [Serializable] public class PlaytableEventFloat : UnityEvent<float> { }
    [Serializable] public class PlaytableEventString : UnityEvent<string> { }
    [Serializable] public class PlaytableEventBool : UnityEvent<bool> { }
}
