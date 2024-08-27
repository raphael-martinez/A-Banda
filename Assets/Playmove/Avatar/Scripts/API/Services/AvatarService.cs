using Playmove.Avatars.API.Models;
using Playmove.Avatars.API.Vms;
using Playmove.Core;
using Playmove.Core.API;
using Playmove.Core.API.Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Playmove.Avatars.API.Services
{
    /// <summary>
    /// Responsible to make requests to SOP about Avatar
    /// </summary>
    public class AvatarService : Service<Player, AlunoVm>
    {
        public void GetPlayersThumbnail(long classroomId, AsyncCallback<List<Player>> completed)
        {
            WebRequestWrapper.Instance.Get("/Alunos/GetAllByTurmaAndNoClass", new Dictionary<string, string> { { "turmaId", classroomId.ToString() }, { "semTurma", "false" } }, result =>
            {
                AsyncResult<List<Player>> parsedResult = ParseVmsJson(result);
                if (parsedResult.Success)
                    parsedResult.Data = parsedResult.Data.ToList();
                // ---
                completed?.Invoke(parsedResult);
            });
        }

        public void GetPlayer(long? id, AsyncCallback<Player> completed)
        {
            WebRequestWrapper.Instance.Get("/Alunos/Get", new Dictionary<string, string> { { "id", id.ToString() } },
                result => completed?.Invoke(Service<Player, AlunoVm>.ParseVmJson(result)));
        }

        public void GetPlayer(string GUID, AsyncCallback<Player> completed)
        {
            WebRequestWrapper.Instance.Get("/Alunos/GetByGUID", new Dictionary<string, string> { { "guid", GUID } },
                result =>
                {
                    AsyncResult<Player> parsedResult = ParseVmJson(result);
                    completed?.Invoke(parsedResult);
                });
        }

        public void UnlockElement(string elementGUID, Slot slot, AsyncCallback<bool> completed)
        {
            UnlockElement(elementGUID, slot.Players, completed);
        }
        public void UnlockElement(string elementGUID, List<Player> players, AsyncCallback<bool> completed)
        {
            completed?.Invoke(new AsyncResult<bool>(true, string.Empty));
        }

        public void OpenLeague(AsyncCallback completed)
        {
            WebRequestWrapper.Instance.Get("/Avatar/OpenLeague", new Dictionary<string, string> { { "config", GameSettings.SlotsConfig.ToString() } }, result =>
            {
                if (result.HasError)
                {
                    Debug.LogError("Erro ao abrir o aplicativo Liga dos Heróis: " + result.Error);
                    return;
                }
                Playtable.Instance.StartCoroutine(AvatarAPI.CheckLeagueAppClosed(completed));
            });
        }

        public void Open(SlotsConfig config, AsyncCallback completed)
        {
            Dictionary<string, string> _slotConfigParams = new Dictionary<string, string>
            {
                {"OpenedGame", config.OpenedGame.ToString()},
                {"TotalSlots", config.TotalSlots.ToString()},
                {"MinSlots", config.MinSlots.ToString()},
                {"MaxPlayersPerSlot", config.MaxPlayersPerSlot.ToString()},
                {"HasAI", config.HasAI.ToString() },
                {"PlayingWithAI", config.PlayingWithAI.ToString() }
            };
            // ---
            WebRequestWrapper.Instance.Get("/Avatar/OpenAvatar", _slotConfigParams, result =>
            {
                if (result.HasError)
                {
                    Debug.LogError("Erro ao abrir o aplicativo Avatar: "+ result.Error);
                    return;
                }
                Playtable.Instance.StartCoroutine(AvatarAPI.CheckAvatarAppClosed(completed));
            });
        }

        public void Open(AsyncCallback completed)
        {
            Open(GameSettings.SlotsConfig, completed);
        }

        public void Close(AsyncCallback completed)
        {
            WebRequestWrapper.Instance.Get("/Avatar/CloseAvatar", result =>
            {
                if (result.HasError)
                {
                    Debug.LogError("Erro ao abrir o aplicativo Avatar");
                    return;
                }
                completed?.Invoke();
            });
        }
    }
}
