using Playmove.Core.API.Vms;
using System;
using System.Collections.Generic;

namespace Playmove.Avatars.API.Vms
{
    public class CategoriaLocalizacaoVm
    {
        public long Id { get; set; }
        public long CategoriaId { get; set; }
        public string Localizacao { get; set; }
        public string Descricao { get; set; }
    }

}