using Playmove.Core.API.Vms;
using System;
using System.Collections.Generic;

namespace Playmove.Avatars.API.Vms
{
    public class CategoriaVm
    {
        public long Id { get; set; }
        public long? DefaultElementId { get; set; }
        public ElementoVm DefaultElement { get; set; }
        public virtual ICollection<ElementoVm> Elementos { get; set; }
        public virtual ICollection<CategoriaLocalizacaoVm> Localizacoes { get; set; }
        public int Ordem { get; set; }
        public string Guid { get; set; }
    }

}