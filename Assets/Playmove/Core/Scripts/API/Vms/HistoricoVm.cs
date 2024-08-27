using System;

namespace Playmove.Core.API.Vms
{
    public class HistoricoVm
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public long Revisao { get; set; }
        public string Descricao { get; set; }
        public bool AvancoAutomatico { get; set; }
        public int Meses { get; set; }
        public bool Excluido { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataExclusao { get; set; }
    }
}
