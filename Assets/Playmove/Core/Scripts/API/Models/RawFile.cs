using Playmove.Core.API.Vms;
using System;

namespace Playmove.Core.API.Models
{
    [Serializable]
    public class RawFile : VmItem<ArquivoVm>, IDatabaseItem
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public float Size { get; set; }
        public string Extension { get; set; }
        private string _fullPath = string.Empty;
        public string FullPath
        {
            get { return _fullPath; }
            set { _fullPath = value.Replace(@"\", "/"); }
        }

        public RawFile() { }
        public RawFile(ArquivoVm vm)
        {
            SetDataFromVm(vm);
        }
        public RawFile(string vmJson)
        {
            SetDataFromVmJson(vmJson);
        }

        public override ArquivoVm GetVm()
        {
            return new ArquivoVm()
            {
                Id = Id,
                Nome = Name,
                Localizacao = Language,
                Extensao = Extension,
                Tamanho = Size,
                CaminhoArquivo = FullPath
            };
        }

        public override void SetDataFromVm(ArquivoVm vm)
        {
            Id = vm.Id;
            Name = vm.Nome;
            Language = vm.Localizacao;
            Extension = vm.Extensao;
            Size = vm.Tamanho;
            FullPath = vm.CaminhoArquivo;
        }
    }
}
