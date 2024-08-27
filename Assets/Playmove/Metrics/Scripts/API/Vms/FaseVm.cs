using Playmove.Metrics.API.Models;
using System;
using System.Collections.Generic;

namespace Playmove.Metrics.API.Vms
{
    public class FaseVm
    {
        public long Id { get; set; }
        public int StageIndex { get; set; }
        public long PartidaID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<EventoVm> Eventos { get; set; }
    }
}
