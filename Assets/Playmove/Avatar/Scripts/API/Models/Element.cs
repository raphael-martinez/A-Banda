using Playmove.Avatars.API.Vms;
using Playmove.Core;
using Playmove.Core.API.Models;
using Playmove.Core.Storages;
using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Playmove.Avatars.API.Models
{
    [Serializable]
    public class Element : VmItem<ElementoVm>, IDatabaseItem
    {
        public string GUID { get; set; }
        public long CategoryId { get; set; }
        public string ThumbnailGUID { get; set; }
        public string AppliedGUID { get; set; }

        private int _categoryOrder = -1;
        public int CategoryOrder
        {
            get
            {
                if(_categoryOrder == -1)
                {
                    var _order = AvatarAPI.Categories.Categories.Find(cat => cat.Id == CategoryId);
                    _categoryOrder = _order != null ? _order.Order : 0;
                }

                return _categoryOrder;
            }
        }

        public Sprite Icon;
        public bool Selected;
        public string type;

        public string ThumbnailPath
        {
            get { return $"{AvatarAPI.RootAvatarPath}/Files/Avatar/{ThumbnailGUID}.png"; }
        }

        public string FullPath
        {
            get
            {
                string fullPath = $"{AvatarAPI.RootAvatarPath}/Files/Avatar/{AppliedGUID}.png";
                if (File.Exists(fullPath))
                    return fullPath;
                else
                    return ThumbnailPath;
            }
        }

        [NonSerialized]
        private Sprite _fullSpriteCache;
        [NonSerialized]
        private Sprite _thumbnailSpriteCache;

        public Element() { }
        public Element(string guid)
        {
            GUID = guid;
        }
        public Element(ElementoVm vm)
        {
            SetDataFromVm(vm);
        }

        public void GetFullSprite(AsyncCallback<Sprite> completed)
        {
            GetFullSprite(0, 0, completed);
        }
        public void GetFullSprite(int width, int height, AsyncCallback<Sprite> completed)
        {
            LoadSprite(_fullSpriteCache, FullPath, width, height, completed);
        }

        public void GetThumbnailSprite(AsyncCallback<Sprite> completed)
        {
            GetThumbnailSprite(0, 0, completed);
        }
        public void GetThumbnailSprite(int width, int height, AsyncCallback<Sprite> completed)
        {
            LoadSprite(_thumbnailSpriteCache, ThumbnailPath, width, height, completed);
        }

        public void ReleaseResource()
        {
            if (_fullSpriteCache != null)
                Object.DestroyImmediate(_fullSpriteCache.texture, true);
            if (_thumbnailSpriteCache != null)
                Object.DestroyImmediate(_thumbnailSpriteCache.texture, true);
            _fullSpriteCache = _thumbnailSpriteCache = null;
        }

        public override string ToString()
        {
            return GUID;
        }

        private void LoadSprite(Sprite spriteCache, string spritePath, int width, int height, AsyncCallback<Sprite> completed)
        {
            if (spriteCache != null)
            {
                if (spriteCache.texture.width == width && spriteCache.texture.height == height)
                {
                    completed?.Invoke(new AsyncResult<Sprite>(spriteCache, string.Empty));
                    return;
                }

                width = width > 0 ? width : spriteCache.texture.width;
                height = height > 0 ? height : spriteCache.texture.height;
                if (width > 0 || height > 0)
                    spriteCache = spriteCache.Scaled(width, height, true);

                completed?.Invoke(new AsyncResult<Sprite>(spriteCache, string.Empty));
                return;
            }
            Storage.LoadSprite(spritePath, width, height, true, (result) =>
            {
                if (!result.HasError) spriteCache = result.Data;
                completed?.Invoke(result);
            });
        }

        public override void SetDataFromVm(ElementoVm vm)
        {
            Id = vm.Id;
            GUID = vm.Guid;
            CategoryId = vm.CategoriaId;
            AppliedGUID = vm.AppliedGUID;
            ThumbnailGUID = vm.ThumbnailGUID;
        }

        public override ElementoVm GetVm()
        {
            return new ElementoVm()
            {
                Id = Id,
                Guid = GUID,
                CategoriaId = CategoryId,
                AppliedGUID = AppliedGUID,
                ThumbnailGUID = ThumbnailGUID,
            };
        }
    }
}
