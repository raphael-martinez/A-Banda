using Playmove.Core.API.Vms;

namespace Playmove.Avatars.API.Vms
{
    public class AlunoTurmaVm
    {
        public long AlunoId { get; set; }

        public long TurmaId { get; set; }

        public TurmaVm Turma { get; set; }

        public AlunoTurmaVm() { }
        public AlunoTurmaVm(long alunoId, long turmaId, TurmaVm turma)
        {
            AlunoId = alunoId;
            TurmaId = turmaId;
            Turma = turma;
        }
    }
}
