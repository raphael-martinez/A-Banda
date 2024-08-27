using Newtonsoft.Json;
using Playmove.Avatars.API.Vms;
using Playmove.Core.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playmove.Avatars.API.Models
{
    [Serializable]
    public class Slot : VmItem<SlotVm>, IDatabaseItem
    {
        public string Guid { get; set; }
        public long SlotsGroupId { get; set; }
        public int Pos { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();

        public Slot(){}

        public Slot(SlotVm vm)
        {
            SetDataFromVm(vm);
        }

        public override SlotVm GetVm()
        {
            return new SlotVm()
            {
                Id = Id,
                Guid = Guid,
                Pos = Pos,
                SlotsGroupId = SlotsGroupId,
                SlotsAlunos = Players.Select(aluno => new SlotAlunoVm(Id, aluno.Id, aluno.GetVm())).ToList(),
            };
        }

        public override void SetDataFromVm(SlotVm vm)
        {
            Id = vm.Id;
            Guid = vm.Guid;
            Pos = vm.Pos;
            SlotsGroupId = vm.SlotsGroupId;
            Players = vm.SlotsAlunos.Select(slotAlu => new Player(slotAlu.Aluno)).ToList();
        }

        public List<long> PlayersId { get { return Players.Select(player => player.Id).ToList(); } }

        public List<string> PlayersGUID { get { return Players.Select(player => player.GUID).ToList(); } }
    }
}
