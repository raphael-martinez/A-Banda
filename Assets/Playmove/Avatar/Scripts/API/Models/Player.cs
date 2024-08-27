using Playmove.Avatars.API.Vms;
using Playmove.Avatars.API.Interfaces;
using Playmove.Core;
using Playmove.Core.API.Models;
using Playmove.Core.Storages;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace Playmove.Avatars.API.Models
{
    [Serializable]
    public class Player : VmItem<AlunoVm>, IPlayerThumbnail, IDatabaseItem, ITrash
    {
        #region Properties
        public string GUID { get; set; }
        public string Name { get; set; }
        public List<Classroom> Classrooms { get; set; } = new List<Classroom>();
        public bool IsVillain { get; set; }
        public bool Deleted { get; set; }
        public DateTime? LastTimePlayed { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string ThumbnailPath { get; set; }
        public Avatar Avatar { get; set; }
        public List<Element> Elements { get; set; } = new List<Element>();

        [NonSerialized]
        private Sprite _thumbnailSpriteCache;
        [NonSerialized]
        private Sprite _avatarSpriteCache;
        #endregion

        #region Constructor
        public Player() { }
        public Player(AlunoVm vm)
        {
            SetDataFromVm(vm);
        }
        #endregion

        #region Sprite Helpers
        public void GetThumbnailSprite(AsyncCallback<Sprite> completed)
        {
            GetAvatarSprite(result =>
            {
                if (result.Data != null)
                {
                    float originalHeight = result.Data.texture.height;
                    float height = originalHeight / 1.5f;
                    completed?.Invoke(new AsyncResult<Sprite>(
                        Sprite.Create(result.Data.texture,
                            new Rect(0, originalHeight - height, result.Data.texture.width, height),
                            Vector2.one / 2f), 
                        string.Empty)
                    );
                }
                else
                    completed?.Invoke(result);
            });
        }

        public void GetAvatarSprite(AsyncCallback<Sprite> completed)
        {
            GetAvatarSprite(0, 0, completed);
        }

        public void GetAvatarSprite(int width, int height, AsyncCallback<Sprite> completed)
        {
            LoadSprite(_avatarSpriteCache, ThumbnailPath, width, height, completed);
        }

        public void ReleaseResource()
        {
            if (_thumbnailSpriteCache != null)
                Object.DestroyImmediate(_thumbnailSpriteCache.texture, true);
            if (_avatarSpriteCache != null)
                Object.DestroyImmediate(_avatarSpriteCache.texture, true);
            _thumbnailSpriteCache = _avatarSpriteCache = null;
        }
        #endregion

        public bool IsAtClassroom(Classroom classroom)
        {
            return IsAtClassroom(classroom.Id);
        }
        public bool IsAtClassroom(long classroomId)
        {
            return Classrooms.Find(classroom => classroom.Id == classroomId) != null;
        }

        public override AlunoVm GetVm()
        {
            return new AlunoVm()
            {
                Id = Id,
                Nome = Name,
                AlunoTurmas = Classrooms.Select(classroom => new AlunoTurmaVm(Id, classroom.Id, classroom.GetVm())).ToList(),
                AlunoGuid = GUID,
                Lixeira = Deleted,
                Excluido = Deleted,
                DataCriacao = CreatedAt,
                DataAtualizacao = UpdatedAt,
                ThumbnailPath = ThumbnailPath,
                DataExclusao = DeletedAt,
                LastTimePlayed = LastTimePlayed,
                ElementosAluno = Elements.Select(element => new ElementosAlunoVm(Id, element.Id, element.GetVm())).ToList(),
            };
        }
        public override void SetDataFromVm(AlunoVm vm)
        {
            Id = vm.Id;
            Name = vm.Nome;
            GUID = vm.AlunoGuid;
            Deleted = vm.Lixeira || vm.Excluido;
            CreatedAt = vm.DataCriacao;
            ThumbnailPath = vm.ThumbnailPath;
            UpdatedAt = vm.DataAtualizacao;
            LastTimePlayed = vm.LastTimePlayed;
            DeletedAt = vm.DataExclusao.HasValue ? vm.DataExclusao : vm.DataAtualizacao;
            // ----
            if(vm.AlunoTurmas != null)
                Classrooms = vm.AlunoTurmas.Select(aluTur => new Classroom(aluTur.Turma)).ToList();
            // ----
            if(vm.ElementosAluno != null)
            {
                Elements = vm.ElementosAluno.Select(elemAlu => new Element(elemAlu.Elemento)).ToList();
                Elements = Elements.OrderBy(el => el.CategoryOrder).ToList();
            }
        }

        public Element GetElement(long categoryID)
        {
            return Elements.Find(cat => cat.CategoryId == categoryID);
        }
        public void SetElement(long categoryID, Element element)
        {
            Element avatarElement = GetElement(categoryID);
            if (avatarElement != null)
                Elements.Remove(avatarElement);

            Elements.Add(element);
        }

        public override string ToString()
        {
            return $"{GUID} => Name: {Name}; Classes: {string.Join(",", Classrooms.Select(classroom => classroom.Name))};" +
                $" IsVillian: {IsVillain}; \nAvatar: {Avatar}";
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
    }
}
