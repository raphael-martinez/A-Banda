using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class PYNumberCounter : MonoBehaviour
    {
        protected float _currentValue;
        public int CurrentValue
        {
            get
            {
                return Mathf.CeilToInt(_currentValue);
            }
        }

        public bool IsCounting { get; set; }

        public PYText Text;
        public float MaxValue = 100;
        public float MinValue = 0;
        public float Duration = 1;
        public TagManager.CountDirection CountingDirection;
        public string NumberFormat = "000";

        private float _timer;

        void Start()
        {
        }

        void Update()
        {
            if (!IsCounting)
                return;

            if (CountingDirection == TagManager.CountDirection.Crescent)
                _currentValue = Mathf.Lerp(MinValue, MaxValue, _timer / Duration);
            else
                _currentValue = Mathf.Lerp(MaxValue, MinValue, _timer / Duration);

            _timer += Time.deltaTime;

            if (Text != null)
                Text.Text = CurrentValue.ToString(NumberFormat);

            if (CountingDirection == TagManager.CountDirection.Crescent)
                IsCounting = !(CurrentValue >= MaxValue);
            else
                IsCounting = !(CurrentValue <= MinValue);
        }

        public void StartCounting()
        {
            IsCounting = true;
        }
    }
}