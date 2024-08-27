using Playmove.Core.API.Models;
using Playmove.Metrics.API.Vms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Metrics.API.Models
{
    [Serializable]
    public class Stage : VmItem<FaseVm>, IDatabaseItem
    {
        public long PartidaID { get; set; }
        public int StageIndex { get; set;  }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<StageEvent> Eventos { get; set; }

        public override FaseVm GetVm()
        {
            return new FaseVm()
            {
                Id = Id,
                PartidaID = PartidaID,
                StartDate = StartDate,
                EndDate = EndDate,
                StageIndex = StageIndex,
                Eventos = (Eventos != null) ? Eventos.Select(ev => ev.GetVm()).ToList() : new List<EventoVm>()
            };
        }

        public override void SetDataFromVm(FaseVm vm)
        {
            Id = vm.Id;
            PartidaID = vm.PartidaID;
            StartDate = vm.StartDate;
            EndDate = vm.EndDate;
            StageIndex = vm.StageIndex;
        }
    }
}
