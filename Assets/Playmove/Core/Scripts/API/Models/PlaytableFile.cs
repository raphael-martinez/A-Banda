using Newtonsoft.Json;
using Playmove.Avatars.API;
using Playmove.Avatars.API.Models;
using Playmove.Core.API.Vms;
using Playmove.Core.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Playmove.Core.API.Models
{
    [Serializable]
    public class PlaytableFileProperties
    {
        public long ApplicationId;
        public List<long> PlayersId = new List<long>();
        public string Name;
        public string Data;
        public string Grouping;
        public float Size;
        public string Language;
        public string Extension;
    }

    [Serializable]
    public class PlaytableFile : VmItem<ArquivoAplicativoVm>, IDatabaseItem, ITrash
    {
        public string GUID { get; set; }
        public long ApplicationId { get; set; }
        public List<long> PlayersId { get; set; } = new List<long>();
        public long? ThemeId { get; set; }
        public string Name
        {
            get { return RawFile.Name; }
            set { RawFile.Name = value; }
        }
        public string Data { get; set; }
        public string Grouping { get; set; }
        public float Size
        {
            get { return RawFile.Size; }
            set { RawFile.Size = value; }
        }
        public string Language
        {
            get { return RawFile.Language; }
            set { RawFile.Language = value; }
        }
        public string Extension
        {
            get { return RawFile.Extension; }
            set { RawFile.Extension = value; }
        }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool Deleted { get; set; }

        public string FullPath
        {
            get { return RawFile.FullPath; }
            set { RawFile.FullPath = value; }
        }
        public bool Exists
        {
            get { return File.Exists(FullPath); }
        }

        public RawFile RawFile { get; private set; }

        [JsonIgnore]
        protected Sprite _spriteCache;

        public PlaytableFile()
        {
            RawFile = new RawFile();
        }
        public PlaytableFile(PlaytableFileProperties properties)
        {
            RawFile = new RawFile();

            ApplicationId = properties.ApplicationId;
            PlayersId = properties.PlayersId;
            Name = properties.Name;
            Data = properties.Data;
            Grouping = properties.Grouping;
            Size = properties.Size;
            Language = properties.Language;
            Extension = properties.Extension;
        }
        public PlaytableFile(ArquivoAplicativoVm vm)
        {
            SetDataFromVm(vm);
        }
        public PlaytableFile(string vmJson)
        {
            SetDataFromVmJson(vmJson);
        }

        public PlaytableFile GetCopy()
        {
            return new PlaytableFile()
            {
                RawFile = new RawFile(),
                ApplicationId = ApplicationId,
                PlayersId = new List<long>(PlayersId),
                ThemeId = ThemeId,
                Name = Name,
                Data = Data,
                Grouping = Grouping,
                Size = Size,
                Language = Language,
                Extension = Extension,
                Deleted = Deleted
            };
        }
        
        /// <summary>
        /// This should be avoided
        /// </summary>
        /// <param name="sprite">Sprite to be setted</param>
        public void SetSpriteCache(Sprite sprite)
        {
            _spriteCache = sprite;
        }
        public void GetSprite(AsyncCallback<Sprite> completed)
        {
            GetSprite(0, 0, false, completed);
        }
        public void GetSprite(int width, int height, bool keepAspect, AsyncCallback<Sprite> completed)
        {
            if (_spriteCache != null)
            {
                if (_spriteCache.texture.width == width && _spriteCache.texture.height == height)
                {
                    completed?.Invoke(new AsyncResult<Sprite>(_spriteCache, string.Empty));
                    return;
                }

                width = width > 0 ? width : _spriteCache.texture.width;
                height = height > 0 ? height : _spriteCache.texture.height;
                if (width > 0 || height > 0)
                    _spriteCache = _spriteCache.Scaled(width, height, keepAspect);

                completed?.Invoke(new AsyncResult<Sprite>(_spriteCache, string.Empty));
                return;
            }
            Storage.LoadSprite(FullPath, width, height, keepAspect, (result) =>
            {
                if (!result.HasError) _spriteCache = result.Data;
                completed?.Invoke(result);
            });
        }

        public void ReleaseResource()
        {
            if (_spriteCache != null)
                Object.DestroyImmediate(_spriteCache.texture, true);
            _spriteCache = null;
        }

        public void GetPlayers(AsyncCallback<List<Player>> completed)
        {
            GetPlayersRecursive(0, new List<Player>(), completed);
        }
        private void GetPlayersRecursive(int indexId, List<Player> players, AsyncCallback<List<Player>> completed)
        {
            if (indexId > PlayersId.Count - 1)
            {
                completed?.Invoke(new AsyncResult<List<Player>>(players, string.Empty));
                return;
            }

            AvatarAPI.GetPlayer(PlayersId[indexId], result =>
            {
                if (result.Data != null)
                    players.Add(result.Data);
                GetPlayersRecursive(++indexId, players, completed);
            });
        }

        public float GetSize(SizeFormat sizeFormat)
        {
            return Size / (float)sizeFormat;
        }

        public void GetTheme(AsyncCallback<Theme> callback)
        {
            if (!ThemeId.HasValue)
            {
                callback?.Invoke(new AsyncResult<Theme>(null, "File does not belong to any Theme!"));
                return;
            }
            PlaytableAPI.Theme.Get(ThemeId.Value, callback);
        }

        public override ArquivoAplicativoVm GetVm()
        {
            if (PlayersId == null)
                PlayersId = new List<long>();

            return new ArquivoAplicativoVm()
            {
                Id = Id,
                Guid = GUID,
                DataCriacao = CreatedAt,
                DataAtualizacao = UpdatedAt,
                Lixeira = Deleted,
                AplicativoId = ApplicationId,
                GrupoAplicativoId = ThemeId,
                ArquivoAlunos = PlayersId.Select(playerId => new ArquivoAluno() { AlunoId = playerId }).ToList(),
                Configuracao = Data,
                Agrupamento = Grouping,
                Arquivo = RawFile.GetVm()
            };
        }

        public override void SetDataFromVm(ArquivoAplicativoVm vm)
        {
            Id = vm.Id;
            GUID = vm.Guid;
            CreatedAt = vm.DataCriacao;
            UpdatedAt = vm.DataAtualizacao;
            Deleted = vm.Lixeira;
            DeletedAt = vm.DataAtualizacao;
            ApplicationId = vm.AplicativoId;
            ThemeId = vm.GrupoAplicativoId;
            PlayersId = vm.ArquivoAlunos.Select(arqAluno => arqAluno.AlunoId).ToList();
            Data = vm.Configuracao;
            Grouping = vm.Agrupamento;
            RawFile = new RawFile(vm.Arquivo);
        }
    }
}
