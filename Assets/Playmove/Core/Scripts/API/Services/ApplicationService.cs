using Playmove.Core.API.Models;
using Playmove.Core.API.Vms;
using System.Collections.Generic;

namespace Playmove.Core.API.Services
{
    /// <summary>
    /// Responsible to access APIs for Application
    /// </summary>
    public class ApplicationService : Service<Application, AplicativoVm>
    {
        /// <summary>
        /// Get application data from the specified GUID
        /// This GUID is passed by Playmove
        /// </summary>
        /// <param name="guid">Application GUID passed by Plamove</param>
        /// <param name="completed">Callback containing data from Application or error</param>
        public void Get(string guid, AsyncCallback<Application> completed)
        {
            WebRequestWrapper.Instance.Get("/Aplicativos/Get", new Dictionary<string, string> { { "productGuid", guid } }, 
                result => completed?.Invoke(ParseVmJson(result)));
        }
    }
}
