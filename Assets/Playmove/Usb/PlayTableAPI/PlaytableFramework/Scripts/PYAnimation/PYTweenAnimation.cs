using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

namespace Playmove
{

    #region Classes

    [Serializable]
    public class PTTVector3
    {
        public Vector3 from, to;
        public bool isLocal, isRelative;
        public PTTAxisMask ignoreAxis;

        public PTTVector3(Vector3 value)
        {
            this.from = value;
            this.to = value;
        }

        public PTTVector3(Vector3 from, Vector3 to)
        {
            this.from = from;
            this.to = to;
        }

        public PTTVector3(Vector3 from, Vector3 to, bool isLocal)
        {
            this.from = from;
            this.to = to;
            this.isLocal = isLocal;
        }

        public PTTVector3(Vector3 from, Vector3 to, bool isLocal, bool isRelative)
        {
            this.from = from;
            this.to = to;
            this.isLocal = isLocal;
            this.isRelative = isRelative;
        }

        public PTTVector3(Vector3 from, Vector3 to, bool isLocal, bool isRelative, PTTAxisMask ignoreAxis)
        {
            this.from = from;
            this.to = to;
            this.isLocal = isLocal;
            this.isRelative = isRelative;
            this.ignoreAxis = ignoreAxis;
        }
    }

    [Serializable]
    public class PTTFloat
    {
        public float from, to, value;

        public PTTFloat(float value)
        {
            this.from = value;
            this.to = value;
        }

        public PTTFloat(float from, float to)
        {
            this.from = from;
            this.to = to;
        }
    }

    [Serializable]
    public class PTTColor
    {
        public Color from, to;

        public PTTColor(Color value)
        {
            this.from = value;
            this.to = value;
        }

        public PTTColor(Color from, Color to)
        {
            this.from = from;
            this.to = to;
        }
    }

    public class PTTTimedAction
    {
        public Action action;
        public float time;

        public PTTTimedAction(Action action, float time)
        {
            this.action = action;
            this.time = time;
        }
    }

    [Serializable]
    public struct PTTAxisMask
    {
        public bool X, Y, Z;

        public bool All
        {
            get { return (X && Y && Z); }
            set { X = value; Y = value; Z = value; }
        }

        public bool None
        {
            get { return (!X && !Y && !Z); }
        }

