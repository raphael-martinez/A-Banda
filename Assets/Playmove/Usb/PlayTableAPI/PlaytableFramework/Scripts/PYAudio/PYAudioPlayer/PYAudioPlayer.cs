using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System;

using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PYAudioType
{
    Tag,
    Clip
}

namespace Playmove
{
    public class PYAudioPlayer : PYComponentBundle, ISerializationCallbackReceiver
    {
        public bool PlayOnEnable = false;
        public bool PlayOnStart = false;
        public float DelayStartEnable = 0;

        public List<PYPlayer> Audios = new List<PYPlayer>();

        #region Serializations
        private const int AUDIO_TAG_ID = 0;
        private const int AUDIO_CLIP_ID = 1;

        [SerializeField]
        private List<PYPlayerTag> _audiosTags = new List<PYPlayerTag>();
        [SerializeField]
        private List<PYPlayerClip> _audiosClip = new List<PYPlayerClip>();
        [SerializeField]
        private List<int> _audiosSequence = new List<int>();
        #endregion

        #region Unity
        void OnEnable()
        {
            if (PlayOnEnable)
                PlayAudioAutomaticEnableStart();
        }

        void Start()
        {
            if (PlayOnStart)
                PlayAudioAutomaticEnableStart();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (PYBundleManager.Instance != null)
            {
                if (PlayOnEnable)
                    PYBundleManager.Instance.onLoadCompleted.RemoveListener(PlayDefaultAudioWhenBundleLoads);
                if (PlayOnStart)
                    PYBundleManager.Instance.onLoadCompleted.RemoveListener(PlayDefaultAudioWhenBundleLoads);
            }
        }

        public void OnAfterDeserialize()
        {
            Audios.Clear();
            for (int x = 0; x < _audiosSequence.Count; x++)
            {
                switch (_audiosSequence[x])
                {
                    case AUDIO_TAG_ID:
                        Audios.Add(_audiosTags[0]);
                        _audiosTags.RemoveAt(0);
                        break;
                    case AUDIO_CLIP_ID:
                        Audios.Add(_audiosClip[0]);
                        _audiosClip.RemoveAt(0);
                        break;
                }
            }
        }

        public void OnBeforeSerialize()
        {
            _audiosTags.Clear();
            _audiosClip.Clear();
            _audiosSequence.Clear();

            for (int x = 0; x < Audios.Count; x++)
            {
                if (Audios[x] is PYPlayerTag)
                {
                    _audiosTags.Add((PYPlayerTag)Audios[x]);
                    _audiosSequence.Add(AUDIO_TAG_ID);
                }
                else if (Audios[x] is PYPlayerClip)
                {
                    _audiosClip.Add((PYPlayerClip)Audios[x]);
                    _audiosSequence.Add(AUDIO_CLIP_ID);
                }
            }
        }
        #endregion

        public PYPlayer GetPYAudio(string name)
        {
            foreach (PYPlayer player in Audios)
                if (player.Name == name)
                    return player;
            return null;
        }

        public PYPlayer GetDefaultAudio(string name)
        {
            PYPlayer audio = GetPYAudio(name);
            // If any default is found we get the first audio
            if (audio == null)
                audio = Audios[0];
            return audio;
        }

        public PYPlayer StartAudio(string name)
        {
            // TODO: Dont makes much sense this, but works
            PYPlayer audio = GetDefaultAudio(name);
            audio.PYSource = audio.StartAudio();
            return audio;
        }

        public void Play()
        {
            Play("Default");
        }
        public void Play(string name)
        {
            PYPlayer player = StartAudio(name);
            PYAudioSource source = player.PYSource;
            if (source == null)
                return;

            source.Volume(player.Volume)
                .Pitch(player.Pitch)
                .Delay(player.Delay)
                .Loop(player.Loop).Play();
        }
        public void Play(string name, Action<PYAudioSource.PYAudioSourceEventData> callback)
        {
            PYPlayer player = StartAudio(name);
            PYAudioSource source = player.PYSource;
            if (source == null)
                return;

            source.Volume(player.Volume)
                .Pitch(player.Pitch)
                .Delay(player.Delay)
                .Loop(player.Loop).Play(callback);
        }

        public void Stop()
        {
            Stop("Default");
        }
        public void Stop(string name)
        {
            GetPYAudio(name).PYSource.Stop();
        }

        public void Stop(float fadeDuration)
        {
            Stop("Default", fadeDuration);
        }
        public void Stop(string idName, float fadeDuration)
        {
            GetPYAudio(idName).PYSource.Stop(fadeDuration);
        }

        public override void UpdateComponent()
        {
            if (PYBundleManager.Instance.IsReady)
            {
                foreach (PYPlayer player in Audios)
                {
                    if (player is PYPlayerClip)
                    {
                        PYPlayerClip playerClip = (PYPlayerClip)player;
                        if (!playerClip.UpdateData.UpdateFromBundle)
                            continue;

                        playerClip.UpdateData.DefaultComponentValue = playerClip.Clip;

                        AudioClip bundleClip = PYComponentBundle.GetAsset<AudioClip>(playerClip.UpdateData.BundlesToCheck, playerClip.AssetTag.Tag);
                        if (bundleClip != null)
                            playerClip.Clip = bundleClip;
                    }
                }
            }
        }

        public override void RestoreComponent()
        {
            foreach (PYPlayer player in Audios)
            {
                if (player is PYPlayerClip)
                {
                    PYPlayerClip playerClip = (PYPlayerClip)player;
                    if (!playerClip.UpdateData.UpdateFromBundle)
                        continue;

                    playerClip.Clip = (AudioClip)playerClip.UpdateData.DefaultComponentValue;
                }
            }
        }

        private void PlayAudioAutomaticEnableStart()
        {
            PYPlayer audio = GetDefaultAudio("Default");
            if (audio is PYPlayerClip)
            {
                PYPlayerClip audioClip = (PYPlayerClip)audio;
                if (PYBundleManager.Instance != null)
                {
                    if (audioClip.UpdateData.UpdateFromBundle && PYBundleManager.Instance.IsReady)
                        Invoke("Play", DelayStartEnable);
                    else
                        PYBundleManager.Instance.onLoadCompleted.AddListener(PlayDefaultAudioWhenBundleLoads);
                }
                else if (audioClip.Clip != null)
                    Invoke("Play", DelayStartEnable);
            }
            else
                Invoke("Play", DelayStartEnable);
        }

        private void PlayDefaultAudioWhenBundleLoads(PYBundleManager.PYBundleManagerEventData data)
        {
            Invoke("Play", DelayStartEnable);
        }
    }
}