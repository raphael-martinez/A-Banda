using UnityEngine;
using System.Collections;

/// <summary>
/// 
/// </summary>
namespace Playmove
{
    public class ContentControl : MonoBehaviour
    {

        public BarDrag Bar;
        public ContentDrag Content;
        public BarSwype ContentSwype;
        [Range(0, 100)]
        public int ButtonPercentagePerPage;
        public float PercentagePerTime = 1, IteragionPerSecond = 0.05f, ArrowHoldTime = 0.5f;

        public float _timesBigger;
        public float TimesBigger
        {
            get { return _timesBigger; }
            set
            {
                _timesBigger = value;
                Bar.SetSize(value);
            }
        }

        private bool _arrowHolded;

        void Start()
        {
            Bar.onBarDrag.AddListener(BarDragging);
            Content.onContentDrag.AddListener(ContentDragging);
            if (ContentSwype != null)
                ContentSwype.onSwype.AddListener(ContentSwyped);
        }
        #region EventLiteners

        private void ContentSwyped(BarSwype swype)
        {
            if (swype.Direction > 0)
                UpPosition();
            else
                DownPosistion();
        }

        private void ContentDragging(ContentDrag data)
        {
            Bar.UpdateBarPosition(100 - data.Percentage);
        }

        private void BarDragging(BarDrag data)
        {
            Content.UpdateContentPosition(100 - data.Percentage);
        }

        #endregion

        public void SetPosition(float percentage)
        {
            Content.UpdateContentPositionAnimated(percentage);
            Bar.UpdateBarPositionAnimated(100 - percentage);
        }

        public void UpPositionForced()
        {
            Content.UpdateContentPosition(0);
            Bar.UpdateBarPosition(100);
        }

        public void UpPosition()
        {
            if (Content.Percentage != 0)
                Content.UpdateContentPositionAnimated(0);
            if (Bar.Percentage != 100)
                Bar.UpdateBarPositionAnimated(100);
        }

        public void DownPosistion()
        {
            Content.UpdateContentPositionAnimated(100);
            Bar.UpdateBarPositionAnimated(0);
        }

        public void ArrowDownClick()
        {
            if (_arrowHolded) return;

            if (Content.Percentage < 100 - ButtonPercentagePerPage)
                Content.UpdateContentPositionAnimated(Content.Percentage + ButtonPercentagePerPage);
            else
                Content.UpdateContentPositionAnimated(100);

            if (Bar.Percentage > ButtonPercentagePerPage)
                Bar.UpdateBarPositionAnimated(Bar.Percentage - ButtonPercentagePerPage);
            else
                Bar.UpdateBarPositionAnimated(0);
        }

        public void ArrowUpClick()
        {
            if (_arrowHolded) return;

            if (Content.Percentage > ButtonPercentagePerPage)
                Content.UpdateContentPositionAnimated(Content.Percentage - ButtonPercentagePerPage);
            else
                Content.UpdateContentPositionAnimated(0);

            if (Bar.Percentage < 100 - ButtonPercentagePerPage)
                Bar.UpdateBarPositionAnimated(Bar.Percentage + ButtonPercentagePerPage);
            else
                Bar.UpdateBarPositionAnimated(100);
        }

        public void ArrowUpStart()
        {
            StartCoroutine("RoutineArrowUp");
        }

        public void ArrowUpStop()
        {
            StopCoroutine("RoutineArrowUp");
        }

        public void ArrowDownStart()
        {
            StartCoroutine("RoutineArrowDown");
        }

        public void ArrowDownStop()
        {
            StopCoroutine("RoutineArrowDown");
        }

        private IEnumerator RoutineArrowDown()
        {
            _arrowHolded = false;

            yield return new WaitForSeconds(ArrowHoldTime);

            _arrowHolded = true;

            while (true)
            {
                if (Content.Percentage < 100)
                    Content.UpdateContentPosition(Content.Percentage + PercentagePerTime);
                if (Bar.Percentage > 0)
                    Bar.UpdateBarPosition(Bar.Percentage - PercentagePerTime);

                if (Content.Percentage >= 100 || Bar.Percentage <= 0) break;

                yield return new WaitForSeconds(IteragionPerSecond);
            }
        }

        private IEnumerator RoutineArrowUp()
        {
            _arrowHolded = false;

            yield return new WaitForSeconds(ArrowHoldTime);

            _arrowHolded = true;

            while (true)
            {
                if (Content.Percentage > 0)
                    Content.UpdateContentPosition(Content.Percentage - PercentagePerTime);
                if (Bar.Percentage < 100)
                    Bar.UpdateBarPosition(Bar.Percentage + PercentagePerTime);

                if (Content.Percentage <= 0 || Bar.Percentage >= 100) break;

                yield return new WaitForSeconds(IteragionPerSecond);
            }
        }

    }
}