        public PTTAxisMask(bool x, bool y, bool z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
    #endregion

    public class PYTweenAnimation : PYAnimation
    {
        [Header("PYTween")]
        [Tooltip("Speed overwrite Duration")]
        public int Id;

        public PYAnimationData AnimationData;

        public float Speed;

        [Tooltip("Reverses the curve when the loop is set to PingPong")]
        public bool FlipCurveOnReverse;

        public PTTVector3 Position;
        public PTTVector3 Rotation;
        public PTTVector3 Scale;
        public PTTColor Color;
        public PTTFloat Alpha;
        public PTTFloat Float;

        private bool _hasReverted = false;

        private Rigidbody2D _rigidbody2D;

        private Vector3 _startPosition, _startRotation, _startScale;

        #region Properties
        [SerializeField]
        protected bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
        }

        protected bool _hasCompleted;
        public bool HasCompleted
        {
            get { return _hasCompleted; }
        }

        private int _loopcount;
        public int LoopCount
        {
            get { return _loopcount; }
        }

        public GameObject Target
        {
            get
            {
                return gameObject;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            if (AnimationData == null)
                AnimationData = new PYAnimationData();

            if ((AnimationData.Curve == null || AnimationData.Curve.keys.Length <= 1) && !AnimationData.UsingEase)
            {
                AnimationData.UsingEase = true;
                AnimationData.EaseType = Ease.Type.Linear;
            }

            easeMethod = System.Type.GetType("Playmove.Ease").GetMethod(AnimationData.EaseType.ToString());

            colorProperty = "_Color";
            GetRenderers();

            _startPosition = OwnTransform.position;
            _startRotation = OwnTransform.eulerAngles;
            _startScale = OwnTransform.localScale;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (PlayOnEnable)
            {
                if (!_hasReverted)
                    Play();
                else
                    Reverse();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StopAllCoroutines();

            Stop();
        }

        protected void Update()
        {
            if (!IsPlaying || IsPaused)
                return;

            if (DelayLoop())
                return;

            if (TweenLoop())
                return;

            TweenCompleted();
        }

        #region Add
        public static PYTweenAnimation Add(GameObject target, bool addRigidbody = true)
        {
            if (!target.GetComponent<PYTweenAnimation>())
            {
                target.AddComponent<PYTweenAnimation>();
            }

            PYTweenAnimation tween = target.GetComponent<PYTweenAnimation>();

            if (addRigidbody && target.GetComponent<Collider2D>() && !target.GetComponent<Rigidbody2D>())
            {
                tween._rigidbody2D = target.AddComponent<Rigidbody2D>();
                tween._rigidbody2D.isKinematic = true;
            }

            tween.Initialize();

            return tween;
        }

        public static PYTweenAnimation AddNew(GameObject target, int id = 1)
        {
            PYTweenAnimation[] tweens = target.GetComponents<PYTweenAnimation>();

            if (tweens != null)
            {
                for (int i = 0; i < tweens.Length; i++)
                {
                    if (tweens[i].Id == id)
                    {
                        return tweens[i];
                    }
                }
            }

            PYTweenAnimation tween = target.AddComponent<PYTweenAnimation>();
            tween.Id = id;
            tween.Initialize();

            return tween;
        }

        #endregion

        #region Setters
        #region Position
        public PYTweenAnimation SetPosition(Vector3 from, Vector3 to, bool isLocal = false, bool isRelative = false)
        {
            Position = new PTTVector3(from, to, isLocal, isRelative);
            Position.ignoreAxis = new PTTAxisMask(false, false, false);

            return this;
        }

        public PYTweenAnimation SetPosition(TagManager.Axis axis, float from, float to, bool isLocal = false, bool isRelative = false)
        {
            if (Position.ignoreAxis.None)
                Position.ignoreAxis = new PTTAxisMask(true, true, true);

            switch (axis)
            {
                case TagManager.Axis.X:
                    Position.ignoreAxis.X = false;
                    Position.from.x = from + (isLocal ? OwnTransform.position.x : 0);
                    Position.to.x = to + (isLocal ? OwnTransform.position.x : 0);
                    break;

                case TagManager.Axis.Y:
                    Position.ignoreAxis.Y = false;
                    Position.from.y = from + (isLocal ? OwnTransform.position.y : 0);
                    Position.to.y = to + (isLocal ? OwnTransform.position.y : 0);
                    break;

                case TagManager.Axis.Z:
                    Position.ignoreAxis.Z = false;
                    Position.from.z = from + (isLocal ? OwnTransform.position.z : 0);
                    Position.to.z = to + (isLocal ? OwnTransform.position.z : 0);
                    break;
            }

            Position.isLocal = isLocal;
            Position.isRelative = isRelative;

            return this;
        }
        #endregion

        #region Rotation
        public PYTweenAnimation SetRotation(Vector3 from, Vector3 to, bool isLocal = false, bool isRelative = false)
        {
            Rotation = new PTTVector3(from, to, isLocal, isRelative);
            Rotation.ignoreAxis = new PTTAxisMask(false, false, false);

            return this;
        }

        public PYTweenAnimation SetRotation(TagManager.Axis axis, float from, float to, bool isLocal = false, bool isRelative = false)
        {
            if (Rotation.ignoreAxis.None)
                Rotation.ignoreAxis = new PTTAxisMask(true, true, true);

            switch (axis)
            {
                case TagManager.Axis.X:
                    Rotation.ignoreAxis.X = false;
                    Rotation.from.x = from;
                    Rotation.to.x = to;
                    break;

                case TagManager.Axis.Y:
                    Rotation.ignoreAxis.Y = false;
                    Rotation.from.y = from;
                    Rotation.to.y = to;
                    break;

                case TagManager.Axis.Z:
                    Rotation.ignoreAxis.Z = false;
                    Rotation.from.z = from;
                    Rotation.to.z = to;
                    break;
            }

            Rotation.isLocal = isLocal;
            Rotation.isRelative = isRelative;

            return this;
        }
        #endregion

        #region Scale
        public PYTweenAnimation SetScale(Vector3 from, Vector3 to, bool isRelative = false)
        {
            Scale = new PTTVector3(from, to, false, isRelative);
            Scale.ignoreAxis = new PTTAxisMask(false, false, false);

            return this;
        }

        public PYTweenAnimation SetScale(TagManager.Axis axis, float from, float to, bool isRelative = false)
        {
            if (Scale.ignoreAxis.None)
                Scale.ignoreAxis = new PTTAxisMask(true, true, true);

            switch (axis)
            {
                case TagManager.Axis.X:
                    Scale.ignoreAxis.X = false;
                    Scale.from.x = from;
                    Scale.to.x = to;
                    break;

                case TagManager.Axis.Y:
                    Scale.ignoreAxis.Y = false;
                    Scale.from.y = from;
                    Scale.to.y = to;
                    break;

                case TagManager.Axis.Z:
                    Scale.ignoreAxis.Z = false;
                    Scale.from.z = from;
                    Scale.to.z = to;
                    break;
            }

            Scale.isRelative = isRelative;

            return this;
        }
        #endregion

        #region Color
        private Renderer[] renderers;
        private UnityEngine.UI.Image[] images;
        private Material[] bkpMaterials;
        private string colorProperty;

        #region Color
        //public PTTColor color;
        public PYTweenAnimation SetColor(Color from, Color to, string colorProperty = "_Color")
        {
            Color = new PTTColor(from, to);
            renderers = GetComponentsInChildren<Renderer>(true);
            this.colorProperty = colorProperty;
            GetRenderers();
            return this;
        }
        #endregion

        #region Alpha
        //public PTTFloat alpha;
        public PYTweenAnimation SetAlpha(float from, float to, string colorProperty = "_Color")
        {
            Alpha = new PTTFloat(from, to);
            renderers = GetComponentsInChildren<Renderer>(true);
            this.colorProperty = colorProperty;
            GetRenderers();
            return this;
        }

        public PYTweenAnimation SetAlpha(TagManager.Type operation, float value, string colorProperty = "_Color")
        {
            this.colorProperty = colorProperty;

            Renderer tempRender = GetComponent<Renderer>() ? GetComponent<Renderer>() : GetComponentInChildren<Renderer>();
            Image tempImageRender = GetComponentInChildren<Image>();

            float targetAlpha = tempImageRender != null ? tempImageRender.color.a : tempRender.sharedMaterial.GetColor(colorProperty).a;
            if (tempRender is SpriteRenderer)
            {
                targetAlpha = ((SpriteRenderer)tempRender).color.a;
            }

            switch (operation)
            {
                case TagManager.Type.From:
                    Alpha = new PTTFloat(value, targetAlpha);
                    break;

                case TagManager.Type.To:
                    Alpha = new PTTFloat(targetAlpha, value);
                    break;
            }

            GetRenderers();
            return this;
        }
        #endregion

        private void GetRenderers()
        {
            //if (color.from == color.to && alpha.from == alpha.to) return;

            renderers = GetComponentsInChildren<Renderer>(true);

            images = GetComponentsInChildren<UnityEngine.UI.Image>(true);

            bkpMaterials = new Material[renderers.Length];

            for (int i = 0; i < bkpMaterials.Length; i++)
            {
                bkpMaterials[i] = renderers[i].sharedMaterial;
            }
        }
        #endregion

        #region Float
        private float _float;
        public PYTweenAnimation SetFloat(float from, float to)
        {
            Float = new PTTFloat(from, to);

            return this;
        }
        #endregion

        #region Duration
        public PYTweenAnimation SetDuration(float time)
        {
            AnimationData.Duration = time;
            return this;
        }
        public void SetDurationEvent(float time)
        {
            AnimationData.Duration = time;
        }
        #endregion

        #region Speed
        public PYTweenAnimation SetSpeed(float speed)
        {
            this.Speed = speed;

            CalculateDuration();

            return this;
        }
        public void SetSpeedEvent(float speed)
        {
            this.Speed = speed;

            CalculateDuration();
        }
        #endregion

        #region Delay
        public PYTweenAnimation SetDelay(float time)
        {
            DelayToStart = time;
            return this;
        }

        public void SetDelayEvent(float time)
        {
            DelayToStart = time;
        }
        #endregion

        #region Loop
        public PYTweenAnimation SetLoop(TagManager.LoopType loop)
        {
            AnimationData.Loop = loop;
            return this;
        }

        public PYTweenAnimation SetLoop(TagManager.LoopType loop, int loopsNumber)
        {
            AnimationData.Loop = loop;
            AnimationData.LoopsNumber = loopsNumber;
            return this;
        }
        #endregion

        #region Ease
        float easeValue;
        System.Reflection.MethodInfo easeMethod = null;
        public PYTweenAnimation SetEaseType(Ease.Type easeType)
        {
            AnimationData.EaseType = easeType;
            AnimationData.UsingEase = true;
            easeMethod = System.Type.GetType("Playmove.Ease").GetMethod(AnimationData.EaseType.ToString());
            return this;
        }
        #endregion

        #region Curve
        public PYTweenAnimation SetCurve(AnimationCurve curve)
        {
            AnimationData.Curve = curve;
            AnimationData.UsingEase = false;
            return this;
        }
        #endregion

        #region Callbacks
        Action startCallback, updateCallback;
        private Action<GameObject> finishCallback;
        public PYTweenAnimation StartCallback(Action callback)
        {
            startCallback = callback;
            return this;
        }

        public PYTweenAnimation UpdateCallback(Action callback)
        {
            updateCallback = callback;
            return this;
        }

        public PYTweenAnimation FinishCallback(Action<GameObject> callback)
        {
            finishCallback = callback;
            return this;
        }

        List<PTTTimedAction> timedCallbacks;
        public PYTweenAnimation TimedCallback(Action callback, float approxTime)
        {
            if (timedCallbacks == null)
                timedCallbacks = new List<PTTTimedAction>();

            timedCallbacks.Add(new PTTTimedAction(callback, approxTime));
            return this;
        }

        public PYTweenAnimation RemoveAllTimedCallbacks()
        {
            timedCallbacks = new List<PTTTimedAction>();
            return this;
        }
        #endregion

        public PYTweenAnimation SetName(string name)
        {
            Name = name;
            return this;
        }

        #endregion

        #region Functions
        Action callback;
        /// <summary>
        /// Plays the animation.
        /// </summary>
        public override void Play()
        {
            Play(null);
        }
        /// <summary>
        /// Plays the animation
        /// </summary>
        /// <param name="callback">The callback function to be called at the end of the animation</param>
        public override void Play(Action callback)
        {
            if (!Target.activeSelf) return;

            this.callback = callback;

            _isPaused = false;

            if (!IsPlaying)
            {
                base.Play();

                if (_hasReverted)
                {
                    ReverseDirection();
                }
                _loopcount = 0;

                //StopCoroutine("Tween");
                //StartCoroutine("Tween");

                StartTween();
            }
        }

        /// <summary>
        /// Plays the animation in reverse
        /// </summary>
        public override void Reverse()
        {
            Reverse(null);
        }
        /// <summary>
        /// Plays the animation in reverse
        /// </summary>
        /// <param name="callback">The callback function to be called at the end of the animation</param>
        public override void Reverse(Action callback)
        {
            if (!Target.activeSelf) return;

            this.callback = callback;

            _isPaused = false;

            if (!IsPlaying)
            {
                base.Reverse();

                if (!_hasReverted)
                {
                    ReverseDirection();
                }

                //StopCoroutine("Tween");
                //StartCoroutine("Tween");

                StartTween();
            }
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public override void Stop()
        {
            base.Stop();

            //StopCoroutine("Tween");
        }

        public void Reset()
        {
            Stop();
            UpdateAll(0);
        }

        public void Clear()
        {
            Initialize();
        }

        public void InitialState()
        {
            UpdateAll(0);
        }

        public void FinalState()
        {
            UpdateAll(1);
        }

        #endregion

        #region Initialize
        void Initialize()
        {
            if (AnimationData == null)
                Awake();

            Position = new PTTVector3(OwnTransform.position);
            Rotation = new PTTVector3(OwnTransform.eulerAngles);
            Scale = new PTTVector3(OwnTransform.localScale);
            if (GetComponent<Renderer>() != null && GetComponent<Renderer>().sharedMaterial != null)
            {
                Color =
                    new PTTColor((GetComponent<Renderer>().sharedMaterial.HasProperty(colorProperty)
                        ? GetComponent<Renderer>().sharedMaterial.GetColor(colorProperty)
                        : UnityEngine.Color.white));
                Alpha =
                    new PTTFloat((GetComponent<Renderer>().sharedMaterial.HasProperty(colorProperty)
                        ? GetComponent<Renderer>().sharedMaterial.GetColor(colorProperty).a
                        : 1));
            }
            else
            {
                Color = new PTTColor(UnityEngine.Color.white);
                Alpha = new PTTFloat(1);
            }
            Float = new PTTFloat(0);

            AnimationData.Duration = 1;
            Speed = 0;
            DelayToStart = 0;
            SetEaseType(Ease.Type.Linear);
            startCallback = null;
            updateCallback = null;
            finishCallback = null;
            _hasReverted = false;

            _loopcount = 0;

            AnimationData.Curve = AnimationCurve.Linear(0, 0, 1, 1);
            AnimationData.Loop = TagManager.LoopType.None;
        }
        #endregion

        #region Tween
        void StartTween()
        {
            StartFrom(0);
        }

        void StartFrom(float time)
        {
            AnimationData.ElapsedTime = time;
            AnimationData.ElapsedDelay = time;

            CalculateDuration();

            GetRenderers();
            InstantiateMaterials();

            UpdateAll(CalculateEase());

            _isPlaying = true;
            _hasCompleted = false;

            if (startCallback != null)
                startCallback();
        }

        private bool DelayLoop()
        {
            if (AnimationData.ElapsedDelay < DelayToStart)
            {
                AnimationData.ElapsedDelay += Time.deltaTime;
                return true;
            }

            return false;
        }

        private bool TweenLoop()
        {
            if (AnimationData.ElapsedTime < AnimationData.Duration)
            {
                AnimationData.ElapsedTime += Time.deltaTime;
                UpdateAll(CalculateEase());
                if (updateCallback != null)
                    updateCallback();
                CheckTimedCallback();

                return true;
            }

            return false;
        }

        private void TweenCompleted()
        {
            if (!HasCompleted && IsPlaying)
            {
                _hasCompleted = true;

                AnimationData.ElapsedTime = AnimationData.Duration;
                UpdateAll(1);

                CompletedAnimation();

                if (callback != null)
                {
                    callback();
                    callback = null;
                }

                if (finishCallback != null)
                    finishCallback(gameObject);

                ReturnMaterials();

                if (AnimationData.Loop != TagManager.LoopType.None)
                {
                    _loopcount++;

                    if (AnimationData.LoopsNumber == 0 || _loopcount < AnimationData.LoopsNumber)
                    {
                        if (AnimationData.Loop == TagManager.LoopType.PingPong)
                            ReverseDirection();

                        StartTween();
                    }
                }
            }
        }
        #endregion

        #region CalculateDuration
        float positionDuration, rotationDuration, scaleDuration, colorDuration, alphaDuration;
        List<float> durationsList = new List<float>();
        void CalculateDuration()
        {
            if (Speed <= 0) return;

            durationsList = new List<float>();

            if (Position.from != Position.to)
            {
                positionDuration = Mathf.Abs(Vector3.Distance(Position.from, Position.to) / Speed);
                durationsList.Add(positionDuration);
            }
            if (Scale.from != Scale.to)
            {
                scaleDuration = Mathf.Abs(Vector3.Distance(Scale.from, Scale.to) / Speed);
                durationsList.Add(scaleDuration);
            }
            if (Rotation.from != Rotation.to)
            {
                rotationDuration = Mathf.Abs((Vector3.Angle(Rotation.from, Rotation.to)) / Speed);
                durationsList.Add(rotationDuration);
            }
            if (Color.from != Color.to)
            {
                colorDuration = Mathf.Abs(Vector4.Distance((Vector4)Color.from, (Vector4)Color.to) / Speed);
                durationsList.Add(colorDuration);
            }
            if (Alpha.from != Alpha.to)
            {
                alphaDuration = Mathf.Abs((Alpha.from - Alpha.to) / Speed);
                durationsList.Add(alphaDuration);
            }

            if (durationsList.Count > 0)
                AnimationData.Duration = durationsList.Max();
        }
        #endregion

        #region Updates
        void UpdateAll(float value)
        {
            easeValue = value;

            if (Target == null) return;

            UpdatePosition();
            UpdateRotation();
            UpdateScale();
            UpdateColor();
            UpdateAlpha();
            UpdateFloat();
        }

        float CalculateEase()
        {
            if (AnimationData.ElapsedTime > AnimationData.Duration)
                AnimationData.ElapsedTime = AnimationData.Duration;

            if (easeMethod == null)
                return 0;

            return (AnimationData.UsingEase) ? (float)easeMethod.Invoke(null, new object[] { AnimationData.ElapsedTime, 0, 1, AnimationData.Duration }) :
                AnimationData.Curve.Evaluate(AnimationData.ElapsedTime / AnimationData.Duration);
        }

        #region Position
        void UpdatePosition()
        {
            if (Position.from == Position.to || ((OwnTransform.position == Position.to || OwnTransform.localPosition == Position.to) &&
                AnimationData.ElapsedTime >= AnimationData.Duration))
                return;

            Vector3 p = Position.isLocal ? OwnTransform.localPosition : OwnTransform.position;
            Vector3 add = Position.isRelative ? _startPosition : Vector3.zero;

            p.x = Position.ignoreAxis.X ? p.x : Position.from.x + easeValue * (Position.to.x - Position.from.x) + add.x;
            p.y = Position.ignoreAxis.Y ? p.y : Position.from.y + easeValue * (Position.to.y - Position.from.y) + add.y;
            p.z = Position.ignoreAxis.Z ? p.z : Position.from.z + easeValue * (Position.to.z - Position.from.z) + add.z;

            if (Position.isLocal)
            {
                OwnTransform.localPosition = p;
            }
            else
            {
                if (_rigidbody2D == null)
                    OwnTransform.position = p;
                else
                    _rigidbody2D.MovePosition(p);
            }
        }
        #endregion

        #region Rotation
        void UpdateRotation()
        {
            if (Rotation.from == Rotation.to || (OwnTransform.eulerAngles == Rotation.to && AnimationData.ElapsedTime >= AnimationData.Duration)) return;

            Vector3 r = Rotation.isLocal ? OwnTransform.localEulerAngles : OwnTransform.eulerAngles;
            Vector3 add = Rotation.isRelative ? _startRotation : Vector3.zero;

            r.x = Rotation.ignoreAxis.X ? OwnTransform.localEulerAngles.x : Rotation.from.x + easeValue * (Rotation.to.x - Rotation.from.x) + add.x;
            r.y = Rotation.ignoreAxis.Y ? OwnTransform.localEulerAngles.y : Rotation.from.y + easeValue * (Rotation.to.y - Rotation.from.y) + add.y;
            r.z = Rotation.ignoreAxis.Z ? OwnTransform.localEulerAngles.z : Rotation.from.z + easeValue * (Rotation.to.z - Rotation.from.z) + add.z;

            if (Rotation.isLocal)
                OwnTransform.localRotation = Quaternion.Euler(r);
            else
            {
                if (!float.IsNaN(r.x) && !float.IsNaN(r.y) && !float.IsNaN(r.z))
                    OwnTransform.rotation = Quaternion.Euler(r);
            }

        }
        #endregion

        #region Scale
        void UpdateScale()
        {
            if (Scale.from == Scale.to || (OwnTransform.localScale == Scale.to && AnimationData.ElapsedTime >= AnimationData.Duration)) return;

            Vector3 s = OwnTransform.localScale;
            Vector3 add = Scale.isRelative ? OwnTransform.localScale : Vector3.zero;

            s.x = Scale.ignoreAxis.X ? OwnTransform.localScale.x : Scale.from.x + easeValue * (Scale.to.x - Scale.from.x) + add.x;
            s.y = Scale.ignoreAxis.Y ? OwnTransform.localScale.y : Scale.from.y + easeValue * (Scale.to.y - Scale.from.y) + add.y;
            s.z = Scale.ignoreAxis.Z ? OwnTransform.localScale.z : Scale.from.z + easeValue * (Scale.to.z - Scale.from.z) + add.z;

            OwnTransform.localScale = s;
        }
        #endregion

        #region Color
        void UpdateColor()
        {
            if (Color.from == Color.to) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                TextMesh tm = renderers[i].GetComponent<TextMesh>();
                Color newColor = new Color();

                if (tm)
                {
                    newColor = Color.from + easeValue * (Color.to - Color.from);
                    PYTextBox tmb = tm.GetComponent<PYTextBox>();
                    if (tmb != null)
                        tmb.Color = newColor;
                    else
                        tm.color = newColor;
                }
                else if (renderers[i] is SpriteRenderer)
                {
                    SpriteRenderer render = (SpriteRenderer)renderers[i];
                    newColor = render.color;
                    newColor = Color.from + easeValue * (Color.to - Color.from);
                    render.color = newColor;
                }
                else if (renderers[i].sharedMaterial.HasProperty(colorProperty))
                {
                    newColor = Color.from + easeValue * (Color.to - Color.from);
                    renderers[i].sharedMaterial.SetColor(colorProperty, newColor);
                }
            }

            if (images == null)
                return;

            for (int j = 0; j < images.Length; j++)
            {
                images[j].color = Color.from + easeValue * (Color.to - Color.from);
            }
        }

        void InstantiateMaterials()
        {
            if (Color.from == Color.to && Alpha.from == Alpha.to) return;

            List<Material> refMaterials = new List<Material>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!refMaterials.Contains(renderers[i].sharedMaterial))
                {
                    refMaterials.Add(renderers[i].sharedMaterial);
                }
            }

            List<Material> cloneMaterials = new List<Material>();
            for (int i = 0; i < refMaterials.Count; i++)
            {
                cloneMaterials.Add(refMaterials[i] == null ? null : new Material(refMaterials[i]));
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < refMaterials.Count; j++)
                {
                    if (renderers[i].sharedMaterial != null && renderers[i].sharedMaterial == refMaterials[j])
                    {
                        renderers[i].sharedMaterial = cloneMaterials[j];
                    }
                }
            }
        }

