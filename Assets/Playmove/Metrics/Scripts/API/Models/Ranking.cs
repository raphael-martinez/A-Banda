using Newtonsoft.Json;
using Playmove.Core;
using Playmove.Core.API.Models;
using Playmove.Metrics.API.Vms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Metrics.API.Models
{
    /// <summary>
    /// Responsible to hold data for the Ranking of this game
    /// </summary>
    [Serializable]
    public partial class Ranking : VmItem<RankingVm>, IDatabaseItem
    {
        private string _gameGUID = string.Empty;
        public string Data { get; set; }
        public string GameGUID
        {
            get { return string.IsNullOrEmpty(_gameGUID) ? GameSettings.GUID : _gameGUID; }
            set { _gameGUID = value; }
        }
        public Dictionary<string, Score> ScoreInfo { get; set; }

        public Ranking()
        {
            GameGUID = GameSettings.GUID;
            ScoreInfo = new Dictionary<string, Score>();
        }
        public Ranking(string gameGUID, Dictionary<string, Score> scoreInfo)
        {
            GameGUID = gameGUID;
            ScoreInfo = ScoreInfo;
        }

        public Score GetScore(string playerGUID)
        {
            if (!ScoreInfo.ContainsKey(playerGUID))
                ScoreInfo.Add(playerGUID, new Score());
            return ScoreInfo[playerGUID];
        }
        /// <summary>
        /// This will only set the score if it's greater than the current Score
        /// </summary>
        /// <param name="playerGUID">Player GUID to update score</param>
        /// <param name="score">Score to be set in case it's greater than the current Score</param>
        public void SetScore(string playerGUID, float score)
        {
            if (!ScoreInfo.ContainsKey(playerGUID))
                ScoreInfo.Add(playerGUID, new Score());
            if (score > ScoreInfo[playerGUID].GlobalScore)
                ScoreInfo[playerGUID].GlobalScore = score;
        }

        public override void SetDataFromVm(RankingVm vm)
        {
            Id = vm.Id;
            GameGUID = vm.GameGUID;
            ScoreInfo = JsonConvert.DeserializeObject<Dictionary<string, Score>>(vm.Data);
        }

        public override RankingVm GetVm()
        {
            return new RankingVm()
            {
                Id = Id,
                GameGUID = GameGUID,
                Data = JsonConvert.SerializeObject(ScoreInfo)
            };
        }
    }
}
