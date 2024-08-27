using Playmove.Core.API.Models;
using Playmove.Metrics.API.Vms;
using System;
using System.Collections.Generic;

namespace Playmove.Metrics.API.Models
{
    /// <summary>
    /// Responsible to save all Session properties for a Player
    /// </summary>
    [Serializable]
    public class Session : VmItem<SessaoVm>, IDatabaseItem
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public override SessaoVm GetVm()
        {
            return new SessaoVm()
            {
                Id = Id,
                StartDate = StartDate,
                EndDate = EndDate                
            };
        }

        public override void SetDataFromVm(SessaoVm vm)
        {
            Id = vm.Id;
            StartDate = vm.StartDate;
            EndDate = vm.EndDate;
        }
    }
}
