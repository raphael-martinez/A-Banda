using System;
using System.Collections.Generic;

namespace Playmove
{
    public class PYPagination
    {
        private int _totalElements = 30;

        public int TotalElements
        {
            get
            {
                return _totalElements;
            }
            set
            {
                _totalElements = value;
                NavigateToPage(CurrentPage);
            }
        }

        private int _elementsPerPage = 8;

        public int ElementsPerPage
        {
            get
            {
                return _elementsPerPage;
            }
            set
            {
                _elementsPerPage = value;
                NavigateToPage(CurrentPage);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="totalElements"></param>
        /// <param name="elementsPerPage"></param>
        /// <param name="currentPage">Página inicial. O valor 1 significa a primeira página</param>
        public PYPagination(int totalElements, int elementsPerPage, int currentPage)
        {
            _totalElements = totalElements;
            _elementsPerPage = elementsPerPage;

            NavigateToPage(currentPage);
        }

        public event Action<int[]> OnPageChanged;

        public int TotalPages
        {
            get
            {
                if (_totalElements % _elementsPerPage == 0)
                    return _totalElements / _elementsPerPage;
                else
                    return _totalElements / _elementsPerPage + 1;
            }
        }

        private int _currentPage;
        public int CurrentPage { get { return _currentPage + 1; } }

        public int LastPage { get; private set; }

        private List<int> _indexes = new List<int>();
        public List<int> Indexes { get { return _indexes; } }

        /// <summary>
        /// Navega para uma pagina especifica.
        /// Irá retornar todos os índices dessas páginas,
        /// os indeces com o valor -1 é que não foram encontrados,
        /// então na parte visual deveriam estar escondidos.
        /// </summary>
        /// <param name="page">Página que desejar ir. O valor 1 significa a primeira página</param>
        /// <returns>Retorna todos os indeces que devem ter na página</returns>
        public int[] NavigateToPage(int page)
        {
            LastPage = _currentPage;
            _currentPage = page - 1;
            if (CurrentPage > TotalPages)
                _currentPage = 0;
            else if (_currentPage < 0)
                _currentPage = TotalPages - 1;

            CalculateIndexes();

            if (OnPageChanged != null)
                OnPageChanged(_indexes.ToArray());

            return _indexes.ToArray();
        }

        /// <summary>
        /// Navega para a direita da página atual.
        /// Irá retornar todos os índices dessas páginas,
        /// os indeces com o valor -1 é que não foram encontrados,
        /// então na parte visual deveriam estar escondidos.
        /// </summary>
        /// <returns>Retorna todos os indeces que devem ter na página</returns>
        public int[] NavigateRight()
        {
            LastPage = _currentPage;
            _currentPage++;
            if (CurrentPage > TotalPages)
                _currentPage = 0;

            CalculateIndexes();

            if (OnPageChanged != null)
                OnPageChanged(_indexes.ToArray());

            return _indexes.ToArray();
        }

        /// <summary>
        /// Navega para a esquerda da página atual.
        /// Irá retornar todos os índices dessas páginas,
        /// os indeces com o valor -1 é que não foram encontrados,
        /// então na parte visual deveriam estar escondidos.
        /// </summary>
        /// <returns>Retorna todos os indeces que devem ter na página</returns>
        public int[] NavigateLeft()
        {
            LastPage = CurrentPage;
            _currentPage--;
            if (_currentPage < 0)
                _currentPage = TotalPages - 1;

            CalculateIndexes();

            if (OnPageChanged != null)
                OnPageChanged(_indexes.ToArray());

            return _indexes.ToArray();
        }

        private void CalculateIndexes()
        {
            _indexes.Clear();
            for (int x = 0; x < _elementsPerPage; x++)
            {
                int index = _elementsPerPage * _currentPage + x;
                _indexes.Add((index < _totalElements) ? index : -1);
            }
        }
    }
}