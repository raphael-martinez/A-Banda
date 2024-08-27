using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Playmove
{
    public class SoundControlButton : PYButton, IDragHandler
    {
        #region Instance

        private static SoundControlButton instance;

        public static SoundControlButton Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<SoundControlButton>();
                return instance;
            }
        }

        #endregion Instance

        [Header("Configuration")]
        [SerializeField]
        private bool _useSoundFader;

        [SerializeField]
        private float _defaultVolume = 0.2f;

        [SerializeField, Tooltip("Valor que condiz com a velocidade e tempo de vida da particula de acordo com a posição do mouse.")]
        private float _trailDeltaSpeed = 10;

        [SerializeField]
        private Vector2 _openPosition = new Vector2(-0.3f, 0.2f);

        #region Reference variables

        [Header("References")]
        [SerializeField]
        private SpriteRenderer Icon;

        [SerializeField]
        private GameObject Dots;

        [SerializeField]
        private SoundControlVolumeBar VolumeBar;

        [SerializeField]
        private GameObject VolumeBalloon;

        [SerializeField]
        private ParticleSystem MouseTrail;

        [SerializeField]
        private ParticleSystem Effect;

        [SerializeField]
        private GameObject TopArrow;

        [SerializeField]
        private Vector3 TopArrowOpen = new Vector3(-0.015f, 2.9f, 0);

        [SerializeField]
        private GameObject LeftArrow;

        [SerializeField]
        private Vector3 LeftArrowOpen = new Vector3(-1.5f, 0, 0);

        [SerializeField]
        private float TweenTime = 0.5f;

        [SerializeField]
        private SpriteRenderer _soundFader;

        [SerializeField]
        private Transform _root;

        #endregion Reference variables

        #region Properties

        private Transform MyTransform { get { return OwnTransform; } }

        private float _volume;

        public float Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
                PYAudioManager.GlobalVolume = _volume * (Mute ? 0 : 1);
            }
        }

        private bool _mute;

        public bool Mute
        {
            get
            {
                return _mute;
            }
            set
            {
                _mute = value;
                PYAudioManager.GlobalVolume = Volume * (_mute ? 0 : 1);
                if (_mute)
                {
                    //PlaytableWin32.Instance.Data.DoMute();
                }
                else
                {
                    //PlaytableWin32.Instance.Data.DoUnMute();
                }
            }
        }

        private SoundControlMuteButton MuteButton { get { return GetComponentInChildren<SoundControlMuteButton>(true); } }

        public bool IsDragging { private set; get; }
        public bool IsOpen { private set; get; }

        private PYTweenAnimation _soundAlphaAnimation;

        public PYTweenAnimation SoundAlphaAnimation
        {
            get
            {
                if (!_soundAlphaAnimation)
                {
                    _soundAlphaAnimation = PYTweenAnimation.AddNew(_soundFader.gameObject, 362346)
                        .SetDuration(0.3f)
                        .SetEaseType(Ease.Type.OutSine)
                        .SetAlpha(0f, 0.6f);
                }
                return _soundFader.transform.GetComponent<PYTweenAnimation>();
            }
        }

        #endregion Properties

        #region Animations

        private PYTweenAnimation _iconClickAnimation;

        private PYTweenAnimation IconClickAnimation
        {
            get
            {
                if (!_iconClickAnimation)
                    _iconClickAnimation = PYTweenAnimation.Add(Icon.gameObject)
                //.SetScale(icon.transform.localScale, (_menuIsOpen ? new Vector3(0.8f, 0.8f, 1f) : Vector3.one))
                .SetScale(Vector3.one, Vector3.one * 0.8f)
                .SetDuration(TweenTime)
                .SetEaseType(Ease.Type.OutElastic);

                return _iconClickAnimation;
            }
        }

        private PYTweenAnimation _upArrowAnimation;

        private PYTweenAnimation UpArrowAnimation
        {
            get
            {
                if (!_upArrowAnimation)
                    _upArrowAnimation = PYTweenAnimation.Add(TopArrow)
                        //.SetPosition((_menuIsOpen ? _arrowUpClose : ArrowUpOpen), (_menuIsOpen ? ArrowUpOpen : _arrowUpClose), true)
                        .SetPosition(_arrowUpClose, TopArrowOpen, true)
                        .SetDuration(TweenTime)
                        .SetEaseType(Ease.Type.OutElastic);
                return _upArrowAnimation;
            }
        }

        private PYTweenAnimation _leftArrowAnimation;

        private PYTweenAnimation LeftArrowAnimation
        {
            get
            {
                if (!_leftArrowAnimation)
                    _leftArrowAnimation = PYTweenAnimation.Add(LeftArrow)
                        //.SetPosition((_menuIsOpen ? _arrowLeftClose : ArrowLeftOpen), (_menuIsOpen ? ArrowLeftOpen : _arrowLeftClose), true)
                        .SetPosition(_arrowLeftClose, LeftArrowOpen, true)
                        .SetDuration(TweenTime)
                        .SetEaseType(Ease.Type.OutElastic);
                return _leftArrowAnimation;
            }
        }

        private PYTweenAnimation _volumeBarAnimation;

        private PYTweenAnimation VolumeBarAnimation
        {
            get
            {
                if (!_volumeBarAnimation)
                    _volumeBarAnimation = PYTweenAnimation.AddNew(VolumeBar.gameObject, 1)
                        .SetScale(TagManager.Axis.Y, 0, 1)
                        .SetDuration(TweenTime)
                        //.SetAlpha((_menuIsOpen ? 0 : 1), (_menuIsOpen ? 1 : 0))
                        //.SetAlpha(0f, 1f)
                        .SetEaseType(Ease.Type.OutElastic);
                return _volumeBarAnimation;
            }
        }

        private PYTweenAnimation _volumeBarAlphaAnimation;

        private PYTweenAnimation VolumeBarAlphaAnimation
        {
            get
            {
                if (!_volumeBarAlphaAnimation)
                    _volumeBarAlphaAnimation = PYTweenAnimation.AddNew(VolumeBar.gameObject, 2)
                        .SetDuration(TweenTime)
                        .SetAlpha(0f, 1f)
                        .SetEaseType(Ease.Type.OutExpo);
                return _volumeBarAlphaAnimation;
            }
        }

        private PYTweenAnimation _muteButtonShowAnimation;

        private PYTweenAnimation MuteButtonShowAnimation
        {
            get
            {
                if (!_muteButtonShowAnimation)
                    _muteButtonShowAnimation = PYTweenAnimation.Add(MuteButton.gameObject)
                .SetScale(TagManager.Axis.X, 0.5f, 1)
                .SetAlpha(0f, 1f)
                .SetDuration(TweenTime)
                .SetEaseType(Ease.Type.OutElastic);
                return _muteButtonShowAnimation;
            }
        }

        private PYTweenAnimation _ballonValueAnimation;

        private PYTweenAnimation BallonValueAnimation
        {
            get
            {
                if (!_ballonValueAnimation)
                    _ballonValueAnimation = PYTweenAnimation.Add(VolumeBalloon)
                        .SetAlpha(0f, 1f)
                        .SetScale(TagManager.Axis.X, 0, 1)
                        .SetDelay(TweenTime * 0.15f)
                        .SetDuration(TweenTime)
                        .SetEaseType(Ease.Type.OutElastic);
                return _ballonValueAnimation;
            }
        }

        private PYTweenAnimation _discreteAnimation;

        private PYTweenAnimation DiscreteAnimation
        {
            get
            {
                if (!_discreteAnimation)
                    _discreteAnimation = PYTweenAnimation.AddNew(_root.gameObject, 55)
                        .SetEaseType(Ease.Type.InOutSine)
                        .SetDuration(0.5f);

                return _discreteAnimation;
            }
        }

        private PYTweenAnimation _rootTranslation;

        private PYTweenAnimation RootTranslation
        {
            get
            {
                if (!_rootTranslation)
                    _rootTranslation = PYTweenAnimation.AddNew(_root.gameObject, 12315)
                        .SetDuration(0.25f)
                        .SetEaseType(Ease.Type.OutSine)
                        .SetPosition(Vector3.zero, _openPosition, true);
                return _rootTranslation;
            }
        }

        #endregion Animations

        private bool _isOverMuteButton;
        private bool _isOverVolumeBar;
        private bool _menuIsOpen = false;
        private bool _discrete = false;

        [SerializeField]
        private bool _isAvailable = true;

        private Vector3 _arrowUpClose, _arrowLeftClose;
        private Action _muteCallback;
        private DelayedCall _setIsDragging = new DelayedCall();
        private DelayedCall _setIsOpen = new DelayedCall();
        private DelayedCall _setAvailable = new DelayedCall();
        private DelayedCall _enableVolumeCollider = new DelayedCall();

        #region Unity

        protected override void Awake()
        {
            base.Awake();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            //DontDestroyOnLoad(gameObject);

            _arrowLeftClose = LeftArrow.transform.localPosition;
            _arrowUpClose = TopArrow.transform.localPosition;

#if RELEASE
            PlaytableWin32.Instance.onGameValidate.AddListener(SyncData);
#endif
        }

        private void SyncData()
        {
            //PlaytableWin32.Instance.Data.GetMute(SynchronizeMuteState);
            //PlaytableWin32.Instance.Data.GetVolume(SynchronizeVolume);
        }

        private void Update()
        {
            if (_menuIsOpen &&
                EventSystem.current.currentSelectedGameObject != OwnGameObject &&
                EventSystem.current.currentSelectedGameObject != VolumeBar.OwnGameObject &&
                EventSystem.current.currentSelectedGameObject != MuteButton.OwnGameObject)
                Close();
        }

        protected override void Start()
        {
            base.Start();
            if (Instance != null && Instance != this)
                return;

            SetCallbacks();

            Volume = _defaultVolume;
            EnableElements(false, 0);
            _isAvailable = true;
        }

        private void SynchronizeMuteState(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError("String is null or empty. Verify your PlayTableAPI version.");
            }
            else
            {
                SetMute(text == "true");
            }

            UpdateSprite();
        }

        private void SynchronizeVolume(int volume)
        {
            Volume = (float)volume / 100;
            StartCoroutine(SyncronizeBarVisual());
        }

        private IEnumerator SyncronizeBarVisual()
        {
            VolumeBar.gameObject.SetActive(true);
            VolumeBar.transform.localScale = Vector3.one;
            VolumeBar.UpdateBarByVolume();
            VolumeBar.transform.localScale = Vector3.zero;
            yield return new WaitForEndOfFrame();
            VolumeBar.gameObject.SetActive(false);
        }

        private void SetCallbacks()
        {
            MuteButton.SetCallback(SetMute);
        }

        #endregion Unity

        #region EventSystem

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled)
                return;

            base.OnPointerDown(eventData);
            /// NOTE: Select in EventSystem selected object,
            /// when different object is selected the button will close
            EventSystem.current.SetSelectedGameObject(OwnGameObject, eventData);
        }

        protected override void DownAction()
        {
            if (!_isAvailable) return;
            if (_menuIsOpen)
                Close();
            else
                Open();
        }

        protected override void UpAction()
        {
            if (_isOverMuteButton)
            {
                SetMute();
            }

            IsDragging = false;
            _setIsDragging.Cancel();

            TrailParticle(false);

            //PlaytableWin32.Instance.Data.SetVolume(((int)(Volume * 100)));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_setIsDragging.running && !IsDragging)
            {
                _setIsDragging.Start(this, () => { IsDragging = true; }, 0.3f);
            }

            Vector3 mousePos = eventData.pointerCurrentRaycast.worldPosition;
            Bounds muteButtonBounds = MuteButton.GetComponentInChildren<Collider2D>().bounds;
            Vector3 muteButtonPos = new Vector3(mousePos.x, mousePos.y, muteButtonBounds.center.z);
            _isOverMuteButton = (muteButtonBounds.Contains(muteButtonPos));

            if (!IsDragging) return;

            Bounds volumeBarBounds = VolumeBar.GetComponentInChildren<Collider2D>().bounds;
            Vector3 volumeBarPos = new Vector3(mousePos.x, mousePos.y, volumeBarBounds.center.z);
            _isOverVolumeBar = (volumeBarBounds.Contains(volumeBarPos));
            if (_isOverVolumeBar)
            {
                VolumeBar.UpdateBarByMousePosition();
            }

            TrailParticle(true, eventData);
        }

        #endregion EventSystem

        public void SetMute()
        {
            SetMute(!Mute);
        }

        public void SetMute(bool mute)
        {
            Mute = mute;
            UpdateSprite();
            SetAvailable(true);
            Effect.Emit(_menuIsOpen ? 1 : 0);

            if (_menuIsOpen)
                Close();

            if (_muteCallback != null)
            {
                _muteCallback();
            }
        }

        public void SetMuteCallback(Action callback)
        {
            _muteCallback = callback;
        }

        public void Open()
        {
            if (!_isAvailable && !IsDragging) return;

            _menuIsOpen = true;

            IconClickAnimation.Stop();
            IconClickAnimation.Play();

            UpArrowAnimation.Stop();
            UpArrowAnimation.Play();

            LeftArrowAnimation.Stop();
            LeftArrowAnimation.Play();

            Show();

            RootTranslation.Stop();
            RootTranslation.Play();

            if (_useSoundFader)
            {
                _soundFader.gameObject.SetActive(true);
                SoundAlphaAnimation.Stop();
                SoundAlphaAnimation.Play();
            }

            Initialize();

            EnableElements(true, TweenTime);
        }

        public void Close()
        {
            if (!_isAvailable && !IsDragging) return;

            _menuIsOpen = false;

            IconClickAnimation.Stop();
            IconClickAnimation.Reverse();

            UpArrowAnimation.Stop();
            UpArrowAnimation.Reverse();

            LeftArrowAnimation.Stop();
            LeftArrowAnimation.Reverse();

            if (_discrete)
                TurnDiscrete();

            RootTranslation.Stop();
            RootTranslation.Reverse();

            if (_useSoundFader)
            {
                SoundAlphaAnimation.Stop();
                SoundAlphaAnimation.Reverse(() => _soundFader.gameObject.SetActive(false));
            }

            Initialize();

            EnableElements(false, TweenTime);
        }

        public void TurnDiscrete()
        {
            _discrete = true;
            DiscreteAnimation
                .SetAlpha(1f, 0.2f)
                .SetScale(MyTransform.localScale, Vector3.one * 0.8f);
            DiscreteAnimation.Play();
        }

        public void RemoveDiscrete()
        {
            Show(0, true);
        }

        private void Initialize()
        {
            _setIsOpen.Cancel();
            _isOverVolumeBar = false;
            _isOverMuteButton = false;
            _isAvailable = false;
            _setAvailable.Start(this, () => { SetAvailable(true); }, 0.5f);
            _setIsOpen.Start(this, () => { IsOpen = _menuIsOpen; }, _menuIsOpen ? 0 : 0.2f);
        }

        private void Show(float delay = 0, bool removeDiscrete = false)
        {
            if (IsOpen) return;

            if (_discrete)
                DiscreteAnimation
                   .SetAlpha(0.2f, 1f)
                   .SetScale(Vector3.one * 0.8f, Vector3.one);
            else
                DiscreteAnimation
                    .SetAlpha(1f, 1f)
                    .SetScale(Vector3.one, Vector3.one);

            if (removeDiscrete)
                _discrete = false;

            gameObject.SetActive(true);
            DiscreteAnimation.Play(delay);
        }

        private void Hide(float delay = 0)
        {
            DiscreteAnimation
                .SetAlpha(0f, 1f)
                .SetScale(Vector3.one * 0.8f, Vector3.one);

            float oldDelay = DiscreteAnimation.DelayToStart;
            DiscreteAnimation.DelayToStart = delay;
            DiscreteAnimation.Reverse(() =>
            {
                DiscreteAnimation.DelayToStart = oldDelay;
                gameObject.SetActive(false);
            });
        }

        private void EnableElements(bool enabled, float tweenTime)
        {
            VolumeBar.gameObject.SetActive(enabled);
            MuteButton.gameObject.SetActive(enabled);
            Dots.gameObject.SetActive(enabled);

            if (!enabled) return;

            VolumeBar.transform.localScale = new Vector3(1, 0, 1);
            MuteButton.transform.localScale = new Vector3(0, 1, 1);

            VolumeBar.GetComponentInChildren<Collider2D>().enabled = false;
            _enableVolumeCollider.Start(this, () => { VolumeBar.GetComponentInChildren<Collider2D>().enabled = true; }, tweenTime * 0.7f);

            VolumeBarAnimation.Stop();
            VolumeBarAlphaAnimation.Stop();

            BallonValueAnimation.Stop();
            VolumeBarAnimation.Play();

            VolumeBarAlphaAnimation.Play();
            BallonValueAnimation.Play();

            MuteButton.GetComponent<Collider2D>().enabled = enabled;

            MuteButtonShowAnimation.Play();

            MuteButton.AnimateHover.Reverse();
        }

        private void SetAvailable(bool value)
        {
            _isAvailable = value;
        }

        private void UpdateSprite()
        {
            Icon.sprite = Resources.Load<Sprite>("sound" + (Mute ? "Off" : "On"));
            MuteButton.UpdateSprite(Mute);
        }

        private void TrailParticle(bool emit, PointerEventData pointer = null)
        {
            ParticleSystem.EmissionModule emission = MouseTrail.emission;
            ParticleSystem.MinMaxCurve rate = emission.rate;
            rate.constantMax = rate.constantMin = 0;
            emission.rate = rate;

            if (!emit) return;
            rate.constantMax = rate.constantMin = 15;

            /// XXX: Tudo é convertido para a mesma escala, fazendo com que toda cena funcione normalmente,
            /// independente da posição do collider de fundo
            /// ou se a camera é perspectiva ou ortografica.
            Vector2 pointerPosition = Camera.main.ScreenToViewportPoint(pointer.position);
            Vector2 myPos = Camera.main.WorldToViewportPoint(transform.position);

            float dist = Vector2.Distance(myPos, pointerPosition) * _trailDeltaSpeed;

            MouseTrail.transform.rotation = Quaternion.LookRotation(pointerPosition - myPos);
            MouseTrail.startLifetime = dist / MouseTrail.startSpeed;
            MouseTrail.startSpeed = dist;
        }
    }

    internal class DelayedCall
    {
        public bool running = false;

        public void Start(MonoBehaviour monoBehaviour, Action action, float delay)
        {
            monoBehaviour.StartCoroutine(DelayedCallback(action, delay));
        }

        public void Cancel()
        {
            running = false;
        }

        private IEnumerator DelayedCallback(Action callback, float delay)
        {
            running = true;
            while (delay > 0 && running)
            {
                delay -= Time.deltaTime;
                yield return null;
            }

            if (running)
            {
                if (callback != null)
                {
                    callback();
                }
            }
        }
    }
}