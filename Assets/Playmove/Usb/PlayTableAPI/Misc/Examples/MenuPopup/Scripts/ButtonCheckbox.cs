using UnityEngine;
using System.Collections;
using Playmove.Core.Audios;

namespace Playmove
{
    public class ButtonCheckbox : PYButton
    {

        public PYAudioTags MarkVoice, DesmarkVoice;
        public string NewMarkVoice, NewDesmarkVoice;
        private GameObject _checked;

        protected override void Start()
        {
            base.Start();
            _checked = transform.GetChild(0).gameObject;
        }

        protected override void ClickAction()
        {
            base.ClickAction();
            AudioManager.StartAudio(AudioChannel.Voice, _checked.activeSelf ? NewDesmarkVoice : NewMarkVoice).Play();
            _checked.SetActive(!_checked.activeSelf);
        }
    }
}