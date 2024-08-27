using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

/// <summary>
/// GameManager irá centralizar todos os scripts de gerenciamento, por exemplo: victoryManager, timerManager.
/// Pode ser colocado nele interações de larga escala de jogo, por exemplo desistir do jogo (botao voltar).
/// </summary>
namespace Playmove
{
    public class PYGameManager : MonoBehaviour
    {

        private static PYGameManager _instance;
        public static PYGameManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<PYGameManager>();

                return _instance;
            }
        }

        [Header("PYGameManager")]
        public TagManager.GameDifficulty GameDifficulty;
        public TagManager.GameState GameState;

        private int _score;
        public int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
            }
        }

        [Serializable]
        public class GameEvent : UnityEvent { }
        [Header("Game Events")]
        public GameEvent onStarted = new GameEvent();
        public GameEvent onCompleted = new GameEvent();
        public GameEvent onPaused = new GameEvent();
        public GameEvent onLost = new GameEvent();
    }
}