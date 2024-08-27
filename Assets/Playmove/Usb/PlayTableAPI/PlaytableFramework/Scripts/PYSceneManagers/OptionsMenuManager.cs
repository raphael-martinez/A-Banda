using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class OptionsMenuManager : PYSceneManager
    {
        private static OptionsMenuManager _instance;
        public static OptionsMenuManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<OptionsMenuManager>();
                return _instance;
            }
        }

        [Header("Buttons")]
        public PYButton VoltarButton;

        void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        protected override void Start()
        {
            base.Start();

            VoltarButton.onClick.AddListener((sender) =>
            {
                ChangeScene(MainMenuManager.Instance);
            });
        }
    }
}