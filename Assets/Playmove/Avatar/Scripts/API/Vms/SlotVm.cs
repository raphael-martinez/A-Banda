using System.Collections.Generic;

namespace Playmove.Avatars.API.Vms
{
    public class SlotVm
    {
        public long Id { get; set; }
        public string Guid { get; set; }
        public int Pos { get; set; }
        public long SlotsGroupId { get; set; }
        public List<SlotAlunoVm> SlotsAlunos { get; set; } = new List<SlotAlunoVm>();
    }
}