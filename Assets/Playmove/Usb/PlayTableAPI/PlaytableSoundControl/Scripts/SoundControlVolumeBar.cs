using UnityEngine;
using UnityEngine.EventSystems;

namespace Playmove
{
    public class SoundControlVolumeBar : PYButton, IDragHandler
    {
        public GameObject marker, volumeBar;
        public TextMesh volumeText;
        public Font volumeTextFont;
        public float borderSize = 0.1f;

        [SerializeField]
        private float _heightCorrection = 0.5f;

        private Vector3 _volumeBarStartScale;
        private float _minPos;
        private float _maxPos;
        private Bounds _colliderBounds;
        private Vector3 _markerPos;
        private Vector3 _lastPos;

        protected override void Awake()
        {
            base.Awake();
            _volumeBarStartScale = volumeBar.transform.localScale;

            _colliderBounds = GetComponentInChildren<Collider2D>().bounds;
            _minPos = _colliderBounds.center.y - _colliderBounds.size.y / 2 + borderSize;
            _maxPos = _colliderBounds.center.y + _colliderBounds.size.y / 2 - borderSize;

            if (volumeTextFont != null)
                volumeText.font = volumeTextFont;
        }

        protected override void DownAction()
        {
            UpdateBarByMousePosition();
        }

        #region EventSystem

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsActive() || !IsEnabled)
                return;

            base.OnPointerDown(eventData);
            EventSystem.current.SetSelectedGameObject(OwnGameObject, eventData);
            _lastPos = eventData.pointerCurrentRaycast.worldPosition;
            UpdateBarByMousePosition();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _lastPos = eventData.pointerCurrentRaycast.worldPosition;
            UpdateBarByMousePosition();
        }

        protected override void UpAction()
        {
            //PlaytableWin32.Instance.Data.SetVolume(((int)(SoundControlButton.Instance.Volume * 100)));
        }

        #endregion EventSystem

        public void UpdateBarByMousePosition()
        {
            SetMarker(_lastPos.y);

            _colliderBounds = GetComponentInChildren<Collider2D>().bounds;
            _minPos = _colliderBounds.center.y - _colliderBounds.size.y / 2 + borderSize;
            _maxPos = _colliderBounds.center.y + _colliderBounds.size.y / 2 - borderSize;

            SoundControlButton.Instance.Volume = ((_markerPos.y - _minPos) / (_colliderBounds.size.y - borderSize * 2));

            SetBarVolume();
        }

        public void UpdateBarByVolume()
        {
            if (SoundControlButton.Instance.Volume == 1) return;
            CalculateMarkPosition();
            SetBarVolume();
        }

        private void SetBarVolume()
        {
            _colliderBounds = GetComponentInChildren<Collider2D>().bounds;
            volumeText.text = Mathf.RoundToInt(SoundControlButton.Instance.Volume * 100).ToString();
            RepositionBarFill(_markerPos.y, _colliderBounds.min.y - _heightCorrection);
            ScaleBar();
        }

        private void SetMarker(float height)
        {
            _markerPos = marker.transform.position;
            _markerPos.y = Mathf.Clamp(height, _minPos, _maxPos);
            _markerPos.z = transform.parent.position.z - .01f;
            marker.transform.position = _markerPos;
        }

        private void RepositionBarFill(float barHeight, float colliderBottom)
        {
            Vector3 pos = volumeBar.transform.position;
            pos.y = (barHeight + colliderBottom) / 2;
            volumeBar.transform.position = pos;
        }

        private void CalculateMarkPosition()
        {
            _colliderBounds = GetComponentInChildren<Collider2D>().bounds;
            float height = CalculateHeight(SoundControlButton.Instance.Volume * 100, _colliderBounds.min.y, _colliderBounds.max.y);
            SetMarker(height);
        }

        private void ScaleBar()
        {
            Vector3 barScale = _volumeBarStartScale;
            barScale.y = _volumeBarStartScale.y * SoundControlButton.Instance.Volume;
            volumeBar.transform.localScale = barScale;
        }

        private float CalculatePercentage(float yPos, float min, float max)
        {
            return (100 * (yPos - min)) / (max - min);
        }

        private float CalculateHeight(float percentage, float min, float max)
        {
            return ((percentage * (max - min)) / 100) + min;
        }
    }
}