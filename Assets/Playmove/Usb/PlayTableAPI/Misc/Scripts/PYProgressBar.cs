using System;
using System.Xml.Schema;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Playmove
{
    public class PYProgressBar : MonoBehaviour
    {
        public MeshFilter BarFill;
        public Transform BarFillTail;

        public float StartValue = 100;
        public float MaxValue = 100;

        protected float _currentValue = 0;

        public float CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;

                if (_currentValue > MaxValue)
                    _currentValue = MaxValue;
                else if (_currentValue < 0)
                    _currentValue = 0;

                UpdateBar(_currentValue);
            }
        }

        public Vector2 AnchorPoint
        {
            get;
            set;
        }

        private Transform ownTransform;

        private int indexUV1 = -1, indexUV2 = -1;

        private Vector2[] uvs;
        private Mesh barFillMesh;

        protected bool playSmoothAnimation;
        protected float targetValue, durationAnimation;

        private bool hasSetupBar;

        private Vector3 initialScale = Vector3.zero;
        private Vector3 initialTailScale = Vector3.zero;

        // Use this for initialization
        protected virtual void Awake()
        {
            ownTransform = transform;
            initialScale = ownTransform.localScale;
            if (BarFillTail != null)
                initialTailScale = BarFillTail.localScale;

            SetupBar();

            Initialize(StartValue, MaxValue);
        }

        protected virtual void Update()
        {
            if (!playSmoothAnimation) return;

            CurrentValue = Mathf.Lerp(CurrentValue, targetValue, Time.deltaTime * durationAnimation);

            if (CurrentValue == targetValue || CurrentValue == 0 || CurrentValue == MaxValue)
            {
                playSmoothAnimation = false;
            }

            if (BarFillTail == null) return;

            Vector3 scale = BarFillTail.localScale;
            scale.x = initialTailScale.x * initialScale.x / ownTransform.localScale.x;
            scale.y = initialTailScale.y * initialScale.y / ownTransform.localScale.y;

            if (!(float.IsNaN(scale.x) || float.IsInfinity(scale.x)) &&
                !(float.IsNaN(scale.y) || float.IsInfinity(scale.y)))
            {
                BarFillTail.localScale = scale;
            }
            else
            {
                playSmoothAnimation = false;
            }
        }

        private void SetupBar()
        {
            if (hasSetupBar) return;
            hasSetupBar = true;

            ownTransform = transform;

            barFillMesh = BarFill.mesh;
            uvs = barFillMesh.uv;
            for (int xm = 0; xm < uvs.Length; xm++)
            {
                if (uvs[xm].x > 0)
                {
                    if (indexUV1 == -1) indexUV1 = xm;
                    else indexUV2 = xm;
                }
            }
        }

        public void Initialize(float startValue, float maxValue)
        {
            if (!hasSetupBar) SetupBar();

            StartValue = startValue;
            MaxValue = maxValue;

            CurrentValue = (StartValue > MaxValue) ? MaxValue : StartValue;
        }

        public virtual void StopAnimation()
        {
            playSmoothAnimation = false;
        }

        public virtual void UpdateBar(float value)
        {
            _currentValue = value > MaxValue ? MaxValue : value;

            float xValue = _currentValue / MaxValue;

            uvs[indexUV1] = new Vector2(xValue, uvs[indexUV1].y);
            uvs[indexUV2] = new Vector2(xValue, uvs[indexUV2].y);
            barFillMesh.uv = uvs;

            Vector3 scale = ownTransform.localScale;
            scale.x = xValue;
            ownTransform.localScale = scale;
        }

        public virtual void UpdateBarSmooth(float value, float speed)
        {
            playSmoothAnimation = true;
            targetValue = value;
            durationAnimation = speed;
        }

        public virtual void ResetBar()
        {
            UpdateBar(StartValue);
        }
    }
}