        void ReturnMaterials()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && bkpMaterials[i] != null && renderers[i].sharedMaterial != null && renderers[i].sharedMaterial.HasProperty(colorProperty))
                {
                    if (renderers[i].sharedMaterial.GetColor(colorProperty) == bkpMaterials[i].GetColor(colorProperty))
                    {
                        renderers[i].sharedMaterial = bkpMaterials[i];
                    }
                }
            }
        }
        #endregion

        #region Alpha
        void UpdateAlpha()
        {
            if (Alpha.from == Alpha.to) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                TextMesh tm = renderers[i].GetComponent<TextMesh>();
                Color newColor = new Color();

                if (tm)
                {
                    newColor = tm.color;
                    newColor.a = Mathf.Clamp01(Alpha.from + easeValue * (Alpha.to - Alpha.from));

                    PYTextBox tmb = tm.GetComponent<PYTextBox>();
                    if (tmb != null)
                        tmb.Color = newColor;
                    else
                        tm.color = newColor;
                }
                else if (renderers[i] is SpriteRenderer)
                {
                    SpriteRenderer render = (SpriteRenderer)renderers[i];
                    newColor = render.color;
                    newColor.a = Mathf.Clamp01(Alpha.from + easeValue * (Alpha.to - Alpha.from));
                    render.color = newColor;
                }

                //TODO: rever uso de shared materials
                else if (renderers[i].material == null)
                    continue;
                else if (renderers[i].material.HasProperty(colorProperty))
                {
                    newColor = renderers[i].material.GetColor(colorProperty);
                    newColor.a = Mathf.Clamp01(Alpha.from + easeValue * (Alpha.to - Alpha.from));
                    renderers[i].material.SetColor(colorProperty, newColor);
                }
            }

            if (images == null)
                return;

            for (int j = 0; j < images.Length; j++)
            {
                Color newColor = new Color();
                newColor = images[j].color;
                newColor.a = Mathf.Clamp01(Alpha.from + easeValue * (Alpha.to - Alpha.from));
                images[j].color = newColor;
            }
        }
        #endregion

        #region Float
        void UpdateFloat()
        {
            if (Float.from == Float.to || (Float.value == Float.to && AnimationData.ElapsedTime >= AnimationData.Duration)) return;

            Float.value = Float.from + easeValue * (Float.to - Float.from);
        }
        #endregion

        #region TimedCallbacks
        void CheckTimedCallback()
        {
            if (timedCallbacks == null) return;

            for (int i = 0; i < timedCallbacks.Count; i++)
            {
                if (timedCallbacks[i].time >= AnimationData.ElapsedTime - Time.deltaTime && timedCallbacks[i].time <= AnimationData.ElapsedTime)
                {
                    timedCallbacks[i].action();
                }
            }
        }
        #endregion
        #endregion

        #region Revert
        void ReverseDirection()
        {
            _hasReverted = !_hasReverted;

            Position = new PTTVector3(Position.to, Position.from, Position.isLocal, Position.isRelative, Position.ignoreAxis);
            Rotation = new PTTVector3(Rotation.to, Rotation.from, Rotation.isLocal, Rotation.isRelative, Rotation.ignoreAxis);
            Scale = new PTTVector3(Scale.to, Scale.from, Scale.isLocal, Scale.isRelative, Scale.ignoreAxis);
            Color = new PTTColor(Color.to, Color.from);
            Alpha = new PTTFloat(Alpha.to, Alpha.from);
            Float = new PTTFloat(Float.to, Float.from);

            if (AnimationData.Curve != null && FlipCurveOnReverse)
                AnimationData.Curve = ReverseCurve(AnimationData.Curve);
        }

        AnimationCurve ReverseCurve(AnimationCurve curve)
        {
            Keyframe[] keys = new Keyframe[curve.keys.Length];
            float totalTime = curve.keys[keys.Length - 1].time;

            for (int i = keys.Length - 1; i >= 0; i--)
            {
                Keyframe key = curve.keys[keys.Length - 1 - i];

                keys[i] = new Keyframe(totalTime - key.time, 1 - key.value, key.inTangent, key.outTangent);
            }

            return new AnimationCurve(keys);
        }

        #endregion
    }

    #region EasingEquations
    public class Ease
    {
        public enum Type
        {
            Linear,
            OutExpo, InExpo, InOutExpo, OutInExpo,
            OutCirc, InCirc, InOutCirc, OutInCirc,
            //OutQuad, InQuad, InOutQuad, OutInQuad,
            OutSine, InSine, InOutSine, OutInSine,
            //OutCubic, InCubic, InOutCubic, OutInCubic,
            //OutQuartic, InQuartic, InOutQuartic, OutInQuartic, 
            //OutQuintic, InQuintic, InOutQuintic, OutInQuintic,
            OutElastic, InElastic, InOutElastic, OutInElastic,
            OutBounce, InBounce, InOutBounce, OutInBounce,
            OutBack, InBack, InOutBack, OutInBack
        }

        #region Linear
        public static float Linear(float time, float start, float end, float duration)
        {
            return end * time / duration + start;
        }
        #endregion

        #region Expo
        public static float OutExpo(float time, float start, float end, float duration)
        {
            return (time == duration) ? start + end : end * (-Mathf.Pow(2, -10 * time / duration) + 1) + start;
        }

        public static float InExpo(float time, float start, float end, float duration)
        {
            return (time == 0) ? start : end * Mathf.Pow(2, 10 * (time / duration - 1)) + start;
        }

        public static float InOutExpo(float time, float start, float end, float duration)
        {
            if (time == 0)
                return start;

            if (time == duration)
                return start + end;

            if ((time /= duration / 2) < 1)
                return end / 2 * Mathf.Pow(2, 10 * (time - 1)) + start;

            return end / 2 * (-Mathf.Pow(2, -10 * --time) + 2) + start;
        }

        public static float OutInExpo(float time, float start, float end, float duration)
        {
            if (time < duration / 2)
                return OutExpo(time * 2, start, end / 2, duration);

            return InExpo((time * 2) - duration, start + end / 2, end / 2, duration);
        }
        #endregion

        #region Circular
        public static float OutCirc(float time, float start, float end, float duration)
        {
            return end * Mathf.Sqrt(1 - (time = time / duration - 1) * time) + start;
        }

        public static float InCirc(float time, float start, float end, float duration)
        {
            return -end * (Mathf.Sqrt(1 - (time /= duration) * time) - 1) + start;
        }

        public static float InOutCirc(float time, float start, float end, float duration)
        {
            if (time < duration / 2)
                return -end / 2 * (Mathf.Sqrt(1 - time * time) - 1) + start;

            return end / 2 * (Mathf.Sqrt(1 - (time -= 2) * time) + 1) + start;
        }

        public static float OutInCirc(float time, float start, float end, float duration)
        {
            if (time < duration / 2)
                return OutCirc(time * 2, start, end / 2, duration);

            return InCirc((time * 2) - duration, start + end / 2, end / 2, duration);
        }
        #endregion

        #region Quad
        #endregion

        #region Sine
        public static float OutSine(float time, float start, float end, float duration)
        {
            return end * Mathf.Sin(time / duration * (Mathf.PI / 2)) + start;
        }

        public static float InSine(float time, float start, float end, float duration)
        {
            return -end * Mathf.Cos(time / duration * (Mathf.PI / 2)) + end + start;
        }

        public static float InOutSine(float time, float start, float end, float duration)
        {
            if ((time /= duration / 2) < 1)
                return end / 2 * (Mathf.Sin(Mathf.PI * time / 2)) + start;

            return -end / 2 * (Mathf.Cos(Mathf.PI * --time / 2) - 2) + start;
        }

        public static float OutInSine(float time, float start, float end, float duration)
        {
            if (time < duration / 2)
                return OutSine(time * 2, start, end / 2, duration);

            return InSine((time * 2) - duration, start + end / 2, end / 2, duration);
        }
        #endregion

        #region Cubic
        #endregion

        #region Quartic
        #endregion

        #region Quintic
        #endregion

        #region Elastic
        public static float OutElastic(float time, float start, float end, float duration)
        {
            if ((time /= duration) == 1)
                return start + end;

            float p = duration * 0.3f;
            float s = p / 4;

            return (end * Mathf.Pow(2, -10 * time) * Mathf.Sin((time * duration - s) * (2 * Mathf.PI) / p) + end + start);
        }

        public static float InElastic(float time, float start, float end, float duration)
        {
            if ((time /= duration) == 1)
                return start + end;

            float p = duration * 0.3f;
            float s = p / 4;

            return -(end * Mathf.Pow(2, 10 * (time -= 1)) * Mathf.Sin((time * duration - s) * (2 * Mathf.PI) / p)) + start;
        }

        public static float InOutElastic(float time, float start, float end, float duration)
        {
            if ((time /= duration / 2) == 2)
                return start + end;

            float p = duration * (0.3f * 1.5f);
            float s = p / 4;

            if (time < 1)
                return -0.5f * (end * Mathf.Pow(2, 10 * (time -= 1)) * Mathf.Sin((time * duration - s) * (2 * Mathf.PI) / p)) + start;

            return end * Mathf.Pow(2, -10 * (time -= 1)) * Mathf.Sin((time * duration - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }

        public static float OutInElastic(float time, float start, float end, float duration)
        {
            if (time < duration / 2)
                return OutElastic(time * 2, start, end / 2, duration);

            return InElastic((time * 2) - duration, start + end / 2, end / 2, duration);
        }
        #endregion

        #region Bounce
        public static float OutBounce(float time, float start, float end, float duration)
        {
            if ((time /= duration) < (1 / 2.75f))
                return end * (7.5625f * time * time) + start;
            else if (time < (2 / 2.75f))
                return end * (7.5625f * (time -= (1.5f / 2.75f)) * time + 0.75f) + start;
            else if (time < (2.5f / 2.75f))
                return end * (7.5625f * (time -= (2.25f / 2.75f)) * time + 0.9375f) + start;
            else
                return end * (7.5625f * (time -= (2.625f / 2.75f)) * time + 0.984375f) + start;
        }

        public static float InBounce(float time, float start, float end, float duration)
        {
            return end - OutBounce(duration - time, 0, end, duration) + start;
        }

        public static float InOutBounce(float time, float start, float end, float duration)
        {
            if (time < duration / 2)
                return InBounce(time * 2, 0, end, duration) * 0.5f + start;

            return OutBounce(time * 2 - duration, 0, end, duration) * 0.5f + end * 0.5f + start;
        }

        public static float OutInBounce(float time, float start, float end, float duration)
        {
            if (time < duration / 2)
                return OutBounce(time * 2, start, end / 2, duration);

            return InBounce((time * 2) - duration, start + end / 2, end / 2, duration);
        }
        #endregion

        #region Back
        public static float OutBack(float time, float start, float end, float duration)
        {
            return end * ((time = time / duration - 1) * time * ((1.70158f + 1) * time + 1.70158f) + 1) + start;
        }

        public static float InBack(float time, float start, float end, float duration)
        {
            return end * (time /= duration) * time * ((1.70158f + 1) * time - 1.70158f) + start;
        }

        public static float InOutBack(float time, float start, float end, float duration)
        {
            float s = 1.70158f;
            if ((time /= duration / 2) < 1)
                return end / 2 * (time * time * (((s *= (1.525f)) + 1) * time - s)) + start;

            return end / 2 * ((time -= 2) * time * (((s *= (1.525f)) + 1) * time + s) + 2) + start;
        }

        public static float OutInBack(float time, float start, float end, float duration)
        {
            if (time < duration / 2)
                return OutBack(time * 2, start, end / 2, duration);

            return InBack((time * 2) - duration, start + end / 2, end / 2, duration);
        }
        #endregion
    }
    #endregion
}