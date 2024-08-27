using Newtonsoft.Json;
using Playmove.Avatars.API;
using Playmove.Avatars.API.Models;
using Playmove.Avatars.API.Vms;
using Playmove.Core;
using Playmove.Core.API;
using Playmove.Core.API.Models;
using Playmove.Core.API.Services;
using Playmove.Core.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Playmove.Avatars.API.Services
{
    /// <summary>
    /// Responsible to access APIs for Avatar this should only be used by Playmovers
    /// </summary>
    public class SlotsServices : Service<SlotsConfig, SlotsConfigVm>
    {
        public void GetConfig(AsyncCallback<SlotsConfig> completed)
        {
            WebRequestWrapper.Instance.Get("/Avatar/GetConfig", result =>
            {
                AsyncResult<SlotsConfig> parsedResult = ParseVmJson(result);
                completed?.Invoke(parsedResult);
            });
        }

        public void SetSlots(List<Slot> slots, bool playingWithAI, AsyncCallback<bool> completed)
        {
            List<SlotVm> slotsVM = new List<SlotVm>();
            foreach (var slot in slots)
            {
                SlotVm vm = new SlotVm()
                {
                    Id = slot.Id,
                    Pos = slot.Pos,
                    Guid = slot.Guid,
                    SlotsGroupId = slot.SlotsGroupId,
                    SlotsAlunos = new List<SlotAlunoVm>()
                };

                foreach (var player in slot.Players)
                {
                    var playerVm = player.GetVm();
                    vm.SlotsAlunos.Add(new SlotAlunoVm(slot.Id, playerVm.Id, player.GetVm()));
                }

                slotsVM.Add(vm);
            }

            Dictionary<string, string> json = new Dictionary<string, string>()
            {
                { "slots", JsonConvert.SerializeObject(slotsVM) },
                { "playingWithAI", playingWithAI.ToString() }
            };

            //var jsonSlots = JsonConvert.SerializeObject(slotsVM);

            WebRequestWrapper.Instance.Post("/Avatar/SetSlots", JsonConvert.SerializeObject(json),
               result => completed?.Invoke(SimpleResult(result)));
        }

        public void GetSlots(SlotsConfig config, AsyncCallback<List<Slot>> completed)
        {
            Dictionary<string, string>  _slotConfigParams = new Dictionary<string, string>
            {
                {"OpenedGame", config.OpenedGame.ToString()},
                {"TotalSlots", config.TotalSlots.ToString()},
                {"MinSlots", config.MinSlots.ToString()},
                {"MaxPlayersPerSlot", config.MaxPlayersPerSlot.ToString()},
                {"HasAI", config.HasAI.ToString()},
                {"PlayingWithAI", config.PlayingWithAI.ToString() }
            };

            WebRequestWrapper.Instance.Get("/Avatar/GetSlots", _slotConfigParams, result =>
            {
                AsyncResult<List<Slot>> parsedResult = Service<Slot, SlotVm>.ParseVmsJson(result);
                completed?.Invoke(parsedResult);
            });
        }

        public void GetSlots(AsyncCallback<List<Slot>> completed)
        {
            GetSlots(GameSettings.SlotsConfig, completed);
        }

        public void GetSlot(int slotPosition, AsyncCallback<Slot> completed)
        {
            WebRequestWrapper.Instance.Get("/Avatar/GetSlot", new Dictionary<string, string> { { "slotPosition", slotPosition.ToString() } }, result =>
            {
                AsyncResult<Slot> parsedResult = Service<Slot, SlotVm>.ParseVmJson(result);
                if (result == null)
                    completed?.Invoke(new AsyncResult<Slot>(null, "This slot is not configured open avatar to configure!"));
                else
                    completed?.Invoke(parsedResult);
            });
        }
    }
}
