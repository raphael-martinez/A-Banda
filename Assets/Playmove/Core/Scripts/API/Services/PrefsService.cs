using Playmove.Core.API.Models;
using Playmove.Core.API.Vms;
using System.Collections.Generic;

namespace Playmove.Core.API.Services
{
    public class PrefsService : Service<Pref, ConfiguracaoVm>
    {
        public void Get(string setupName, AsyncCallback<Pref> completed)
        {
            WebRequestWrapper.Instance.Get("/Configuracoes/Get", new Dictionary<string, string> { { "configuracaoNome", setupName } },
                (result) => completed?.Invoke(ParseVmJson(result)));
        }

        public void Set(string setupName, ValorTipo type, string value, AsyncCallback<bool> completed)
        {
            Set(new Pref()
            {
                Name = setupName,
                Type = type,
                Value = value
            }, completed);
        }

        public void Set(Pref setup, AsyncCallback<bool> completed)
        {
            WebRequestWrapper.Instance.Post("/Configuracoes/SetConfiguration", setup.GetVmJson(),
                (resut) => completed?.Invoke(SimpleResult(resut)));
        }
    }
}
