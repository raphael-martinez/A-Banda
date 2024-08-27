using UnityEngine;
using System.Collections;

namespace Playmove
{
    public class ScoreMenuManager : PYSceneManager
    {
        private static ScoreMenuManager _instance = null;
        public static ScoreMenuManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<ScoreMenuManager>();
                return _instance;
            }
        }

        [Header("Buttons")]
        public PYButton VoltarButton;

        protected override void Start()
        {
            base.Start();

            VoltarButton.onClick.AddListener((sender) =>
            {
                ChangeScene(TagManager.Scenes.MainMenu);
            });
        }
    }
}