using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playmove.Avatars.API.Vms;
using Playmove.Core;
using Playmove.Core.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playmove.Avatars.API.Models
{
    public interface ICategorySavable
    {
        string GUID { get; }
        Element Element { get; set; }
    }

    public interface ICategory : ICategorySavable
    {
        int Order { get; }
    }

    [Serializable]
    public class Category : VmItem<CategoriaVm>, IDatabaseItem, ICategory
    {
        public string GUID { get; set; }
        public int Order { get; set; }
        public long? DefaultElementId { get; set; }
        public ElementoVm DefaultElement { get; set; }
        public List<CategoriaLocalizacaoVm> Localizations { get; set; }
        public List<ElementoVm> Elements { get; set; }

        public List<Element> GetElements()
        {
            List<Element> _elements = new List<Element>();
            foreach (var elem in Elements)
            {
                _elements.Add(new Element()
                {
                    GUID = elem.Guid,
                    AppliedGUID = elem.AppliedGUID,
                    ThumbnailGUID = elem.ThumbnailGUID,
                    Id = elem.Id,
                    CategoryId = elem.CategoriaId
                });
            }
            return _elements;
        }

        public Element Element
        {
            get
            {
                if (Elements.Count == 0)
                    Elements.Add(new ElementoVm());
                return GetElement(Elements[0]);
            }
            set
            {
                if (Elements.Count == 0)
                    Elements.Add(new ElementoVm());
                Elements[0] = value.GetVm();
            }
        }

        public Element GetElement(ElementoVm elem)
        {
            Element _element = new Element()
            {
                GUID = elem.Guid,
                AppliedGUID = elem.AppliedGUID,
                ThumbnailGUID = elem.ThumbnailGUID,
                Id = elem.Id,
                CategoryId = elem.CategoriaId
            };
            return _element;
        }


        public string Title
        {
            get
            {
                CategoriaLocalizacaoVm localized = Localizations.Find(loc => loc.Localizacao.ToLower() == GameSettings.Language.ToLower());
                return localized.Descricao;
            }
            set { }
        }

        public override CategoriaVm GetVm()
        {
            return new CategoriaVm()
            {
                Id = Id,
                DefaultElement = DefaultElement,
                DefaultElementId = DefaultElementId,
                Elementos = Elements,
                Ordem = Order,
                Guid = GUID
            };
        }

        public override void SetDataFromVm(CategoriaVm vm)
        {
            Id = vm.Id;
            Order = vm.Ordem;
            Localizations = vm.Localizacoes.ToList();
            GUID = vm.Guid;
            Elements = vm.Elementos.ToList();
            DefaultElement = vm.DefaultElement;
            DefaultElementId = vm.DefaultElementId;
        }

        public override string ToString()
        {
            //return base.ToString();
            return $"{GUID} => Title: {Title}; Order: {Order}; Elements: {string.Join(", ", Elements.Select(elem => elem.Guid))}";
        }
    }
}
