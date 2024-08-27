using UnityEngine;
using System.Collections;

namespace Playmove
{
    [RequireComponent(typeof(PYButton))]
    public class ButtonCheckBoxAudio : MonoBehaviour
    {

        public PYGroupTag Group;

        void Start()
        {
            if (PYAudioManager.Instance != null)
                GetComponent<PYButton>().onClick.AddListener((sender) => PYAudioManager.Instance.MuteGroup(Group, !PYAudioManager.Instance.IsMute(Group)));
        }
    }
}