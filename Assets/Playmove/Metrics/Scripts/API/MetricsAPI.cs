using Playmove.Avatars.API;
using Playmove.Avatars.API.Models;
using Playmove.Core;
using Playmove.Metrics.API.Models;
using Playmove.Metrics.API.Services;
using Playmove.Metrics.API.Vms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Metrics.API
{
    /// <summary>
    /// Responsible to handle APIs for the Metrics functionality
    /// StartMatch = Should be called when the gameplay is about to start
    /// and can only be called one time per match, after an EndMatch
    /// you can call it again to star another match
    /// StartEvent = Should be called when any Event/Question happens in
    /// your gameplay, this should only be called again when an EndEvent
    /// happens
    /// EndEvent = Should be called when the player answer the Event/Question
    /// this will end the event and you can call the StartEventAgain
    /// </summary>
    public static class MetricsAPI
    {
        private static Session _currentSession = null;
        private static Match _currentMatch = null;
        private static List<Stage> _stages = new List<Stage>();
        private static int _currentStage = 0;
        private static List<StageEvent> _currentStageEvents = new List<StageEvent>();

        private static ScoreService _score;
        /// <summary>
        /// Service responsible to handle Score APIs 
        /// </summary>
        public static ScoreService Score
        {
            get
            {
                if (_score == null)
                    _score = new ScoreService();
                return _score;
            }
        }

        private static MetricService _metrics;
        /// <summary>
        /// Service responsible to handle Score APIs
        /// </summary>
        public static MetricService Metrics
        {
            get
            {
                if (_metrics == null)
                    _metrics = new MetricService();
                return _metrics;
            }
        }

        /// <summary>
        /// Start session for the game
        /// This should be called when a game just started,
        /// If this method is called again before the EndSession the last StartSession
        /// will be ignored with all its Events
        /// </summary>
        public static void StartSession()
        {
            Metrics.StartSession(
                (result) => 
                {
                _currentSession = result.Data;
                });
        }

        /// <summary>
        /// End session for the game
        /// This should be called when a game will end,
        /// </summary>
        public static void EndSession(AsyncCallback<bool> completed = null)
        {
            if (_currentSession != null)
                Metrics.EndSession(_currentSession.Id, completed);
        }

        /// <summary>
        /// Start a match for this game
        /// This should be called when a gameplay match is about to start,
        /// If this method is called again before the EndMatch the last StartMatch
        /// will be ignored with all its Events
        /// </summary>
        /// <param name="gameMode">Your game mode ou category of this match</param>
        /// <param name="difficulty">Difficulty of this match</param>
        /// <param name="totalStages">Total of stages in this match</param>
        public static void StartMatch(string gameMode, string difficulty, int totalStages)
        {
            _currentStageEvents.Clear();
            _stages.Clear();
            _currentMatch = new Match
            {
                TotalStages = totalStages,
                SessaoID = _currentSession.Id,
                StartDate = DateTime.Now,
                GameMode = gameMode,
                Difficulty = difficulty,
                Players = AvatarAPI.AllPlayersId
            };

            Metrics.StartMatch(_currentMatch, (result) =>
                {
                    if (result.HasError)
                        Debug.LogError("Não foi possível iniciar a partida");

                    _currentMatch.Id = result.Data;
                    StartStage();
                });
        }

        /// <summary>
        /// Start an Metrics stage
        /// If this method is called before EndStage
        /// the Stage will be ignored with all its Events
        /// </summary>
        public static void StartStage(int stage = 0)
        {
            if (_stages.Count > 0)
                if(_stages[_currentStage] != null)
                {
                    _currentStageEvents.Clear();
                    _stages.Remove(_stages[_currentStage]);
                }
            // ---
            _stages.Add(new Stage
            {
                StageIndex = stage,
                PartidaID = _currentMatch.Id,
                StartDate = DateTime.Now,
            });
            _currentStage = _stages.Count - 1;
        }

        /// <summary>
        /// Ends the current Stage and set
        /// the events related to it
        /// </summary>
        public static void EndStage()
        {
            _stages[_currentStage].Eventos = _currentStageEvents;
            _stages[_currentStage].EndDate = DateTime.Now;
        }

        /// <summary>
        /// Ends the current Stage and starts another
        /// </summary>
        public static void RetryStage()
        {
            _stages[_currentStage].Eventos = _currentStageEvents;
            _stages[_currentStage].EndDate = DateTime.Now;
            // --
            _currentStageEvents.Clear();
            StartStage(_stages[_currentStage].StageIndex);
        }

        /// <summary>
        /// Start an Metrics event for the specified players
        /// If this StartEvent is called again before the EndEvent
        /// the last StartEvent will be ignored.
        /// You should keep a reference to this event for you End it
        /// when user answers or your game completes the event
        /// </summary>
        /// <param name="players">Players to start this event</param>
        /// <param name="question">Question of this event</param>
        /// <returns>Reference to the Event created</returns>
        public static StageEvent StartEvent(List<long> players, string question)
        {
            var ev = GetRunningEventForPlayers(players);

            var matchEvent = new StageEvent()
            {
                Id = _currentStageEvents.Count,
                StartDate = DateTime.Now,
                Players = players,
                Question = question,
            };
            _currentStageEvents.Add(matchEvent);
            return matchEvent;
        }

        /// <summary>
        /// End the started event with the rightAnswer and the playerAnswer
        /// </summary>
        /// <param name="players">Players where the event was started</param>
        /// <param name="rightAnswer">Correct answer for this current Question/Event</param>
        /// <param name="playerAnswer">Answer that the player gave for this current Question/Event</param>
        public static void EndEvent(List<long> players, string rightAnswer, string playerAnswer)
        {
            StageEvent ev = GetRunningEventForPlayers(players);
            if (ev != null)
                ev.End(rightAnswer, playerAnswer);
        }

        /// <summary>
        /// End the started event with the rightAnswer and the playerAnswer
        /// </summary>
        /// <param name="id">id of the event</param>
        /// <param name="rightAnswer">Correct answer for this current Question/Event</param>
        /// <param name="playerAnswer">Answer that the player gave for this current Question/Event</param>
        public static void EndEvent(int id, string rightAnswer, string playerAnswer)
        {
            StageEvent ev = _currentStageEvents[id];
            if (ev != null)
                ev.End(rightAnswer, playerAnswer);
        }

        /// <summary>
        /// Cancel the started event
        /// </summary>
        /// <param name="players">Players where the event was started</param>
        public static void CancelEvent(List<long> players)
        {
            StageEvent ev = GetRunningEventForPlayers(players);
            if (ev != null)
                _currentStageEvents.Remove(ev);
        }

        /// <summary>
        /// Cancel the started event
        /// </summary>
        /// <param name="id">Id of the event</param>
        public static void CancelEvent(int id)
        {
            StageEvent ev = _currentStageEvents[id];
            if (ev != null)
                _currentStageEvents.Remove(ev);
        }


        /// <summary>
        /// End the started Match with a reason and saves it in SOP
        /// </summary>
        /// <param name="reason">Reason why this match is being finished</param>
        /// <param name="completed">Callback with reasult of the SOP operation</param>
        public static void EndMatch(EndReasons reason, AsyncCallback<bool> completed)
        {
            EndMatch(reason, AvatarAPI.AllPlayersId, completed);
        }

        /// <summary>
        /// End the started Match with a reason and saves it in SOP
        /// </summary>
        /// <param name="reason">Reason why this match is being finished</param>
        /// <param name="players">Players who receive that reason</param>
        /// <param name="completed">Callback with reasult of the SOP operation</param>
        public static void EndMatch(EndReasons reason, List<long> players, AsyncCallback<bool> completed)
        {
            if(_currentMatch != null)
            {
                foreach (var ev in _currentStageEvents.Where(ev => !ev.IsComplete).ToList())
                    CancelEvent(ev.Players);
                // ---
                _currentMatch.EndDate = DateTime.Now;
                _currentMatch.EndReason = (int)reason;
                _currentMatch.Players = players;
                _currentMatch.Stages = _stages;
                _currentMatch.Stages[_currentStage].Eventos = _currentStageEvents;
                _currentMatch.Stages[_currentStage].EndDate = DateTime.Now;

                Metrics.SaveMatch(_currentMatch, (result) => {
                    completed?.Invoke(result);
                });
            }
        }

        /// <summary>
        /// Get the current running/started MatchEvent for the specified players
        /// </summary>
        /// <param name="players">Players where the event is running/started</param>
        /// <returns>Returns the MatchEvent or null is none is found</returns>
        private static StageEvent GetRunningEventForPlayers(List<long> players)
        {
            foreach (var ev in _currentStageEvents.Where(ev => !ev.IsComplete))
            {
                bool foundEvent = true;
                foreach (var player in players)
                {
                    if (!ev.Players.Contains(player))
                    {
                        foundEvent = false;
                        break;
                    }
                }

                if (foundEvent) return ev;
            }
            return null;
        }
    }
}
