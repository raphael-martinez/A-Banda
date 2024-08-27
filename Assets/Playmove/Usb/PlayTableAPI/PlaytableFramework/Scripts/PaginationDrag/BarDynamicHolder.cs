using UnityEngine;

namespace Playmove
{
    [ExecuteInEditMode]
 public class BarDynamicHolder : MonoBehaviour
    {
        [SerializeField]
        private Renderer _top;
        [SerializeField]
        private Renderer _down;

        private BoxCollider2D _myCollider;
        private BoxCollider2D MyCollider
        {
            get
            {
                if (!_myCollider) _myCollider = GetComponentInChildren<BoxCollider2D>();
                return _myCollider;
            }
        }

        private Renderer _middle;
        private Renderer Middle
        {
            get
            {
                if (!_middle) _middle = GetComponentInChildren<Renderer>();
                return _middle;
            }
        }

        void LateUpdate()
        {
            if (_top != null)
                _top.transform.position = new Vector3(Middle.transform.position.x, Middle.bounds.max.y, Middle.transform.position.z);
            if (_down != null)
                _down.transform.position = new Vector3(Middle.transform.position.x, Middle.bounds.min.y, Middle.transform.position.z);

            if (!MyCollider)
                MyCollider.size = Middle.bounds.size;
        }
    }
}