using Playmove.Core.API.Vms;
using System;
using System.Collections.Generic;

namespace Playmove.Avatars.API.Vms
{
    public class ElementoVm
    {
        public long Id { get; set; }
        public string Guid { get; set; }
        public long CategoriaId { get; set; }
        public string ThumbnailGUID { get; set; }
        public string AppliedGUID { get; set; }
    }
}
