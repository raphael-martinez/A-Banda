using System;
using System.Collections.Generic;

namespace Playmove.Core.API.Vms
{
    public class ArquivoAplicativoVm
    {
        public long Id { get; set; }
        public ArquivoVm Arquivo { get; set; }
        public long? GrupoAplicativoId { get; set; }
        public List<ArquivoAluno> ArquivoAlunos { get; set; }
        public string Guid { get; set; }
        public long AplicativoId { get; set; }
        public string Configuracao { get; set; }
        public string Agrupamento { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public bool Lixeira { get; set; }
    }
}
