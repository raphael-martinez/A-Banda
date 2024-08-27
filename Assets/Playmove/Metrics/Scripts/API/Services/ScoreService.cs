using Playmove.Avatars.API.Models;
using Playmove.Core;
using Playmove.Core.API;
using Playmove.Core.API.Services;
using Playmove.Metrics.API.Models;
using Playmove.Metrics.API.Vms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Metrics.API.Services
{
    /// <summary>
    /// Responsible to handle APIs for Score and Progress.
    /// This class can Save score, Update and Delete
    /// user also can save the game Progress like levels
    /// unlocked for each player
    /// </summary>
    public class ScoreService
    {
        public void GetRanking(AsyncCallback<Ranking> completed)
        {
            WebRequestWrapper.Instance.Get("/Metricas/GetRanking", new Dictionary<string, string> { { "gameGUID", GameSettings.GUID } }, result =>
            {
                AsyncResult<Ranking> parsedResult = Service<Ranking, RankingVm>.ParseVmJson(result);
                
                completed?.Invoke(parsedResult);
            });
        }
        
        public void SaveRanking(string gameGUID, Ranking ranking, AsyncCallback<bool> completed)
        {
            ranking.GameGUID = gameGUID;
            SaveRanking(ranking, completed);
        }

        public void SaveRanking(Ranking ranking, AsyncCallback<bool> completed)
        {
            ranking.ScoreInfo = ranking.ScoreInfo.Take(50).ToDictionary(item => item.Key, item => item.Value);
            WebRequestWrapper.Instance.Post("/Metricas/SaveRanking", ranking.GetVmJson(), result =>
            {
                if (result.HasError)
                {
                    Debug.LogError("Erro ao salvar Ranking:" + result.Error);
                    return;
                }
                completed?.Invoke(new AsyncResult<bool>(true, string.Empty));
            });
        }

        public void DeleteRanking(Ranking ranking, AsyncCallback<bool> completed)
        {
            WebRequestWrapper.Instance.Post("/Metricas/DeleteRanking", ranking.Id.ToString(), result =>
            {
                if (result.HasError)
                {
                    Debug.LogError("Erro ao deletar Ranking:" + result.Error);
                    return;
                }
                completed?.Invoke(new AsyncResult<bool>(true, string.Empty));
            });
        }
    }
}
