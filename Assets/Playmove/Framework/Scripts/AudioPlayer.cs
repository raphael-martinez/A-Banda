using Playmove.Core.Audios;
using Playmove.Core.Bundles;
using System;
using System.Linq;
using UnityEngine;

namespace Playmove.Framework
{
    public class AudioPlayer : MonoBehaviour
    {
        [Serializable]
        public class Audio
        {
            public string Tag = string.Empty;
            public AudioChannel Channel = AudioChannel.Master;
            [Header("Only one will be used. AudioClip has more priority")]
            public AudioClip AudioClip = null;
            public PlayAsset AudioAsset = null;
            public string AudioName = string.Empty;
        }

        [SerializeField] bool _playOnEnable = false;
        [SerializeField] bool _playOnStart = false;

        [Header("Audio Settings")]
        [SerializeField] float _volume = 1;
        [SerializeField] float _pitch = 1;
        [SerializeField] float _delay = 0;
        [SerializeField] float _delayBetween = 0;
        [SerializeField] bool _loop = false;
        [SerializeField] float _fadeTo = 0;
        [SerializeField] float _fadeDuration = 0;

        [Header("Audios")]
        [SerializeField] Audio[] _audios = default;

        private void OnEnable()
        {
            if (_playOnEnable)
                Play();
        }
        private void Start()
        {
            if (_playOnStart)
                Play();
        }

        public void PlayInSequence()
        {
            if (_audios.Length == 0) return;
            if (_audios[0].AudioClip)
            {
                ApplySettings(AudioManager.StartAudio(_audios[0].Channel, _audios.Where(audio => audio.AudioAsset)
                    .Select(audio => audio.AudioClip).ToArray())).Play();
            }
            else if (_audios[0].AudioAsset)
            {
                ApplySettings(AudioManager.StartAudio(_audios[0].Channel, _audios.Where(audio => audio.AudioAsset)
                    .Select(audio => audio.AudioAsset).ToArray())).Play();
            }
            else if (!string.IsNullOrEmpty(_audios[0].AudioName))
            {
                ApplySettings(AudioManager.StartAudio(_audios[0].Channel, _audios.Where(audio => audio.AudioAsset)
                    .Select(audio => audio.AudioName).ToArray())).Play();
            }
        }
        public void Play()
        {
            if (_audios.Length == 0) return;
            Play(_audios[0].Tag);
        }
        public void Play(string tag)
        {
            Audio audio = _audios.FirstOrDefault(a => a.Tag == tag);
            if (audio == null) return;
            Play(audio);
        }

        private void Play(Audio audio)
        {
            if (audio.AudioClip)
                ApplySettings(AudioManager.StartAudio(audio.Channel, audio.AudioClip)).Play();
            else if (audio.AudioAsset)
                ApplySettings(AudioManager.StartAudio(audio.Channel, audio.AudioAsset)).Play();
            else if (!string.IsNullOrEmpty(audio.AudioName))
                ApplySettings(AudioManager.StartAudio(audio.Channel, audio.AudioName)).Play();
        }

        IAudioData ApplySettings(IAudioData audioData)
        {
            if (_loop) audioData.SetLoop();
            return audioData.SetVolume(_volume).SetPitch(_pitch)
                .SetDelay(_delay).SetDelayBetween(_delayBetween)
                .SetFade(_fadeTo, _fadeDuration);
        }
    }
}
