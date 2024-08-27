using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Playmove
{
    public class FloatingAnimation : PYAnimation
    {
        [Flags]
        public enum AxisMask
        {
            X = 1,
            Y = 2,
            Z = 4
        }

        [Header("Floating Config")]
        public AxisMask FreezeAxis = AxisMask.X | AxisMask.Y;
        public float Duration = 1;
        public Vector3 VelocityLimit = new Vector3(2, 2, 0);
        public float Amplitude = 0.1f;
        public bool ResetPositionOnDisable;

        [Header("Rotation Config")]
        public bool Rotate;
        public float Angle = 3f;
        public AnimationCurve RotationCurve;

        private Vector3 _floatPosition, _originalPos;
        private float _randomMultiplierX, _randomMultiplierY, _randomMultiplierZ;

        private PYTweenAnimation _tweenRotate;

        protected override void OnDisable()
        {
            CompletedAnimation();

            if (ResetPositionOnDisable && OwnTransform != null)
                OwnTransform.localPosition = _originalPos;
        }

        protected override void OnEnable()
        {
            if (PlayOnEnable)
                Play();
        }

        protected override void Awake()
        {
            Initialize();
        }

        void Update()
        {
            if (!IsPlaying) return;

            _floatPosition.x += (Mathf.Sin(Time.time * _randomMultiplierX) * Amplitude) * Time.deltaTime;
            _floatPosition.y += (Mathf.Sin(Time.time * _randomMultiplierY) * Amplitude) * Time.deltaTime;
            _floatPosition.z += (Mathf.Sin(Time.time * _randomMultiplierZ) * Amplitude) * Time.deltaTime;

            OwnTransform.localPosition = _floatPosition;
        }

        private void Initialize()
        {
            if ((FreezeAxis & AxisMask.X) == AxisMask.X)
                _randomMultiplierX = Random.Range(1f, VelocityLimit.x);
            if ((FreezeAxis & AxisMask.Y) == AxisMask.Y)
                _randomMultiplierY = Random.Range(1f, VelocityLimit.y);
            if ((FreezeAxis & AxisMask.Z) == AxisMask.Z)
                _randomMultiplierZ = Random.Range(1f, VelocityLimit.z);

            _floatPosition = _originalPos = OwnTransform.localPosition;

            if (Rotate)
            {
                if (_tweenRotate == null)
                    _tweenRotate =
                        PYTweenAnimation.AddNew(gameObject, 1)
                            .SetDuration(Random.Range(Duration, Duration + (Duration * .25f)))
                            .SetRotation(TagManager.Axis.Z, -Random.Range(Angle, Angle + (Angle * .25f)),
                                Random.Range(Angle, Angle + (Angle * .25f)))
                            .SetLoop(TagManager.LoopType.PingPong).SetCurve(RotationCurve);
            }
        }

        public override void Play()
        {
            base.Play();

            gameObject.SetActive(true);
            Initialize();

            if (Rotate)
            {
                _tweenRotate.Play();
            }
        }

        protected override void CompletedAnimation()
        {
            base.CompletedAnimation();
            if (Rotate && _tweenRotate != null) _tweenRotate.Stop();
        }

        public void UpdatePosition(Vector3 newPos)
        {
            _floatPosition = _originalPos = newPos;
        }
    }
}