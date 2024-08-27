using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class OptionsMenu : PYOpenable
    {

        public PYButton CheckboxMusic, CheckboxSFX, CheckboxVoice;

        void Start()
        {
            CheckboxMusic.onClick.AddListener((sender) => PYAudioManager.Instance.MuteGroup(PYGroupTag.Music, PYAudioManager.Instance.IsMute(PYGroupTag.Music)));
            CheckboxMusic.onClick.AddListener((sender) => PYAudioManager.Instance.MuteGroup(PYGroupTag.SFX, PYAudioManager.Instance.IsMute(PYGroupTag.SFX)));
            CheckboxMusic.onClick.AddListener((sender) => PYAudioManager.Instance.MuteGroup(PYGroupTag.Voice, PYAudioManager.Instance.IsMute(PYGroupTag.Voice)));
        }
    }
}