using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Playmove
{
    public class PYAudioManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        private static PYAudioManager _instance;
        public static PYAudioManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PYAudioManager>();
                return _instance;
            }
        }

        private static float _globalVolume = 1;
        public static float GlobalVolume
        {
            get { return _globalVolume; }
            set
            {
                _globalVolume = value;
                AudioListener.volume = _globalVolume;
            }
        }

        public AudioMixer Mixer;

        public PYAudioTags PreviousVoice = PYAudioTags.None;

        [System.Serializable]
        public class AudioTrack
        {
            public string Tag;
            // Group Data
            public PYGroupTag GroupTag;
            // Path to audio in resources folder
            public string PathToClip;

            private AudioClip _clip;
            public AudioClip Clip
            {
                get
                {
                    if (_clip == null && !string.IsNullOrEmpty(PathToClip))
                        _clip = Resources.Load<AudioClip>(PathToClip);

                    return _clip;
                }
                private set
                {
                    _clip = value;
                }
            }

            public AudioTrack(string tag, PYGroupTag groupTag, string pathToClip)
            {
                Tag = tag;
                GroupTag = groupTag;
                PathToClip = pathToClip;
            }
            public AudioTrack(PYGroupTag groupTag, AudioClip clip)
            {
                GroupTag = groupTag;
                Clip = clip;
            }
        }

        [System.Serializable]
        public class AudioGroup
        {
            public PYGroupTag Group;
            public float Volume = 1;
            public bool Mute;

            public AudioGroup(PYGroupTag group)
            {
                Group = group;
                Volume = 1;
                Mute = false;
            }
        }

        [SerializeField, FormerlySerializedAs("_keys")]
        private List<PYGroupTag> _groupKeys = new List<PYGroupTag>();
        [SerializeField, FormerlySerializedAs("_values")]
        private List<AudioGroup> _groupValues = new List<AudioGroup>();
        public Dictionary<PYGroupTag, AudioGroup> Groups { get; set; }

        [SerializeField]
        private List<AudioTrack> _audios = new List<AudioTrack>();
        public List<AudioTrack> Audios
        {
            get { return _audios; }
        }

        private Queue<PYAudioSource> _poolAudioSource = new Queue<PYAudioSource>();
        private List<PYAudioSource> _executingAudioSources = new List<PYAudioSource>();

        #region Unity Functions
        void Awake()
        {
            _poolAudioSource = new Queue<PYAudioSource>();
            _executingAudioSources = new List<PYAudioSource>();
        }

        public void OnAfterDeserialize()
        {
            // Groups
            Groups = new Dictionary<PYGroupTag, AudioGroup>();
            for (int i = 0; i != Mathf.Min(_groupKeys.Count, _groupValues.Count); i++)
            {
                if (Groups.ContainsKey(_groupKeys[i]))
                    continue;
                Groups.Add(_groupKeys[i], _groupValues[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            // Groups
            _groupKeys.Clear();
            _groupValues.Clear();
            foreach (var keyValue in Groups)
            {
                _groupKeys.Add(keyValue.Key);
                _groupValues.Add(keyValue.Value);
            }
        }
        #endregion

        #region StartAudio, Pause, Resume, Stop/StopGroup, ...
        /// <summary>
        /// Use this to play Resource audios
        /// </summary>
        /// <param name="audioTag">Use PYAudioTags that dont start with a B_</param>
        /// <returns></returns>
        public PYAudioSource StartAudio(PYAudioTags audioTag)
        {
            PYAudioSource audioSource = GetPYAudioSource(audioTag);
            if (audioSource != null && audioSource.Track.GroupTag == PYGroupTag.Voice)
                return audioSource;

            AudioTrack track = (audioTag == PYAudioTags.None) ?
                new AudioTrack(audioTag.ToString(), PYGroupTag.Master, string.Empty) : GetAudioTrack(audioTag);

            return StartAudio(audioTag, track);
        }
        /// <summary>
        /// Use this to play bundleAudios from Localization folder
        /// </summary>
        /// <param name="audioTag">Use PYAudioTags that start with a B_</param>
        /// <param name="group">In which group/channel this audio will be played</param>
        /// <returns></returns>
        public PYAudioSource StartAudio(PYAudioTags audioTag, PYGroupTag group)
        {
            if (audioTag.ToString().StartsWith("B_"))
                return StartAudio(PYBundleManager.Localization, audioTag, group);
            else
                return StartAudio(audioTag, new AudioTrack(audioTag.ToString(), group, GetAudioTrack(audioTag).PathToClip));
        }
        /// <summary>
        /// Use this when you need to play a AudioClip inside a group/channel
        /// </summary>
        /// <param name="audioClip">AudioClip to be played</param>
        /// <param name="group">In which group/channel this audio will be played</param>
        /// <returns></returns>
        public PYAudioSource StartAudio(AudioClip audioClip, PYGroupTag group)
        {
            PYAudioTags audioTag = (PYAudioTags)ConvertNameToTag(audioClip.name);

            PYAudioSource audioSource = GetPYAudioSource(audioTag);
            if (audioSource != null && audioSource.Track.GroupTag == PYGroupTag.Voice)
                return audioSource;

            return StartAudio(audioTag, new AudioTrack(group, audioClip));
        }
        /// <summary>
        /// Use this to play audios from a specific bundleManager
        /// </summary>
        /// <param name="bundleManager">From which bundleManager the audio will be fetch</param>
        /// <param name="audioTag">Use PYAudioTags that start with a B_</param>
        /// <param name="group">In which group/channel this audio will be played</param>
        /// <returns></returns>
        public PYAudioSource StartAudio(PYBundleSubManager bundleManager, PYAudioTags audioTag, PYGroupTag group)
        {
            PYAudioSource audioSource = GetPYAudioSource(audioTag);
            if (audioSource != null && audioSource.Track.GroupTag == PYGroupTag.Voice)
                return audioSource;

            return StartAudio(audioTag, new AudioTrack(group, bundleManager.GetAsset<AudioClip>(audioTag.ToString().Replace("B_", ""))));
        }

        // Base method for others implementations
        private PYAudioSource StartAudio(PYAudioTags audioTag, AudioTrack track)
        {
            PYAudioSource audioSource = CreatePYAudioSource(audioTag.ToString());
            audioSource.Initialize(audioTag, track);

            if (_audioTriggers.ContainsKey(audioSource.AudioTag))
            {
                foreach (PYAudioTrigger.Trigger trigger in _audioTriggers[audioSource.AudioTag])
                    audioSource.AudioTrigger.AddTriggerAction(trigger);
            }

            if (_audioSourceActions.ContainsKey(audioSource.AudioTag))
            {
                foreach (PYAudioSource.EventListenerData listener in _audioSourceActions[audioSource.AudioTag])
                    audioSource.AddListenerToEvent(listener);
            }

            return audioSource;
        }

        [Obsolete("Instead use method StartAudio(PYBundleManager, PYAudioTag, PYGroupTag")]
        public PYAudioSource StartBundleAudio(PYAudioTags audioTag, PYGroupTag group)
        {
            if (audioTag.ToString().StartsWith("B_"))
                return StartBundleAudioClip(PYBundleManager.Localization.GetAsset<AudioClip>(audioTag.ToString().Replace("B_", "")), audioTag, group);

            return StartAudio(audioTag);
        }
        private PYAudioSource StartBundleAudioClip(AudioClip audioClip, PYAudioTags audioTag, PYGroupTag group)
        {
            PYAudioSource audioSource = GetPYAudioSource(audioTag);
            if (audioSource != null && audioSource.Track.GroupTag == PYGroupTag.Voice)
                return audioSource;

            return StartAudio(audioTag, new AudioTrack(group, audioClip));
        }

        public void Pause(PYAudioTags audio)
        {
            PYAudioSource audioSource = GetPYAudioSource(audio);
            if (audioSource == null)
                return;

            audioSource.Pause();
        }
        public void Pause(AudioClip audioClip)
        {
            Pause((PYAudioTags)ConvertNameToTag(audioClip.name));
        }

        public void Resume(PYAudioTags audio)
        {
            PYAudioSource audioSource = GetPYAudioSource(audio);
            if (audioSource == null)
                return;

            audioSource.Resume();
        }
        public void Resume(AudioClip audioClip)
        {
            Resume((PYAudioTags)ConvertNameToTag(audioClip.name));
        }

        public void Stop(PYAudioTags audio)
        {
            PYAudioSource audioSource = GetPYAudioSource(audio);
            if (audioSource == null)
                return;

            audioSource.Stop();
        }
        public void Stop(PYAudioTags audio, float fadeDuration)
        {
            PYAudioSource source = GetPYAudioSource(audio);
            if (source == null)
                return;

            source.Stop(fadeDuration);
        }
        public void Stop(AudioClip audioClip)
        {
            Stop((PYAudioTags)ConvertNameToTag(audioClip.name));
        }
        public void Stop(AudioClip audioClip, float fadeDuration)
        {
            Stop((PYAudioTags)ConvertNameToTag(audioClip.name), fadeDuration);
        }

        public void StopGroup(PYGroupTag group, float duration = 0)
        {
            foreach (PYAudioSource item in _executingAudioSources.GetRange(0, _executingAudioSources.Count))
            {
                if (item.Track.GroupTag == group)
                {
                    if (duration > 0)
                        Stop(item.AudioTag, duration);
                    else
                        Stop(item.AudioTag);
                }
            }
        }

        public bool IsPlaying(PYAudioTags audioTag)
        {
            return _executingAudioSources.Find((data) => data.AudioTag == audioTag) != null;
        }
        public bool IsPlaying(AudioClip audioClip)
        {
            if (audioClip == null)
                return false;

            return IsPlaying((PYAudioTags)ConvertNameToTag(audioClip.name));
        }

        public bool IsMute(PYGroupTag groupTag)
        {
            return Groups[groupTag].Mute;
        }

        public void MuteGroup(PYGroupTag group, bool mute = true)
        {
            Groups[group].Mute = mute;

            if (_executingAudioSources == null || _executingAudioSources.Count == 0)
                return;

            for (int x = 0; x < _executingAudioSources.Count; x++)
            {
                if (_executingAudioSources[x].Track.GroupTag == group)
                    _executingAudioSources[x].Mute = mute;
            }
        }

        public float GetGroupVolume(PYGroupTag group)
        {
            return Groups[group].Volume;
        }
        public void SetGroupVolume(PYGroupTag group, float volume)
        {
            Groups[group].Volume = volume;

            if (_executingAudioSources == null || _executingAudioSources.Count == 0)
                return;

            for (int x = 0; x < _executingAudioSources.Count; x++)
            {
                if (_executingAudioSources[x].Track.GroupTag == group)
                    _executingAudioSources[x].Source.volume = volume;
            }
        }
        #endregion

        #region Listen to Events
        // PYAudioTrigger
        private Dictionary<PYAudioTags, List<PYAudioTrigger.Trigger>> _audioTriggers = new Dictionary<PYAudioTags, List<PYAudioTrigger.Trigger>>();
        public PYAudioTrigger.Trigger AddTriggerAction(PYAudioTags audioTag, float time, Action action)
        {
            if (!_audioTriggers.ContainsKey(audioTag))
                _audioTriggers.Add(audioTag, new List<PYAudioTrigger.Trigger>());
            _audioTriggers[audioTag].Add(new PYAudioTrigger.Trigger() { Time = time, Action = action });

            // Caso o audio já esteja rodando adicionamos o trigger nele
            PYAudioSource audioSource = GetPYAudioSource(audioTag);
            if (audioSource)
                audioSource.AudioTrigger.AddTriggerAction(time, action);

            return _audioTriggers[audioTag][_audioTriggers[audioTag].Count - 1];
        }
        public PYAudioTrigger.Trigger AddTriggerAction(AudioClip audioClip, float time, Action action)
        {
            return AddTriggerAction((PYAudioTags)ConvertNameToTag(audioClip.name), time, action);
        }

        public bool RemoveTriggerAction(PYAudioTags audioTag, PYAudioTrigger.Trigger trigger)
        {
            if (_audioTriggers.ContainsKey(audioTag))
                return _audioTriggers[audioTag].Remove(trigger);
            else
                return false;
        }
        public bool RemoveTriggerAction(AudioClip audioClip, PYAudioTrigger.Trigger trigger)
        {
            if (!audioClip)
                return false;

            return RemoveTriggerAction((PYAudioTags)ConvertNameToTag(audioClip.name), trigger);
        }

        // PYAudioSource
        private Dictionary<PYAudioTags, List<PYAudioSource.EventListenerData>> _audioSourceActions = new Dictionary<PYAudioTags, List<PYAudioSource.EventListenerData>>();
        public PYAudioSource.EventListenerData AddListenerPYAudio(PYAudioTags audioTag, PYAudioSource.EventsType type, Action<PYAudioSource.PYAudioSourceEventData> action)
        {
            if (!_audioSourceActions.ContainsKey(audioTag))
                _audioSourceActions.Add(audioTag, new List<PYAudioSource.EventListenerData>());
            _audioSourceActions[audioTag].Add(new PYAudioSource.EventListenerData() { Type = type, Action = action });

            // Caso o audio já esteja rodando adicionamos a ação nele
            PYAudioSource audioSource = GetPYAudioSource(audioTag);
            if (audioSource)
                audioSource.AddListinerToEvent(type, action);

            return _audioSourceActions[audioTag][_audioSourceActions[audioTag].Count - 1];
        }
        public PYAudioSource.EventListenerData AddListenerPYAudio(AudioClip audioClip, PYAudioSource.EventsType type, Action<PYAudioSource.PYAudioSourceEventData> action)
        {
            return AddListenerPYAudio((PYAudioTags)ConvertNameToTag(audioClip.name), type, action);
        }

        public bool RemoveListenerPYAudio(PYAudioTags audioTag, PYAudioSource.EventListenerData listener)
        {
            if (_audioSourceActions.ContainsKey(audioTag))
                return _audioSourceActions[audioTag].Remove(listener);
            else
                return false;
        }
        public bool RemoveListenerPYAudio(AudioClip audioClip, PYAudioSource.EventListenerData listener)
        {
            if (!audioClip)
                return false;

            return RemoveListenerPYAudio((PYAudioTags)ConvertNameToTag(audioClip.name), listener);
        }
        #endregion

        public void AddInExecutingList(PYAudioSource data)
        {
            _executingAudioSources.Add(data);
        }

        public void PutInPool(PYAudioSource data)
        {
            if (_poolAudioSource.Contains(data))
                return;

            StartCoroutine(PutInPoolRoutine(data));
        }
        private System.Collections.IEnumerator PutInPoolRoutine(PYAudioSource data)
        {
            yield return new WaitForEndOfFrame();

            if (data.IsPlaying || data.IsPaused)
                data.Stop();

            data.name = "* " + data.name;
            _executingAudioSources.Remove(data);
            _poolAudioSource.Enqueue(data);
        }

        public PYAudioSource GetPYAudioSource(PYAudioTags audioTag)
        {
            for (int x = 0; x < _executingAudioSources.Count; x++)
            {
                if (_executingAudioSources[x].AudioTag == audioTag)
                    return _executingAudioSources[x];
            }
            return null;
        }
        public PYAudioSource GetPYAudioSource(AudioClip audioClip)
        {
            return GetPYAudioSource((PYAudioTags)ConvertNameToTag(audioClip.name));
        }

        private AudioTrack GetAudioTrack(PYAudioTags audioTag)
        {
            return Audios.Find(item => item.Tag == audioTag.ToString());
        }

        private PYAudioSource CreatePYAudioSource(string name)
        {
            PYAudioSource source = null;
            if (_poolAudioSource.Count > 0)
                source = _poolAudioSource.Dequeue();

            // Verifica pare ter certeza de que o aúdio removido do pool
            // realmente está parado, antes de reusá-lo, caso ele
            // ainda esteja sendo utilizado criamos um novo PYAudioSource
            if (source == null || source.IsPlaying || source.IsPaused)
            {
                GameObject clone = new GameObject();
                clone.transform.parent = transform;
                clone.transform.localPosition = Vector3.zero;
                source = clone.AddComponent<PYAudioSource>();
            }

            source.name = string.Format("PYAudio ({0})", name);
            return source;
        }

        public int ConvertNameToTag(string name)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(name);
            double finalNumber = bytes[0];
            for (int i = 1; i < bytes.Length; i++)
                finalNumber += bytes[i] * i;

            return (int)(finalNumber * 0.498989f);
        }
    }
}