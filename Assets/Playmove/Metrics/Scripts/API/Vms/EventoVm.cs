using Playmove.Metrics.API.Models;
using System;
using System.Collections.Generic;

namespace Playmove.Metrics.API.Vms
{
    public class EventoVm
    {
        public long FaseID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Question { get; set; }
        public string RightAnswer { get; set; }
        public string PlayerAnswer { get; set; }
        public List<long> Players { get; set; }
    }
}
