using System;
using System.Collections.Generic;

namespace Playmove.Metrics.API.Vms
{
    public class SessaoVm
    {
        public long Id { get; set; }
        public string ProdutoGuid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual List<PartidaVm> Partidas { get; set; }
    }
}