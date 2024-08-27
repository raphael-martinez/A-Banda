using Playmove.Core.API.Models;
using Playmove.Metrics.API.Vms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Metrics.API.Models
{
    /// <summary>
    /// Responsible to hold data for each game match
    /// </summary>
    [Serializable]
    public class Match : VmItem<PartidaVm>, IDatabaseItem
    {
        public long SessaoID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EndReason { get; set; }
        public string GameMode { get; set; }
        public string Difficulty { get; set; }
        public List<long> Players { get; set; }
        public int TotalStages { get; set; }
        public List<Stage> Stages { get; set; }

        public Match(){}

        public Match(PartidaVm vm)
        {
            SetDataFromVm(vm);
        }

        public override void SetDataFromVm(PartidaVm vm)
        {
            Id = vm.Id;
            SessaoID = vm.SessaoID;
            StartDate = vm.StartDate;
            EndDate = vm.EndDate;
            GameMode = vm.GameMode;
            Difficulty = vm.Difficulty;
            TotalStages = vm.TotalStages;
            Players = vm.Players;
            Stages = vm.Fases.Select(vmF => new Stage { Id = vmF.Id, EndDate = vmF.EndDate, PartidaID = vmF.PartidaID, StartDate = vmF.StartDate }).ToList();
        }

        public override PartidaVm GetVm()
        {          
            return new PartidaVm()
            {
                Id = Id,
                SessaoID = SessaoID,
                StartDate = StartDate,
                EndDate = EndDate,
                GameMode = GameMode,
                Difficulty = Difficulty,
                TotalStages = TotalStages,
                Players = Players,
                EndReason = EndReason,
                Fases = (Stages != null) ? Stages.Select(
                    fase => fase.GetVm()).ToList() : new List<FaseVm>()
            };
        }
    }
}
