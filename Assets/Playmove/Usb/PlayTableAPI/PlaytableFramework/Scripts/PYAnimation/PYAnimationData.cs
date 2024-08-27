using UnityEngine;
using System;

namespace Playmove
{
    [Serializable]
    public class PYAnimationData
    {
        public float Duration = 1;

        public TagManager.LoopType Loop;
        public int LoopsNumber;

        public bool UsingEase = true;
        public Ease.Type EaseType;
        public AnimationCurve Curve;

        [Tooltip("Debug Only")]
        public float ElapsedDelay;
        [Tooltip("Debug Only")]
        public float ElapsedTime;
    }
}