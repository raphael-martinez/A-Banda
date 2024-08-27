using Playmove.Core.API.Vms;
using System;
using System.Collections.Generic;

namespace Playmove.Avatars.API.Vms
{
    public class AlunoVm
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public bool Lixeira { get; set; }
        public string AlunoGuid { get; set; }
        public long? TurmaId { get; set; }
        public string Email { get; set; }
        public bool Excluido { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public DateTime? LastTimePlayed { get; set; }
        public DateTime? DataExclusao { get; set; }
        public string ThumbnailPath { get; set; }
        public ICollection<ArquivoVm> Arquivos { get; set; }
        public ICollection<AlunoTurmaVm> AlunoTurmas { get; set; }
        public ICollection<ElementosAlunoVm> ElementosAluno { get; set; }
    }
}
