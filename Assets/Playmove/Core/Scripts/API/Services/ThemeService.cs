using Playmove.Core.API.Models;
using Playmove.Core.API.Vms;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Core.API.Services
{
    /// <summary>
    /// Responsible to access APIs for Themes
    /// </summary>
    public class ThemeService : Service<Theme, GrupoArquivosVm>
    {
        /// <summary>
        /// Returns all themes for this Application based on the current Language
        /// </summary>
        /// <param name="completed">Callback containing all themes and error message if any</param>
        public void GetAll(AsyncCallback<List<Theme>> completed)
        {
            GetAll(GameSettings.Language, completed);
        }
        /// <summary>
        /// Returns all themes for this Application
        /// </summary>
        /// <param name="language">Language of the themes</param>
        /// <param name="completed">Callback containing all themes and error message if any</param>
        public void GetAll(string language, AsyncCallback<List<Theme>> completed)
        {
            if (GameSettings.ApplicationId == -1)
            {
                Debug.LogError("Contact Playmove Support for more informations about this!");
                return;
            }

            var parameters = new Dictionary<string, string>
            {
                { "aplicativoId", GameSettings.ApplicationId.ToString() }
            };
            if (!string.IsNullOrEmpty(language))
                parameters.Add("localizacao", language);
            
            WebRequestWrapper.Instance.Get("/GrupoArquivos/GetAllByAppAndLocalization", parameters, 
                result =>
                {
                    AsyncResult<List<Theme>> parsedResult = ParseVmsJson(result);
                    if (!parsedResult.HasError)
                        parsedResult.Data = parsedResult.Data.Where(theme => !theme.Deleted).ToList();
                    completed?.Invoke(parsedResult);
                });
        }

        /// <summary>
        /// Get a specific theme by it's GUID
        /// </summary>
        /// <param name="guid">Theme GUID</param>
        /// <param name="completed">Callback containing the requested Theme or error</param>
        public void Get(string guid, AsyncCallback<Theme> completed)
        {
            WebRequestWrapper.Instance.Get("/GrupoArquivos/Get",
                new Dictionary<string, string> { { "guid", guid } },
                result => completed?.Invoke(ParseVmJson(result)));
        }
        /// <summary>
        /// Get a specific theme by it's Id
        /// </summary>
        /// <param name="id">Theme Id</param>
        /// <param name="completed">Callback containing the requested Theme or error</param>
        public void Get(long id, AsyncCallback<Theme> completed)
        {
            WebRequestWrapper.Instance.Get("/GrupoArquivos/Get", new Dictionary<string, string> { { "id", id.ToString() } },
                result => completed?.Invoke(ParseVmJson(result)));
        }

        /// <summary>
        /// Register the factory themes in playtable
        /// </summary>
        /// <param name="theme">Theme data to be registered</param>
        /// <param name="completed">Callback containing the representation of the Theme or error</param>
        public void RegisterFactoryTheme(Theme theme, AsyncCallback<Theme> completed)
        {
            WebRequestWrapper.Instance.Post("/GrupoArquivos/RegisterFactoryGroup", theme.GetVmJson(),
                result => completed?.Invoke(ParseVmJson(result)));
        }
    }
}
