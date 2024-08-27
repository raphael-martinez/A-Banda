using Newtonsoft.Json;
using System;

namespace Playmove.Core.API.Models
{
    [Serializable]
    public abstract class VmItem<Vm>
    {
        public long Id { get; set; }

        public void SetDataFromVmJson(string json)
        {
            SetDataFromVm(JsonConvert.DeserializeObject<Vm>(json));
        }
        public abstract void SetDataFromVm(Vm vm);

        public abstract Vm GetVm();
        public string GetVmJson()
        {
            return JsonConvert.SerializeObject(GetVm());
        }
    }
}
