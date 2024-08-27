using System;

namespace Playmove.Core.API.Vms
{
    public class TurmaVm
    {
        public long Id { get; set; }
        public string Guid { get; set; }
        public string Nome { get; set; }
        public int Ordem { get; set; }
        public long? SerieId { get; set; }
        public string SerieNome { get; set; }
        public string EmailNotficacao { get; set; }
        public bool Concluida { get; set; }
        public bool Excluido { get; set; }
        public DateTime? DataExclusao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public bool Lixeira { get; set; }
    }
}
