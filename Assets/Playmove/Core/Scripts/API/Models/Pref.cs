using Playmove.Core.API.Vms;
using System;

namespace Playmove.Core.API.Models
{
    [Serializable]
    public class Pref : VmItem<ConfiguracaoVm>, IDatabaseItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public ValorTipo Type { get; set; }

        public T GetValue<T>()
        {
            object valueObj = default;
            switch (Type)
            {
                case ValorTipo.Long:
                    long valueLong = 0;
                    long.TryParse(Value, out valueLong);
                    valueObj = valueLong;
                    break;
                case ValorTipo.Bool:
                    bool valueBool = false;
                    bool.TryParse(Value, out valueBool);
                    valueObj = valueBool;
                    break;
                case ValorTipo.DataHora:
                    DateTime valueDate = default;
                    DateTime.TryParse(Value, out valueDate);
                    valueObj = valueDate;
                    break;
                case ValorTipo.Bytes:
                case ValorTipo.None:
                default:
                    valueObj = Value;
                    break;
            }
            return (T)valueObj;
        }

        public override ConfiguracaoVm GetVm()
        {
            return new ConfiguracaoVm()
            {
                Id = Id,
                Nome = Name,
                Valor = Value,
                Tipo = Type
            };
        }

        public override void SetDataFromVm(ConfiguracaoVm vm)
        {
            Id = vm.Id;
            Name = vm.Nome;
            Value = vm.Valor;
            Type = vm.Tipo;
        }
    }
}
