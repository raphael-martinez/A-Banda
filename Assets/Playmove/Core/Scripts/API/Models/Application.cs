using Playmove.Core.API.Vms;
using System;

namespace Playmove.Core.API.Models
{
    [Serializable]
    public class Application : VmItem<AplicativoVm>, IDatabaseItem
    {
        public string GUID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public Application() { }
        public Application(AplicativoVm vm)
        {
            SetDataFromVm(vm);
        }
        public Application(string vmJson)
        {
            SetDataFromVmJson(vmJson);
        }

        public override AplicativoVm GetVm()
        {
            return new AplicativoVm()
            {
                Id = Id,
                ProdutoGuid = GUID,
                NomeApp = Name
            };
        }
        public override void SetDataFromVm(AplicativoVm vm)
        {
            Id = vm.Id;
            GUID = vm.ProdutoGuid;
            Name = vm.NomeApp;
        }
    }
}
