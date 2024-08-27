using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Playmove
{

    public abstract class PYScoreNavigation : MonoBehaviour
    {
        [Header("PYScoreNavigation")]
        public Transform ContentObj;
        public GameObject RecordItemObj;

        public float LineSpace = 0.6f;
        public string EmptyRecordText = "---";

        public int AmountRecordsItemPerPage = 10;

        [Tooltip("É usado apenas para desativar os botões quando tiver animação")]
        public PYButton[] ButtonsNavigation;

        protected PYPagination _paginator;

        protected List<GameObject> _recordItems = new List<GameObject>();
        protected List<Student> _students = new List<Student>();

        protected int _counterSettingRecordItems = 0;

        protected virtual void Start()
        {
            CreateRecordItems();

            // Create new Pagination with default values
            _paginator = new PYPagination(0, AmountRecordsItemPerPage, 1);
        }

        public virtual void Show()
        {
            EnableNavigationButtons(false);
            Refresh();
        }

        public virtual void Refresh(int? page = null)
        {
            _paginator.TotalElements = _students.Count;
            if (page.HasValue)
                _paginator.NavigateToPage(page.Value);
            else
                _paginator.NavigateToPage(1);
            AnimateRecordItems();
        }

        public virtual void NavigateBackward()
        {
            if (_students.Count == 0)
                return;

            EnableNavigationButtons(false);
            _paginator.NavigateLeft();
            AnimateRecordItems();
        }

        public virtual void NavigateForward()
        {
            if (_students.Count == 0)
                return;

            EnableNavigationButtons(false);
            _paginator.NavigateRight();
            AnimateRecordItems();
        }

        protected abstract void AnimateRecordItems();
        protected abstract void CompletedSettingRecordItems();
        /*
         * Trecho de código apenas como exemplo de uma implementação simples
        protected override void CompletedSettingRecordItems()
        {
            _counterSettingRecordItems++;
            if (_counterSettingRecordItems >= AmountRecordsItemPerPage)
            {
                _counterSettingRecordItems = 0;
                UpdateButtonsNavigationState();
            }
        }
         */

        protected virtual void CreateRecordItems()
        {
            // If we already have all the necessary recordItens created just return
            if (ContentObj.childCount >= AmountRecordsItemPerPage) return;

            RecordItemObj.name = "RecordItem0";
            _recordItems.Add(RecordItemObj);
            for (int x = 0; x < AmountRecordsItemPerPage - 1; x++)
            {
                GameObject item = (GameObject)Instantiate(RecordItemObj);
                item.name = "RecordItem" + (x + 1);
                item.transform.SetParent(ContentObj);
                item.transform.localPosition = RecordItemObj.transform.localPosition - (Vector3.up * LineSpace) * (x + 1);
                _recordItems.Add(item);
            }
        }

        protected void EnableNavigationButtons(bool isEnabled)
        {
            foreach (PYButton btn in ButtonsNavigation)
                btn.IsEnabled = isEnabled;
        }

        protected void UpdateButtonsNavigationState()
        {
            foreach (PYButton btn in ButtonsNavigation)
                btn.IsEnabled = _students.Count != 0 && _paginator.TotalPages > 1;
        }
    }
}