using UnityEngine;
using System.Collections;

namespace Playmove
{
    [RequireComponent(typeof(SkeletonAnimation))]
    public class PYSkeletonAnimation : PYAnimation
    {

        [Header("PY Skeleton Animation")]
        public string CurrentAnimationName;

        private Spine.Animation _currentAnimation;

        public Spine.SkeletonData SkeletonData
        {
            get { return Skeleton.skeleton.Data; }
        }

        private Color _color = Color.clear;
        public Color Color
        {
            get
            {
                if (_color == Color.clear)
                {
                    _color.r = Skeleton.skeleton.r;
                    _color.g = Skeleton.skeleton.g;
                    _color.b = Skeleton.skeleton.b;
                    _color.a = Skeleton.skeleton.a;
                }

                return _color;
            }
        }

        public bool Loop
        {
            get { return Skeleton.loop; }
            protected set { Skeleton.loop = value; }
        }

        private float _timeScale;
        public float TimeScale
        {
            get { return Skeleton.timeScale; }
            protected set { _timeScale = value; Skeleton.timeScale = value; }
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
        }

        private SkeletonAnimation _skeletonAnimation;
        public SkeletonAnimation Skeleton
        {
            get
            {
                if (_skeletonAnimation == null)
                    _skeletonAnimation = GetComponent<SkeletonAnimation>();

                return _skeletonAnimation;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _timeScale = TimeScale;
        }

        protected override void Start()
        {
            base.Start();
        }

        public override void Play()
        {
            _isPaused = false;
            TimeScale = _timeScale;

            base.Play();
        }

        public override void Reverse()
        {
            Debug.LogError("Spine animations can't be reversed");
        }

        public void Pause()
        {
            _isPaused = true;
            TimeScale = 0;
        }

        public override void Stop()
        {
            _isPlaying = false;
            TimeScale = 0;
            Skeleton.skeleton.SetToSetupPose();
        }

        public Spine.TrackEntry PlayAnimation(string animationName)
        {
            return PlayAnimation(animationName, Loop);
        }

        public Spine.TrackEntry PlayAnimation(string animationName, bool loop, bool resetToSetupPose = true)
        {
            if (resetToSetupPose && animationName != CurrentAnimationName)
                Skeleton.skeleton.SetToSetupPose();

            CurrentAnimationName = animationName;

            Play();

            return Skeleton.state.SetAnimation(0, CurrentAnimationName, loop);
        }

        public PYSkeletonAnimation SetSkin(string skinName)
        {
            Skeleton.skeleton.SetSkin(skinName);

            return this;
        }

        public PYSkeletonAnimation SetLoop(bool loop)
        {
            Loop = loop;

            return this;
        }

        public PYSkeletonAnimation SetTimeScale(float timeScale)
        {
            _timeScale = timeScale;

            if (!IsPaused)
                TimeScale = timeScale;

            return this;
        }

        public PYSkeletonAnimation SetColor(Color color)
        {
            Skeleton.skeleton.r = color.r;
            Skeleton.skeleton.g = color.g;
            Skeleton.skeleton.b = color.b;
            Skeleton.skeleton.a = color.a;

            return this;
        }

        public PYSkeletonAnimation SetAlpha(float alpha)
        {
            Skeleton.skeleton.a = alpha;

            return this;
        }
    }
}