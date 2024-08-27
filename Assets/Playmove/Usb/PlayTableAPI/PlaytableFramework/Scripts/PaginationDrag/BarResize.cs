using UnityEngine;
using System.Collections;

    namespace Playmove { public class BarResize : MonoBehaviour
    {
        [Range(1, 100)]
        public float Size;
        [SerializeField]
        private BarDrag _bardrag;
        private BarDrag BarDrag
        {
            get
            {
                if (!_bardrag) _bardrag = GetComponent<BarDrag>();
                return _bardrag;
            }
        }

        private float _maxSize { get { return BarDrag.BarLimit.bounds.size.y; } }
        private float _minSize { get { return GetComponent<Renderer>().bounds.size.y; } }

        private void Update()
        {
            SetSize(Size);
        }

        public void SetSize(float percentage)
        {
            BarDrag.Holder.localScale = new Vector3(1, (_maxSize * percentage) / 100, 1);
        }
    }
}