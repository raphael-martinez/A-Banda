using Playmove.Avatars.API.Models;
using Playmove.Avatars.API.Vms;
using Playmove.Core;
using Playmove.Core.API;
using Playmove.Core.API.Services;
using System.Collections.Generic;
using System.Linq;

namespace Playmove.Avatars.API.Services
{
    public class CategoryService : Service<Category, CategoriaVm>
    {
        public List<Category> Categories { get; private set; } = new List<Category>();
        public List<Element> DefaultElements { get; private set; } = new List<Element>();

        public void LoadElements(AsyncCallback<List<Category>> completed)
        {            
            if (Categories.Count > 0)
            {
                completed?.Invoke(new AsyncResult<List<Category>>(Categories, string.Empty));
                return;
            }

            WebRequestWrapper.Instance.Get("/Avatar/GetAllElements", result =>
            {
                AsyncResult<List<Category>> parsedResult = ParseVmsJson(result);
                if (parsedResult.Success)
                {
                    parsedResult.Data = parsedResult.Data.ToList();
                    Categories = parsedResult.Data.OrderBy(cat => cat.Order).ToList();

                    foreach (var cat in Categories)
                    {
                        cat.Elements = cat.Elements.OrderBy(ele => ele.Id).ToList();
                        DefaultElements.Add(
                            new Element(cat.DefaultElement)
                        );
                    }
                }
                completed?.Invoke(parsedResult);
            });
        }
    }
}