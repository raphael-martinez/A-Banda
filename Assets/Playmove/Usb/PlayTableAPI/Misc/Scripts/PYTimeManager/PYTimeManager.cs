using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

namespace Playmove
{
    public class PYTimeManager : MonoBehaviour
    {
        [Header("Time Manager")]
        public int TotalGameTime;
        public float TimeScale = 1;
        public TagManager.CountDirection TimeCountDirection;

        [Header("Debug")]
        public float ElapsedGameTime;
        public bool IsPlaying;

        public float CurrentTime
        {
            get
            {
                return GetCurrentTime();
            }
        }

        private static PYTimeManager _instance;
        public static PYTimeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PYTimeManager>();

                return _instance;
            }
        }


        [Serializable]
        public class TimerEvent : UnityEvent { }
        [Header("Events")]
        public TimerEvent onStarted = new TimerEvent();
        public TimerEvent onTick = new TimerEvent();
        public TimerEvent onPaused = new TimerEvent();
        public TimerEvent onResumed = new TimerEvent();
        public TimerEvent onStopped = new TimerEvent();
        public TimerEvent onCompleted = new TimerEvent();

        void Start()
        {
            StartTimer();
        }


        public void StartTimer()
        {
            IsPlaying = true;

            if (ElapsedGameTime == 0)
                onStarted.Invoke();
        }

        public void StartTimer(int totalGameTime)
        {
            TotalGameTime = totalGameTime;
            StartTimer();
        }

        public void StartTimer(int totalGameTime, TagManager.CountDirection timeDirection)
        {
            TotalGameTime = totalGameTime;
            TimeCountDirection = timeDirection;
            StartTimer();
        }

        public void PauseTimer()
        {
            IsPlaying = false;
            onPaused.Invoke();
        }

        public void ResumeTimer()
        {
            IsPlaying = true;
            onResumed.Invoke();
        }

        public void ResetTimer()
        {
            ElapsedGameTime = 0;
        }

        public void StopTimer()
        {
            IsPlaying = false;
            ResetTimer();
            onStopped.Invoke();
        }

        private int _secondsTimer;
        private void FixedUpdate()
        {
            if (!IsPlaying) return;

            ElapsedGameTime += Time.deltaTime * TimeScale;

            if (ElapsedGameTime - _secondsTimer >= 1)
            {
                _secondsTimer = Mathf.FloorToInt(ElapsedGameTime);
                onTick.Invoke();
            }

            if (ElapsedGameTime >= TotalGameTime)
            {
                TimerCompleted();
            }
        }

        private void TimerCompleted()
        {
            IsPlaying = false;
            onCompleted.Invoke();
        }

        public int CurrentTimeInteger()
        {
            return Mathf.RoundToInt(CurrentTime);
        }

        private float GetCurrentTime()
        {
            switch (TimeCountDirection)
            {
                case TagManager.CountDirection.Crescent:
                default:
                    return ElapsedGameTime;

                case TagManager.CountDirection.Decrescent:
                    return TotalGameTime - ElapsedGameTime;
            }
        }
    }
}