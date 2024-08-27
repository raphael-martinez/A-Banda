using UnityEngine;
using System.Collections;

namespace Playmove
{
    [RequireComponent(typeof(AudioSource), typeof(PYAudioTrigger))]
    public class PYAudioSource : MonoBehaviour
    {
        public class EventListenerData
        {
            public EventsType Type = EventsType.OnStartPlaying;
            public System.Action<PYAudioSourceEventData> Action;
        }

        public enum EventsType
        {
            OnStartPlaying,
            OnPlaying,
            OnResume,
            OnPaused,
            OnCompleted,
            OnStopped
        }

        public class PYAudioSourceEventData
        {
            public PYAudioSource AudioSource;
            public float Duration;
            public float Length;
        }

        [System.Serializable]
        public class FadeData
        {
            public float From;
            public float To;
            public float Duration;
            public bool HasFader;

            public FadeData(float from, float to, float duration)
            {
                From = from;
                To = to;
                Duration = duration;
                HasFader = true;
            }

            public FadeData()
            {
                HasFader = false;
                From = 0;
                To = 0;
                Duration = 0;
            }
        }

        public PYAudioTags AudioTag;

        [SerializeField]
        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set { _isPlaying = value; }
        }

        [SerializeField]
        private bool _isPaused = false;
        public bool IsPaused
        {
            get { return _isPaused; }
            private set { _isPaused = value; }
        }

        private AudioSource _source;
        public AudioSource Source
        {
            get { return _source; }
        }

        private PYAudioTrigger _audioTrigger;
        public PYAudioTrigger AudioTrigger
        {
            get
            {
                if (_audioTrigger == null)
                    _audioTrigger = GetComponent<PYAudioTrigger>();
                return _audioTrigger;
            }
        }

        public PYAudioManager.AudioGroup Group;
        public PYAudioManager.AudioTrack Track;

        public bool Mute
        {
            get { return _source.mute; }
            set { _source.mute = value; }
        }

        [SerializeField]
        private float _duration = 0;
        [SerializeField]
        private float _delay = 0;
        [SerializeField]
        private bool _loop = false;

        [SerializeField]
        private FadeData _fadeData = null;

        private bool _isStoppingWithFade = false;

        #region Events
        private System.Action<PYAudioSourceEventData> _onStartPlaying;
        public System.Action<PYAudioSourceEventData> OnStartPlaying
        {
            get { return _onStartPlaying; }
            set { _onStartPlaying = value; }
        }

        private System.Action<PYAudioSourceEventData> _onPlaying;
        public System.Action<PYAudioSourceEventData> OnPlaying
        {
            get { return _onPlaying; }
            set { _onPlaying = value; }
        }

        private System.Action<PYAudioSourceEventData> _onResume;
        public System.Action<PYAudioSourceEventData> OnResume
        {
            get { return _onResume; }
            set { _onResume = value; }
        }

        private System.Action<PYAudioSourceEventData> _onPaused;
        public System.Action<PYAudioSourceEventData> OnPaused
        {
            get { return _onPaused; }
            set { _onPaused = value; }
        }

        private System.Action<PYAudioSourceEventData> _onCompleted;
        public System.Action<PYAudioSourceEventData> OnCompleted
        {
            get { return _onCompleted; }
            set { _onCompleted = value; }
        }

        private System.Action<PYAudioSourceEventData> _onStopped;
        public System.Action<PYAudioSourceEventData> OnStopped
        {
            get { return _onStopped; }
            set { _onStopped = value; }
        }

        private System.Action<PYAudioSourceEventData> _onCompletedByPlay;
        private System.Action<PYAudioSourceEventData> OnCompletedByPlay
        {
            get { return _onCompletedByPlay; }
            set { _onCompletedByPlay = value; }
        }
        #endregion

        public void Initialize(PYAudioTags audioTag, PYAudioManager.AudioTrack track)
        {
            AudioTag = audioTag;
            Track = track;
            Group = PYAudioManager.Instance.Groups[Track.GroupTag];

            if (_source == null)
                _source = GetComponent<AudioSource>();

            _source.playOnAwake = false;
            _source.clip = Track.Clip;
            _source.loop = false;
            _source.mute = Group.Mute;
            _source.volume = Group.Volume;
            _source.pitch = 1;
            _source.outputAudioMixerGroup = PYAudioManager.Instance.Mixer.FindMatchingGroups(Track.GroupTag.ToString())[0];

            if (_source.clip != null)
                _duration = _source.clip.length;

            _delay = 0;
            _loop = false;
            IsPlaying = IsPaused = false;
            _isStoppingWithFade = false;

            OnStartPlaying = null;
            OnPlaying = null;
            OnResume = null;
            OnPaused = null;
            OnCompleted = null;
            OnStopped = null;

            AudioTrigger.Initialize(audioTag);
        }

        public PYAudioSource AddListinerToEvent(EventsType type, System.Action<PYAudioSourceEventData> action)
        {
            switch (type)
            {
                case EventsType.OnStartPlaying:
                    OnStartPlaying += action;
                    break;
                case EventsType.OnPlaying:
                    OnPlaying += action;
                    break;
                case EventsType.OnResume:
                    OnResume += action;
                    break;
                case EventsType.OnPaused:
                    OnPaused += action;
                    break;
                case EventsType.OnCompleted:
                    OnCompleted += action;
                    break;
                case EventsType.OnStopped:
                    OnStopped += action;
                    break;
            }

            return this;
        }
        public PYAudioSource AddListenerToEvent(EventListenerData data)
        {
            return AddListinerToEvent(data.Type, data.Action);
        }
        public PYAudioSource RemoveListinerFromEvent(EventsType type, System.Action<PYAudioSourceEventData> action)
        {
            switch (type)
            {
                case EventsType.OnStartPlaying:
                    OnStartPlaying -= action;
                    break;
                case EventsType.OnPlaying:
                    OnPlaying -= action;
                    break;
                case EventsType.OnResume:
                    OnResume -= action;
                    break;
                case EventsType.OnPaused:
                    OnPaused -= action;
                    break;
                case EventsType.OnCompleted:
                    OnCompleted -= action;
                    break;
                case EventsType.OnStopped:
                    OnStopped -= action;
                    break;
            }

            return this;
        }

