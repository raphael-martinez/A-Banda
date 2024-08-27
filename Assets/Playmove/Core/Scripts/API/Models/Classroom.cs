using Playmove.Core.API.Vms;
using System;

namespace Playmove.Core.API.Models
{
    [Serializable]
    public class Classroom : VmItem<TurmaVm>, IDatabaseItem, ITrash
    {
        public string GUID { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public long? GradeId { get; set; }
        public string GradeName { get; set; }
        public string MailToNotify { get; set; }
        public bool Finished { get; set; }
        public bool Deleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(GradeName))
                    return Name;
                return $"{Name} ({GradeName})";
            }
        }

        public Classroom() { }
        public Classroom(long id)
        {
            Id = id;
        }
        public Classroom(TurmaVm vm)
        {
            SetDataFromVm(vm);
        }

        public override TurmaVm GetVm()
        {
            return new TurmaVm()
            {
                Id = Id,
                Guid = GUID,
                Nome = Name,
                Ordem = Order,
                SerieId = GradeId,
                SerieNome = GradeName,
                EmailNotficacao = MailToNotify,
                Concluida = Finished,
                Excluido = Deleted,
                Lixeira = Deleted,
                DataExclusao = DeletedAt
            };
        }

        public override void SetDataFromVm(TurmaVm vm)
        {
            Id = vm.Id;
            GUID = vm.Guid;
            Name = vm.Nome;
            Order = vm.Ordem;
            GradeId = vm.SerieId;
            GradeName = vm.SerieNome;
            MailToNotify = vm.EmailNotficacao;
            Finished = vm.Concluida;
            Deleted = vm.Lixeira || vm.Excluido;
            DeletedAt = vm.DataExclusao.HasValue ? vm.DataExclusao : vm.DataAtualizacao;
        }
    }
}
