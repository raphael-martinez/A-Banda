using UnityEngine;
using System;
using System.Collections;

namespace Playmove
{
    public class PYAnimator : PYAnimation
    {
        [Serializable]
        public class AnimatorData
        {
            public string Tag;
            public PYAnimation[] Animations;
        }

        public AnimatorData[] Animations;

        /// <summary>
        /// Hack to identify multiples calls os animations, launching the callback when ther counter return to 0
        /// </summary>
        private int _callbackCounter;
        private int CallbackCounter
        {
            get
            {
                return _callbackCounter;
            }
            set
            {
                _callbackCounter = value;
                if (_callbackCounter <= 0)
                {
                    _callbackCounter = 0;
                    CompletedAnimation();
                }
            }
        }

        public void Play(string tag)
        {
            if (IsPlaying) return;
            for (int x = 0; x < Animations.Length; x++)
            {
                if (Animations[x].Tag == tag)
                {
                    for (int i = 0; i < Animations[x].Animations.Length; i++)
                    {
                        CallbackCounter++;
                        Animations[x].Animations[i].Play(() => CallbackCounter--);
                    }
                    break;
                }
            }
            base.Play();
        }
        public void Play(string tag, Action callback)
        {
            if (IsPlaying) return;
            _callbackPlay = callback;
            Play(tag);
        }

        public void Reverse(string tag)
        {
            if (IsPlaying) return;
            for (int x = 0; x < Animations.Length; x++)
            {
                if (Animations[x].Tag == tag)
                {
                    for (int i = 0; i < Animations[x].Animations.Length; i++)
                    {
                        CallbackCounter++;
                        Animations[x].Animations[i].Reverse(() => CallbackCounter--);
                    }
                    break;
                }
            }
            base.Reverse();
        }
        public void Reverse(string tag, Action callback)
        {
            if (IsPlaying) return;
            _callbackReverse = callback;
            Reverse(tag);
        }

        public void Stop(string tag)
        {
            for (int x = 0; x < Animations.Length; x++)
            {
                if (Animations[x].Tag == tag)
                {
                    for (int i = 0; i < Animations[x].Animations.Length; i++)
                    {
                        CallbackCounter++;
                        Animations[x].Animations[i].Stop();
                    }
                    break;
                }
            }
            base.Stop();
        }

        public PYAnimation[] GetAnimation(string Tag)
        {
            for (int x = 0; x < Animations.Length; x++)
            {
                if (Animations[x].Tag == tag)
                {
                    return Animations[x].Animations;
                }
            }
            return null;
        }
    }
}