using Playmove.Metrics.API.Models;
using System;
using System.Collections.Generic;

namespace Playmove.Metrics.API.Vms
{
    public class PartidaVm
    {
        public long Id { get; set; }
        public long SessaoID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GameMode { get; set; }
        public string Difficulty { get; set; }
        public int TotalStages { get; set; }
        public List<FaseVm> Fases { get; set; }
        public List<long> Players { get; set; }
        public int EndReason { get; set; }
    }
}
