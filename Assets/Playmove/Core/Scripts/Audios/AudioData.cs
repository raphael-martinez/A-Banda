using Playmove.Core.BasicEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Playmove.Core.Audios
{
    public interface IAudioPlayable
    {
        AudioChannel Channel { get; }
        AudioSource AudioSource { get; }
        bool IsPlaying { get; }
        bool IsPaused { get; }

        IAudioData SetVolume(float volume);
        IAudioData SetPitch(float pitch);
        IAudioData SetLoop();
        IAudioData OnStart(UnityAction<AudioData> onStart);
        IAudioData OnResume(UnityAction<AudioData> onResume);
        IAudioData OnPause(UnityAction<AudioData> onPause);
        IAudioData OnStop(UnityAction<AudioData> onStop);
        IAudioData OnSequenceComplete(UnityAction<AudioData> onSequenceCompleted);
        IAudioData OnTimeTrigger(float time, UnityAction<AudioData> onTime);

        IAudioPlayable Play();
        void Resume();
        void Pause();
        void Stop();
        void Stop(float fadeDuration);
    }

    public interface IAudioData : IAudioPlayable
    {
        IAudioData SetDelay(float delay);
        IAudioData SetDelayBetween(float delay);
        IAudioData SetFade(float to, float duration);
    }

    /// <summary>
    /// Responsible to handle individual or sequence of audios to be played.
    /// The properties can not be changed if the audio is already playing!
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioData : MonoBehaviour, IAudioData
    {
        private PlaytableEvent<AudioData> _onStart = new PlaytableEvent<AudioData>();
        private PlaytableEvent<AudioData> _onResume = new PlaytableEvent<AudioData>();
        private PlaytableEvent<AudioData> _onPause = new PlaytableEvent<AudioData>();
        private PlaytableEvent<AudioData> _onStop = new PlaytableEvent<AudioData>();
        private PlaytableEvent<AudioData> _onSequenceCompleted = new PlaytableEvent<AudioData>();

        public AudioChannel Channel { get; private set; }
        public float Delay { get; private set; }
        public float DelayBetween { get; private set; }
        public float Duration { get; private set; }
        public float Volume
        {
            get { return AudioSource.volume; }
            private set { AudioSource.volume = value; }
        }
        public float Pitch
        {
            get { return AudioSource.pitch; }
            private set { AudioSource.pitch = value; }
        }
        public bool IsLoop
        {
            get { return AudioSource.loop; }
            private set { AudioSource.loop = value; }
        }

        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        public float CurrentTime { get; private set; }
        public AudioClip CurrentClip
        {
            get
            {
                return _currentClipIndex < _clips.Length ?
                    _clips[_currentClipIndex] : null;
            }
        }

        private float _initialVolume;
        private AudioClip[] _clips;
        private int _currentClipIndex;
        private float _delayCounter = 0;
        private bool _shouldStop = false;
        private float _fadeTo = 0;
        private float _fadeDuration = 0;
        private float _lowestTimeTrigger = 0;
        private Dictionary<float, PlaytableEvent<AudioData>> _timeTriggers = new Dictionary<float, PlaytableEvent<AudioData>>();

        private AudioSource _audioSource;
        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GetComponent<AudioSource>();
                return _audioSource;
            }
        }

        /// <summary>
        /// Initialize audio data and reset values to its default state
        /// </summary>
        /// <param name="channel">Channel where audio will be played</param>
        /// <param name="clips">All the clips to play</param>
        public void Initialize(AudioChannel channel, params AudioClip[] clips)
        {
            _onStart.RemoveAllListeners();
            _onResume.RemoveAllListeners();
            _onPause.RemoveAllListeners();
            _onStop.RemoveAllListeners();
            _onSequenceCompleted.RemoveAllListeners();
            _timeTriggers.Clear();

            AudioSource.playOnAwake = false;
            AudioSource.outputAudioMixerGroup = AudioManager.Instance.GetAudioMixerGroup(channel);

            _clips = clips;
            _currentClipIndex = 0;
            _shouldStop = false;
            _fadeTo = 0;
            _fadeDuration = 0;

            Channel = channel;
            IsPlaying = IsPaused = false;
            Delay = 0;
            DelayBetween = 0;
            IsLoop = false;
            Volume = 1;
            Pitch = 1;
        }

        /// <summary>
        /// Set delay for this audio to be played, this delay only works for the initial play
        /// if you are playing a sequence of audios this will only affect the inicial play
        /// </summary>
        /// <param name="delay">Delay value</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData SetDelay(float delay)
        {
            if (IsPlaying) return this;
            Delay = delay;
            return this;
        }
        /// <summary>
        /// Set delay between audios when playing a sequence of audios
        /// </summary>
        /// <param name="delay">Delay value</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData SetDelayBetween(float delay)
        {
            if (IsPlaying) return this;
            DelayBetween = delay;
            return this;
        }
        /// <summary>
        /// Set fadeIn on audio, this is used if you want a smooth start on musics or any other type of sounds,
        /// this will be applied to all audios that is in the sequence
        /// </summary>
        /// <param name="to">Final value you want, default and max is 1</param>
        /// <param name="duration">Fade duration</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData SetFade(float to, float duration)
        {
            if (IsPlaying) return this;
            _fadeTo = to;
            _fadeDuration = duration;
            return this;
        }

         /// <summary>
        /// Set fadeIn on audio, this is used if you want a smooth start on musics or any other type of sounds,
        /// this will be applied to all audios that is in the sequence
        /// </summary>
        /// <param name="to">Final value you want, default and max is 1</param>
        /// <param name="duration">Fade duration</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData SetFade(float duration)
        {
            if (IsPlaying) return this;
            _fadeTo = _initialVolume;
            _fadeDuration = duration;
            return this;
        }
        /// <summary>
        /// Makes the audio loop, this will not work with sequences
        /// </summary>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData SetLoop()
        {
            IsLoop = true;
            return this;
        }
        /// <summary>
        /// Set pitch for audio, this will be applied to all audios in sequences
        /// </summary>
        /// <param name="pitch">Pitch value</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData SetPitch(float pitch)
        {
            Pitch = pitch;
            return this;
        }
        /// <summary>
        /// Set volume for audio, this will be applied to all audios in sequences
        /// </summary>
        /// <param name="volume">Volume value</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData SetVolume(float volume)
        {
            Volume = volume;
            return this;
        }
        /// <summary>
        /// Event when audio starts playing, this is fired when any audio starts
        /// inclusive on sequences audios
        /// </summary>
        /// <param name="onStart">Method to be called on event</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData OnStart(UnityAction<AudioData> onStart)
        {
            _onStart.AddListener(onStart);
            return this;
        }
        /// <summary>
        /// Event when audio resume playing, this is fired when any audio resume
        /// inclusive on sequences audios
        /// </summary>
        /// <param name="onResume">Method to be called on event</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData OnResume(UnityAction<AudioData> onResume)
        {
            _onStart.AddListener(onResume);
            return this;
        }
        /// <summary>
        /// Event when audio pause playing, this is fired when any audio pause
        /// inclusive on sequences audios
        /// </summary>
        /// <param name="onPause">Method to be called on event</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData OnPause(UnityAction<AudioData> onPause)
        {
            _onStart.AddListener(onPause);
            return this;
        }
        /// <summary>
        /// Event when audio stops playing, this is fired when any audio stops
        /// inclusive on sequences audios
        /// </summary>
        /// <param name="onStop">Method to be called on event</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData OnStop(UnityAction<AudioData> onStop)
        {
            _onStop.AddListener(onStop);
            return this;
        }
        /// <summary>
        /// Event when sequence completes
        /// </summary>
        /// <param name="onSequenceCompleted">Method to be called on event</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData OnSequenceComplete(UnityAction<AudioData> onSequenceCompleted)
        {
            _onSequenceCompleted.AddListener(onSequenceCompleted);
            return this;
        }
        /// <summary>
        /// Event when audio gets to a specific point in time, this will be triggered
        /// on every audio that are on sequence
        /// </summary>
        /// <param name="time">Time to trigger</param>
        /// <param name="onTime">Method to be called on event</param>
        /// <returns>IAudioData to set more settings</returns>
        public IAudioData OnTimeTrigger(float time, UnityAction<AudioData> onTime)
        {
            if (!_timeTriggers.ContainsKey(time))
                _timeTriggers.Add(time, new PlaytableEvent<AudioData>());
            _timeTriggers[time].AddListener(onTime);
            return this;
        }

        /// <summary>
        /// Play audio
        /// </summary>
        /// <returns>IAudioPlayable reference</returns>
        public IAudioPlayable Play()
        {
            if (_clips.Length == 0)
            {
                Stop();
                return this;
            }
            if (IsPlaying) return this;
            IsPlaying = true;
            StartCoroutine(PlayRoutine());
            return this;
        }

        public void Resume()
        {
            IsPaused = false;
            AudioSource.UnPause();
            _onResume.Invoke(this);
        }

        public void Pause()
        {
            IsPaused = true;
            AudioSource.Pause();
            _onPause.Invoke(this);
        }

        /// <summary>
        /// Stop audio
        /// </summary>
        public void Stop()
        {
            if (!gameObject.activeSelf) return;

            _shouldStop = true;
            IsPlaying = false;
            AudioSource.Stop();

            Release();

            _onStop.Invoke(this);
            if (_clips.Length > 1)
                _onSequenceCompleted.Invoke(this);
        }
        /// <summary>
        /// Stop audio with a fade
        /// </summary>
        /// <param name="fadeDuration">Duration that the fade will take</param>
        public void Stop(float fadeDuration)
        {
            if (fadeDuration == 0)
            {
                Stop();
                return;
            }
            StartCoroutine(FadeVolumeRoutine(0, fadeDuration, () => Stop()));
        }

        private void PlayNext()
        {
            StartCoroutine(PlayRoutine());
        }

        /// <summary>
        /// Release this audio to AudioManager be able to reuse it
        /// </summary>
        private void Release()
        {
            IsPlaying = IsPaused = false;
            AudioManager.Instance.PutAudioDataIntPool(this);
        }

        /// <summary>
        /// Routine to handle audio playing
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlayRoutine()
        {
            _delayCounter = 0;
            CurrentTime = 0;

            while (_delayCounter < Delay)
            {
                _delayCounter += Time.deltaTime;
                yield return null;
            }
            
            // Apply fade if it has one
            if (_fadeDuration > 0)
            {
                Volume = 0;
                StartCoroutine(FadeVolumeRoutine(_fadeTo, _fadeDuration, null));
            }

            Duration = CurrentClip.length;
            AudioSource.clip = CurrentClip;
            AudioSource.Play();
            name = CurrentClip.name;
            _onStart.Invoke(this);

            if (_timeTriggers.Count > 0) _lowestTimeTrigger = _timeTriggers.Keys.Min();
            while ((CurrentTime < Duration || IsLoop) && !_shouldStop)
            {
                if (!IsPaused)
                {
                    CurrentTime += Time.deltaTime;
                    if (_timeTriggers.Count > 0 && CurrentTime >= _lowestTimeTrigger)
                    {
                        _timeTriggers[_lowestTimeTrigger].Invoke(this);
                        _timeTriggers.Remove(_lowestTimeTrigger);
                        if (_timeTriggers.Count > 0)
                            _lowestTimeTrigger = _timeTriggers.Keys.Min();
                    }
                }
                yield return null;
            }

            if (!_shouldStop)
            {
                AudioSource.Stop();

                // We have a sequence of audios to be played
                if (_clips.Length > 1)
                {
                    _currentClipIndex++;
                    if (_currentClipIndex > _clips.Length - 1)
                    {
                        yield return null;
                        Release();

                        _onStop.Invoke(this);
                        _onSequenceCompleted.Invoke(this);
                    }
                    else
                    {
                        Delay = DelayBetween;
                        PlayNext();
                    }
                }
                else
                {
                    yield return null;
                    Release();

                    _onStop.Invoke(this);
                }
            }
        }
        /// <summary>
        /// Routine to fade volume
        /// </summary>
        /// <param name="to">Target volume value</param>
        /// <param name="duration">Fade duration</param>
        /// <param name="completed">Method to be called when fade complete</param>
        /// <returns></returns>
        private IEnumerator FadeVolumeRoutine(float to, float duration, Action completed)
        {
            float timer = 0;
            _initialVolume = Volume;
            while (timer < 1)
            {
                Volume = Mathf.Lerp(_initialVolume, to, timer);
                timer += Time.deltaTime / duration;
                yield return null;
            }
            Volume = to;
            completed?.Invoke();
        }
    }
}