        public PYAudioSource Volume(float volume, bool ignoreGroupVolume = false)
        {
            _source.volume = (ignoreGroupVolume ? 1 : Group.Volume) * volume;
            return this;
        }

        public PYAudioSource Pitch(float pitch)
        {
            _source.pitch = pitch;
            return this;
        }

        public PYAudioSource Delay(float delay)
        {
            _delay = delay;
            return this;
        }

        public PYAudioSource Repeat(int repeatAmount)
        {
            _duration *= (repeatAmount + 1);
            _loop = false;
            _source.loop = repeatAmount > 0;
            return this;
        }

        public PYAudioSource Loop()
        {
            return Loop(true);
        }
        public PYAudioSource Loop(bool hasLoop)
        {
            _source.loop = _loop = hasLoop;
            return this;
        }

        public PYAudioSource Fade(float from, float to, float duration = 0.1f)
        {
            if (from > Group.Volume)
                from = Group.Volume;
            if (to > Group.Volume)
                to = Group.Volume;

            _fadeData = new FadeData(from, to, duration);
            _fadeData.HasFader = true;
            return this;
        }

        public void Play()
        {
            Play(true);
        }
        public void Play(bool interruptVoice)
        {
            if (Track.GroupTag == PYGroupTag.Voice && IsPlaying)
                return;

            StopAllCoroutines();

            IsPlaying = true;
            IsPaused = false;
            PYAudioManager.Instance.AddInExecutingList(this);
            StartCoroutine(PlayRoutine(_delay, _duration, interruptVoice));

            SendEvent(OnStartPlaying);
        }
        public void Play(System.Action<PYAudioSourceEventData> completedCallback)
        {
            OnCompletedByPlay = completedCallback;
            Play();
        }

        public void Resume()
        {
            _source.Play();
            IsPlaying = true;
            IsPaused = false;

            SendEvent(OnResume);
        }

        public void Pause()
        {
            _source.Pause();
            IsPlaying = false;
            IsPaused = true;

            SendEvent(OnPaused);
        }

        public void Stop()
        {
            // We can call stop just one time
            if (!IsPlaying && !IsPaused)
            {
                OnCompletedByPlay = null;
                _isStoppingWithFade = false;
                if (_source.isPlaying)
                {
                    StopAllCoroutines();
                    _source.Stop();
                }
                PYAudioManager.Instance.PutInPool(this);
                return;
            }

            OnCompletedByPlay = null;

            StopAllCoroutines();
            _isStoppingWithFade = false;
            _source.Stop();
            PYAudioManager.Instance.PutInPool(this);

            IsPlaying = IsPaused = false;
            SendEvent(OnStopped);
        }
        public void Stop(float fadeDuration)
        {
            if (_isStoppingWithFade)
                return;
            _isStoppingWithFade = true;

            StopAllCoroutines();
            StartCoroutine(FadeRoutine(_source.volume, 0, fadeDuration, Stop));
        }

        private IEnumerator PlayRoutine(float delay, float duration, bool interruptVoice)
        {
            while (delay > 0)
            {
                if (IsPlaying)
                    delay -= Time.deltaTime;
                yield return null;
            }

            if (Track.GroupTag == PYGroupTag.Voice)
            {
                if (interruptVoice && PYAudioManager.Instance.PreviousVoice != AudioTag)
                    PYAudioManager.Instance.Stop(PYAudioManager.Instance.PreviousVoice);
                PYAudioManager.Instance.PreviousVoice = AudioTag;
            }

            if (IsPlaying)
                _source.Play();

            if (_fadeData != null &&
                _fadeData.HasFader)
            {
                StartCoroutine(FadeRoutine(_fadeData.From, _fadeData.To, _fadeData.Duration, null));
            }

            while (duration > 0)
            {
                if (IsPlaying)
                {
                    duration -= Time.deltaTime;
                    SendEvent(OnPlaying, _duration - duration);
                }
                yield return null;
            }

            if (!_loop)
            {
                PYAudioManager.Instance.PutInPool(this);
            }

            SendEvent(OnCompleted, _duration - duration);

            if (OnCompletedByPlay != null)
            {
                OnCompletedByPlay(new PYAudioSourceEventData() { AudioSource = this, Duration = _duration - duration, Length = _duration });
                OnCompletedByPlay = null;
            }

            IsPlaying = IsPaused = false;
        }

        private System.Collections.IEnumerator FadeRoutine(float from, float to, float duration, System.Action callback)
        {
            float timer = 0;
            while ((timer < 1) && (duration > 0))
            {
                _source.volume = Mathf.Lerp(from, to, timer);
                timer += Time.deltaTime / duration;
                yield return null;
            }

            _source.volume = to;
            _fadeData = null;
            if (callback != null) callback();
        }

        private void SendEvent(System.Action<PYAudioSourceEventData> ev)
        {
            SendEvent(ev, 0);
        }
        private void SendEvent(System.Action<PYAudioSourceEventData> ev, float duration)
        {
            if (ev != null)
                ev(new PYAudioSourceEventData() { AudioSource = this, Duration = duration, Length = _duration });
        }
    }
}