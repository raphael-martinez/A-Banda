using Playmove.Core.API.Models;
using Playmove.Metrics.API.Vms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playmove.Metrics.API.Models
{
    public class EventPlayers : VmItem<EventosAlunoVm>
    {

        public long EventoId { get; set; }
        public long AlunoId { get; set; }

        public override EventosAlunoVm GetVm()
        {
            return new EventosAlunoVm()
            {
                EventoId = EventoId,
                AlunoId = AlunoId,
            };
        }

        public override void SetDataFromVm(EventosAlunoVm vm)
        {
            AlunoId = vm.AlunoId;
            EventoId = vm.EventoId;
        }
    }
}