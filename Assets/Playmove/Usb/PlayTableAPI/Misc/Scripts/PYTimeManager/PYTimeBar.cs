using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class PYTimeBar : PYProgressBar
    {
        public bool TimeIsRunning;

        protected override void Awake()
        {
            StartValue = PYTimeManager.Instance.TimeCountDirection == TagManager.CountDirection.Crescent ? 0 : PYTimeManager.Instance.TotalGameTime;
            MaxValue = PYTimeManager.Instance.TotalGameTime;

            base.Awake();
        }

        protected override void Update()
        {
            if (!TimeIsRunning)
                return;

            UpdateBarSmooth(PYTimeManager.Instance.CurrentTimeInteger(), 1.2f);
            base.Update();
        }

        public void StartBar()
        {
            TimeIsRunning = true;
        }
    }
}