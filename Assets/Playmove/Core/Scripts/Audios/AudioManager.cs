using Playmove.Core.Bundles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Playmove.Core.Audios
{
    /// <summary>
    /// Responsible to handle audios
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static Dictionary<AudioSource, float> _audioSources = new Dictionary<AudioSource, float>();
        private static AudioManager _instance = null;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<AudioManager>();
                return _instance;
            }
        }

        private static Dictionary<AudioChannel, float> _oldChannelVolume = new Dictionary<AudioChannel, float>();

        /// <summary>
        /// Initial method to be called when wanting to play an audio or a sequence of audios
        /// You can pass only one audio or any amount you want including an array.
        /// This method will lookup for those PlayAsset at bundles and Unity Resources folder
        /// if none is found null will be returned
        /// </summary>
        /// <param name="channel">Channel where audio will be played</param>
        /// <param name="clipsAsset">Clip assets that you want to play</param>
        /// <returns>Reference to audio config to change settings or null if none is found</returns>
        public static IAudioData StartAudio(AudioChannel channel, params PlayAsset[] clipsAsset)
        {
            return StartAudio(channel, clipsAsset.Where(asset => asset != null && asset.Type == "AudioClip")
                .Select(asset => asset.AssetName).ToArray());
        }
        /// <summary>
        /// Initial method to be called when wanting to play an audio or a sequence of audios
        /// You can pass only one audio or any amount you want including an array.
        /// This method will lookup for those PlayAsset at bundles and Unity Resources folder
        /// if none is found null will be returned
        /// </summary>
        /// <param name="channel">Channel where audio will be played</param>
        /// <param name="clipsName">Clips name that you want to play</param>
        /// <returns>Reference to audio config to change settings or null if none is found</returns>
        public static IAudioData StartAudio(AudioChannel channel, params string[] clipsName)
        {
            List<AudioClip> clips = new List<AudioClip>();
            foreach (var clipName in clipsName)
            {
                AudioClip clip = Data.GetAsset<AudioClip>(clipName);
                if (clip == null)
                {
                    clip = Localization.GetAsset<AudioClip>(clipName);
                    if (clip == null)
                    {
                        clip = Content.GetAsset<AudioClip>(clipName);
                        if (clip == null)
                            clip = Content.GetAssetsFromAllContents(clipName, new List<AudioClip>()).FirstOrDefault();
                    }
                }

                if (clip == null) continue;
                clip.name = clipName;
                clips.Add(clip);
            }
            if (clips.Count == 0)
            {
                string clipName = string.Empty;
                if (clipsName != null && clipsName.Length > 0)
                    clipName = clipsName[0];
                if (string.IsNullOrEmpty(clipName))
                    clipName = "Unknow";

                Debug.LogWarning($"Couldn't find the specified audios: {clipName}");
                AudioData errorAudio = Instance.GetAudioData($"Not Found: {clipName}");
                errorAudio.Initialize(AudioChannel.Voice, new AudioClip[0]);
                Instance._executingAudios.Add(errorAudio);
                return errorAudio;
            }
            return StartAudio(channel, clips.ToArray());
        }
        /// <summary>
        /// Initial method to be called when wanting to play an audio or a sequence of audios
        /// You can pass only one audio or any amount you want including an array.
        /// </summary>
        /// <param name="channel">Channel where audio will be played</param>
        /// <param name="clips">AudioClips that you want to play</param>
        /// <returns>Reference to audio config to change settings</returns>
        public static IAudioData StartAudio(AudioChannel channel, params AudioClip[] clips)
        {
            if (channel == AudioChannel.Voice)
            {
                // Verify if the audio you want to play is not already playing
                // in case this is true and the audio is also in Voice channel
                // we return it's own reference
                AudioData currentVoice = Instance.GetExecutingAudioDataReference(clips[0]);
                if (currentVoice != null && currentVoice.Channel == AudioChannel.Voice)
                    return currentVoice;

                // Only one voice can be playing at same time
                foreach (var audio in Instance._executingAudios)
                {
                    if (audio.Channel == AudioChannel.Voice)
                    {
                        audio.Stop();
                        break;
                    }
                }
            }

            // Initialize an AudioData
            AudioData audioData = Instance.GetAudioData(clips[0].name);
            audioData.Initialize(channel, clips);
            Instance._executingAudios.Add(audioData);
            return audioData;
        }

        /// <summary>
        /// Check if a specific audioData is playing
        /// </summary>
        /// <param name="audioData">AudioData to check</param>
        /// <returns>State of the audio</returns>
        public static bool IsPlaying(AudioData audioData)
        {
            return IsPlaying(audioData.name);
        }
        /// <summary>
        /// Check if a specific clip is playing
        /// </summary>
        /// <param name="clip">Clip to check</param>
        /// <returns>State of the clip</returns>
        public static bool IsPlaying(AudioClip clip)
        {
            return IsPlaying(clip.name);
        }
        /// <summary>
        /// Check if a specific audioData is playing by PlayAsset
        /// </summary>
        /// <param name="asset">PlayAsset to check</param>
        /// <returns>State of the audio</returns>
        public static bool IsPlaying(PlayAsset asset)
        {
            return IsPlaying(asset.AssetName);
        }
        /// <summary>
        /// Check if a specific audioData is playing by name
        /// </summary>
        /// <param name="name">Audio name to check</param>
        /// <returns>State of the audio</returns>
        public static bool IsPlaying(string name)
        {
            return GetAudio(name) != null;
        }
        /// <summary>
        /// Check if the channel has any audio playing
        /// </summary>
        /// <param name="channel">Channel to check</param>
        /// <returns>State if this channel has any audio playing</returns>
        public static bool IsPlaying(AudioChannel channel)
        {
            return GetAudio(channel).Count > 0;
        }

        /// <summary>
        /// Stop a specific audioData with fade if you want
        /// </summary>
        /// <param name="audioData">AudioData to stop</param>
        /// <param name="fadeDuration">Fade duration with you want some</param>
        public static void StopAudio(AudioData audioData, float fadeDuration = 0)
        {
            StopAudio(audioData.name, fadeDuration);
        }
        /// <summary>
        /// Stop a specific clip with fade if you want
        /// </summary>
        /// <param name="clip">Clip to stop</param>
        /// <param name="fadeDuration">Fade duration with you want some</param>
        public static void StopAudio(AudioClip clip, float fadeDuration = 0)
        {
            StopAudio(clip.name, fadeDuration);
        }
        /// <summary>
        /// Stop a specific clip with fade if you want
        /// </summary>
        /// <param name="asset">PlayAsset to stop</param>
        /// <param name="fadeDuration">Fade duration with you want some</param>
        public static void StopAudio(PlayAsset asset, float fadeDuration = 0)
        {
            StopAudio(asset.AssetName);
        }
        /// <summary>
        /// Stop a specific audioData by name with fade if you want
        /// </summary>
        /// <param name="name">Audio name to stop</param>
        /// <param name="fadeDuration">Fade duration</param>
        public static void StopAudio(string name, float fadeDuration = 0)
        {
            IAudioData audio = GetAudio(name);
            if (audio != null)
                audio.Stop(fadeDuration);
        }

        /// <summary>
        /// Stop specific Channel with fade if you want
        /// </summary>
        /// <param name="channel">Channel to stop</param>
        /// <param name="fadeDuration">Fade duration</param>
        public static void StopChannel(AudioChannel channel, float fadeDuration = 0)
        {
            var audios = Instance._executingAudios;
            audios = audios.GetRange(0, audios.Count).Where(audio => audio.Channel == channel).ToList();
            foreach (var audio in audios)
                audio.Stop(fadeDuration);
        }

        /// <summary>
        /// Resume all Game Sounds
        /// </summary>
        /// <param name="channel">Channel to stop</param>
        /// <param name="fadeDuration">Fade duration</param>
        public static void ResumeGameSounds(float fadeDuration = 0)
        {
            foreach (var audio in _audioSources)
            {
                Instance.StartCoroutine(Instance.FadeVolumeRoutine(audio, 1, 5f));
            }
            _audioSources.Clear();
        }

        /// <summary>
        /// Stops all game sounds
        /// </summary>
        /// <param name="channel">Channel to stop</param>
        /// <param name="fadeDuration">Fade duration</param>
        public static void StopGameSounds(float fadeDuration = 0)
        {
            var audios = GameObject.FindObjectsOfType<AudioSource>();
            var n = 0;
            foreach (var audio in audios)
            {
                _audioSources.Add(audio, audio.volume);
                Instance.StartCoroutine(Instance.FadeVolumeRoutine(_audioSources.ElementAt(n), 0, 1f));
                n++;
            }
        }

        /// <summary>
        /// Routine to fade volume
        /// </summary>
        /// <param name="to">Target volume value</param>
        /// <param name="duration">Fade duration</param>
        /// <param name="completed">Method to be called when fade complete</param>
        /// <returns></returns>
        public IEnumerator FadeVolumeRoutine(KeyValuePair<AudioSource, float> item, float to, float duration)
        {
            float timer = 0;
            var audio = item.Key;
            var from = item.Value - to;
            while (timer < 1)
            {
                audio.volume = Mathf.Lerp(from, to, timer);
                timer += Time.deltaTime / duration;
                yield return null;
            }
            audio.volume = to;
        }

        /// <summary>
        /// Get the channel volume that goes from 0 to 1
        /// </summary>
        /// <param name="channel">Channel to get volume</param>
        /// <returns>Volume between 0 to 1</returns>
        public static float GetChannelVolume(AudioChannel channel)
        {
            Instance._mixer.GetFloat($"{channel}Volume", out float volume);
            if (volume >= 0) return 1;
            return 1 - (volume / -80f);
        }
        /// <summary>
        /// Set volume to a channel that goes from 0 to 1
        /// </summary>
        /// <param name="channel">Channel to set volume</param>
        /// <param name="volume">Volume in range of 0 to 1</param>
        public static void SetChannelVolume(AudioChannel channel, float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (!_oldChannelVolume.ContainsKey(channel))
                _oldChannelVolume.Add(channel, GetChannelVolume(channel));
            Instance._mixer.SetFloat($"{channel}Volume", -80f * (1 - volume));
        }

        /// <summary>
        /// Get mute state from channel
        /// </summary>
        /// <param name="channel">Channel to query the mute state</param>
        /// <returns>State of the channel</returns>
        public static bool GetChannelIsMute(AudioChannel channel)
        {
            return GetChannelVolume(channel) == 0;
        }
        /// <summary>
        /// Set a channel to mute or unmute
        /// </summary>
        /// <param name="channel">Channel to set mute</param>
        /// <param name="isMute">Value for mute</param>
        public static void SetChannelIsMute(AudioChannel channel, bool isMute)
        {
            float volume = 0;
            if (!isMute)
            {
                if (!_oldChannelVolume.ContainsKey(channel))
                    _oldChannelVolume.Add(channel, GetChannelVolume(channel));
                volume = _oldChannelVolume[channel];
            }

            SetChannelVolume(channel, volume);
        }

        /// <summary>
        /// Get reference from all audios that it's playing on the specified channel
        /// </summary>
        /// <param name="channel">Channel that you want the audios</param>
        /// <returns>Reference to a list of audios or empty list</returns>
        public static List<IAudioData> GetAudio(AudioChannel channel)
        {
            return Instance._executingAudios.Where(audio => audio.Channel == channel)
                .Select(audio => audio as IAudioData).ToList();
        }
        /// <summary>
        /// Get a possible reference from the audio based on it's name
        /// </summary>
        /// <param name="name">Audio name to get reference</param>
        /// <returns>Reference to audio or null</returns>
        public static IAudioData GetAudio(string name)
        {
            foreach (var audio in Instance._executingAudios)
            {
                if (audio.name == name)
                    return audio;
            }
            return null;
        }

        [SerializeField] private AudioMixer _mixer = null;

        private Queue<AudioData> _pool = new Queue<AudioData>();
        private List<AudioData> _executingAudios = new List<AudioData>();
        
        /// <summary>
        /// Get Unity's AudioMixerGroup based on AudioChannel
        /// </summary>
        /// <param name="channel">Channel</param>
        /// <returns>Unity's AudioMixerGroup based on AudioChannel</returns>
        public AudioMixerGroup GetAudioMixerGroup(AudioChannel channel)
        {
            return _mixer.FindMatchingGroups(channel.ToString())[0];
        }
        /// <summary>
        /// Gets the list of currently audios playing
        /// </summary>
        /// <returns>List with current audios playing</returns>
        public List<AudioData> GetPlayingAudios()
        {
            return _executingAudios;
        }

        /// <summary>
        /// Puts AudioData back to pool
        /// </summary>
        /// <param name="audioData">AudioData to be put on pool</param>
        public void PutAudioDataIntPool(AudioData audioData)
        {
            _executingAudios.Remove(audioData);
            audioData.name = "* " + audioData.name;
            audioData.gameObject.SetActive(false);
            _pool.Enqueue(audioData);
        }

        /// <summary>
        /// Get an AudioData from pool or creates a new one
        /// </summary>
        /// <param name="name">Audio clip name</param>
        /// <returns>AudioData</returns>
        private AudioData GetAudioData(string name)
        {
            AudioData audioData = null;
            if (_pool.Count == 0)
            {
                audioData = new GameObject(name).AddComponent<AudioData>();
                audioData.transform.SetParent(transform);
            }
            else
            {
                audioData = _pool.Dequeue();
                audioData.name = audioData.name.Replace("* ", string.Empty);
            }
            audioData.gameObject.SetActive(true);
            return audioData;
        }

        private AudioData GetExecutingAudioDataReference(AudioClip clip)
        {
            foreach (var audioData in _executingAudios.Where(audio => audio != null && audio.CurrentClip != null))
            {
                if (audioData.CurrentClip.name == clip.name)
                    return audioData;
            }
            return null;
        }
    }
}
