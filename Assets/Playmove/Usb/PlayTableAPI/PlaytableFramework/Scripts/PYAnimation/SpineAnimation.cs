using System;
using UnityEngine;
using System.Collections;

namespace Playmove
{
    [Serializable]
    public struct SpineData
    {
        public string AnimationName;
        public string Skin;
        public bool Loop;
    }

    public class SpineAnimation : PYAnimation
    {

        public SkeletonAnimation SkeletoAnimation;
        public SpineData SplineDataOnPlay, SplineDataOnReverse;
        public bool OnMouseOver;

        public override void Play()
        {
            base.Play();
            Invoke("PlayAnimation", DelayToStart);
        }

        private void PlayAnimation()
        {
            if (!string.IsNullOrEmpty(SplineDataOnPlay.Skin))
                SkeletoAnimation.skeleton.SetSkin(SplineDataOnPlay.Skin);
            else
                SkeletoAnimation.skeleton.SetSkin("default");

            SkeletoAnimation.Update();
            SkeletoAnimation.state.SetAnimation(0, SplineDataOnPlay.AnimationName, SplineDataOnPlay.Loop);
        }

        public override void Reverse()
        {
            base.Reverse();
            if (!string.IsNullOrEmpty(SplineDataOnPlay.Skin))
                SkeletoAnimation.skeleton.SetSkin(SplineDataOnReverse.Skin);
            else
                SkeletoAnimation.skeleton.SetSkin("default");

            SkeletoAnimation.Update();
            SkeletoAnimation.state.SetAnimation(0, SplineDataOnReverse.AnimationName, SplineDataOnReverse.Loop);
        }
    }
}