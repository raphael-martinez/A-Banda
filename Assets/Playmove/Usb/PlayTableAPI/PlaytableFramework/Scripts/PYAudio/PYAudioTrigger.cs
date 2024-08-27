using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Playmove
{
    public class PYAudioTrigger : MonoBehaviour
    {
        [Serializable]
        public class Trigger
        {
            public float Time;
            public Action Action;
        }

        private PYAudioSource _audioSource;
        public PYAudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GetComponent<PYAudioSource>();
                return _audioSource;
            }
        }

        //private PYAudioTags _currentAudioTag;
        [SerializeField]
        private List<Trigger> _triggersAction = new List<Trigger>();
        private List<Trigger> _queueTriggers = new List<Trigger>();

        public void Initialize(PYAudioTags audioTag)
        {
            //_currentAudioTag = audioTag;
            _triggersAction.Clear();
            _queueTriggers.Clear();

            AudioSource.OnPlaying += AudioSource_onPlaying;
            AudioSource.OnCompleted += AudioSource_onCompleted;
        }

        public PYAudioTrigger AddTriggerAction(float time, Action action)
        {
            _triggersAction.Add(new Trigger() { Time = time, Action = action });
            _queueTriggers = _triggersAction.OrderBy(t => t.Time).ToList();
            return this;
        }
        public PYAudioTrigger AddTriggerAction(Trigger trigger)
        {
            _triggersAction.Add(trigger);
            _queueTriggers = _triggersAction.OrderBy(t => t.Time).ToList();
            return this;
        }

        void AudioSource_onPlaying(PYAudioSource.PYAudioSourceEventData data)
        {
            if (_queueTriggers.Count == 0)
                return;

            if (data.Duration >= _queueTriggers[0].Time)
            {
                if (_queueTriggers[0].Action != null)
                    _queueTriggers[0].Action();

                _queueTriggers.RemoveAt(0);
            }
        }

        void AudioSource_onCompleted(PYAudioSource.PYAudioSourceEventData data)
        {
            _queueTriggers = _triggersAction.OrderBy(t => t.Time).ToList();
        }
    }
}