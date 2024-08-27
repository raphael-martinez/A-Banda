using UnityEngine;

namespace Playmove
{
    [ExecuteInEditMode]
    public class SortingOrder : MonoBehaviour
    {
        [SerializeField]
        private string _sortLayerName = "Default";
        public string SortLayerName
        {
            get
            {
                return _sortLayerName;
            }
            set
            {
                _sortLayerName = value;
            }
        }
        [SerializeField]
        private int _order;
        public int Order
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;
            }
        }

        public bool AlwaysUpdate = false;
        [Header("Afetar filhos, quais componentes:")]
        public bool AffectChildrenRenderer = false;
        public bool AffectChildrenSortingOrder = false;
        [Header("Afetar filhos, quais propriedades:")]
        public bool Name = true;
        public bool Number = true;

        private Renderer _render;
        private Renderer Render
        {
            get
            {
                if (_render == null)
                    _render = GetComponent<Renderer>();
                return _render;
            }
        }

        private Renderer[] _children;
        private Renderer[] Children
        {
            get
            {
                if (_children == null)
                    _children = GetComponentsInChildren<Renderer>(true);
                return _children;
            }
        }

        private SortingOrder[] _soChildren;
        private SortingOrder[] SOChildren
        {
            get
            {
                if (_soChildren == null)
                    _soChildren = GetComponentsInChildren<SortingOrder>(true);
                return _soChildren;
            }
        }

        void Awake()
        {
            ChangeSortingOrder(_order, _sortLayerName);
        }

        void Update()
        {
            if (!AlwaysUpdate) return;
            ChangeSortingOrder();
        }

        private void ChangeSortingOrder()
        {
            if (Render != null)
            {
                Render.sortingOrder = _order;
                Render.sortingLayerName = _sortLayerName;
            }

            if (AffectChildrenRenderer)
            {
                for (int x = 0; x < Children.Length; x++)
                {
                    if (Children[x] != null)
                    {
                        if (Number)
                            Children[x].sortingOrder = _order;
                        if (Name)
                            Children[x].sortingLayerName = _sortLayerName;
                    }
                }
            }
            if (AffectChildrenSortingOrder)
            {
                for (int x = 0; x < SOChildren.Length; x++)
                {
                    if (SOChildren[x] != null)
                    {
                        if (Number)
                            SOChildren[x].Order = _order;
                        if (Name)
                            SOChildren[x].SortLayerName = _sortLayerName;
                    }
                }
            }
        }
        public void ChangeSortingOrder(int order)
        {
            _order = order;
            ChangeSortingOrder();
        }
        public void ChangeSortingOrder(int order, string layerName)
        {
            _order = order;
            _sortLayerName = layerName;
            ChangeSortingOrder();
        }
    }
}