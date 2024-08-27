using System;
using System.Collections.Generic;

namespace Playmove.Core.API.Vms
{
    public class GrupoArquivosVm
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Localizacao { get; set; }
        public string Guid { get; set; }
        public bool ConfiguracaoDeFabrica { get; set; }
        public bool Visivel { get; set; }
        public long? GrupoImgId { get; set; }
        public long? GrupoAudioId { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
        public bool Lixeira { get; set; }
        public virtual ICollection<ArquivoAplicativoVm> ArquivosAplicativo { get; set; }
    }
}
