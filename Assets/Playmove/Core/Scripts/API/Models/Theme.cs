using Playmove.Core.API.Vms;
using System;
using System.Collections.Generic;

namespace Playmove.Core.API.Models
{
    [Serializable]
    public class Theme : VmItem<GrupoArquivosVm>, IDatabaseItem, ITrash
    {
        public string GUID { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Language { get; set; }
        public bool IsFactory { get; set; }
        public bool IsVisible { get; set; }
        public long? IconId { get; set; }
        public long? AudioId { get; set; }
        public List<PlaytableFile> Files { get; set; } = new List<PlaytableFile>();
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Theme()
        {
            Files = new List<PlaytableFile>();
        }
        public Theme(GrupoArquivosVm vm)
            : this()
        {
            SetDataFromVm(vm);
        }
        public Theme(string vmJson)
        {
            SetDataFromVmJson(vmJson);
        }

        public void GetIcon(AsyncCallback<PlaytableFile> callback)
        {

        }
        public void GetAudio(AsyncCallback<PlaytableFile> callback)
        {

        }

        public override GrupoArquivosVm GetVm()
        {
            List<ArquivoAplicativoVm> filesVm = new List<ArquivoAplicativoVm>();
            foreach (var file in Files)
                filesVm.Add(file.GetVm());

            return new GrupoArquivosVm()
            {
                Id = Id,
                Guid = GUID,
                Nome = Name,
                Localizacao = Language,
                ConfiguracaoDeFabrica = IsFactory,
                Visivel = IsVisible,
                ArquivosAplicativo = filesVm,
                DataCriacao = CreatedAt,
                DataAtualizacao = UpdatedAt,
                GrupoAudioId = AudioId,
                GrupoImgId = IconId,
                Lixeira = Deleted
            };
        }

        public override void SetDataFromVm(GrupoArquivosVm vm)
        {
            Id = vm.Id;
            GUID = vm.Guid;
            Name = vm.Nome;
            Language = vm.Localizacao;
            IsFactory = vm.ConfiguracaoDeFabrica;
            IsVisible = vm.Visivel;
            CreatedAt = vm.DataCriacao;
            UpdatedAt = vm.DataAtualizacao;
            IconId = vm.GrupoImgId;
            AudioId = vm.GrupoAudioId;
            Deleted = vm.Lixeira;
            DeletedAt = vm.DataAtualizacao;

            if (vm.ArquivosAplicativo != null)
            {
                foreach (var fileVm in vm.ArquivosAplicativo)
                    Files.Add(new PlaytableFile(fileVm));
            }
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Factory: {IsFactory}, FilesAmount: {Files.Count}";
        }
    }
}
