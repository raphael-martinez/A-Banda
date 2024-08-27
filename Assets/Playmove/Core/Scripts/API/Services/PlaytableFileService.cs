using Newtonsoft.Json;
using Playmove.Core.API.Models;
using Playmove.Core.API.Vms;
using Playmove.Core.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Playmove.Core.API.Services
{
    /// <summary>
    /// Responsible to access APIs for Files
    /// </summary>
    public class PlaytableFileService : Service<PlaytableFile, ArquivoAplicativoVm>
    {
        /// <summary>
        /// Get all gallery files for this game from all languages
        /// </summary>
        /// <param name="completed">Callback containing a list of all files or error</param>
        public void GetGalleryFiles(AsyncCallback<List<PlaytableFile>> completed)
        {
            GetGalleryFiles(GameSettings.Language, completed);
        }
        /// <summary>
        /// Get all gallery files for this game filtered by the specified language
        /// </summary>
        /// <param name="language">Files language</param>
        /// <param name="completed">Callback containing a list of all files or error</param>
        public void GetGalleryFiles(string language, AsyncCallback<List<PlaytableFile>> completed)
        {
            WebRequestWrapper.Instance.Get("/ArquivoAplicativos/GetAllByAppFromStudent", new Dictionary<string, string> {
                { "aplicativoId", GameSettings.ApplicationId.ToString() },
                { "arquivosComAluno", true.ToString() }
            }, result =>
            {
                AsyncResult<List<PlaytableFile>> parsedResult = ParseVmsJson(result);
                if (!parsedResult.HasError)
                    parsedResult.Data = parsedResult.Data.Where((file) => !file.Deleted && file.Language.ToLower() == language.ToLower()).ToList();
                completed?.Invoke(parsedResult);
            });
        }
        /// <summary>
        /// Get all files belonging to the specified player
        /// </summary>
        /// <param name="playerId">Id of the player that has the files</param>
        /// <param name="completed">Callback containing a list of all files or error</param>
        public void GetPlayerFiles(long playerId, AsyncCallback<List<PlaytableFile>> completed)
        {
            var parameters = new Dictionary<string, string>
            {
                { "AplicativoId", GameSettings.ApplicationId.ToString() },
                { "AlunoId", playerId.ToString() },
                { "HistoricoId", "-1" },
                { "TurmaId", "-1" },
                { "AgrupamentoId", "-1" }
            };
            WebRequestWrapper.Instance.Get("/ArquivoAplicativos/GetAllByFilter", parameters,
                result =>
                {
                    if (result.HasError)
                    {
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(null, result.Error));
                        return;
                    }

                    List<PlaytableFile> files = new List<PlaytableFile>();
                    try
                    {
                        var filesGroupedVm = JsonConvert.DeserializeObject<Dictionary<string, List<ArquivoAplicativoVm>>>(result.Data.text);
                        foreach (var fileVm in filesGroupedVm.FirstOrDefault().Value)
                            files.Add(new PlaytableFile(fileVm));

                        files = files.Where(file => !file.Deleted).ToList();
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(files, string.Empty));
                    }
                    catch (Exception e)
                    {
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(null, e.ToString()));
                    }
                });
        }
        /// <summary>
        /// Get all files belonging to the specified player and filtered by language
        /// </summary>
        /// <param name="playerId">Id of the player that has the files</param>
        /// <param name="language">Files language</param>
        /// <param name="completed">Callback containing a list of all files or error</param>
        public void GetPlayerFiles(long playerId, string language, AsyncCallback<List<PlaytableFile>> completed)
        {
            GetPlayerFiles(playerId,
                result =>
                {
                    if (result.HasError)
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(null, result.Error));
                    else
                    {
                        var files = result.Data.Where((file) => file.Language.ToLower() == language.ToLower()).ToList();
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(files, string.Empty));
                    }
                }
            );
        }
        /// <summary>
        /// Get all files belonging to the specified classroom
        /// </summary>
        /// <param name="classroomId">Id of the classroom that has the files</param>
        /// <param name="completed">Callback containing a list of all files or error</param>
        public void GetClassroomFiles(long classroomId, AsyncCallback<List<PlaytableFile>> completed)
        {
            var parameters = new Dictionary<string, string>
            {
                { "AplicativoId", GameSettings.ApplicationId.ToString() },
                { "AlunoId", "-1" },
                { "HistoricoId", "-1" },
                { "TurmaId", classroomId.ToString() },
                { "AgrupamentoId", "-1" }
            };
            WebRequestWrapper.Instance.Get("/ArquivoAplicativos/GetAllByFilter", parameters, 
                result =>
                {
                    if (result.HasError)
                    {
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(null, result.Error));
                        return;
                    }

                    List<PlaytableFile> files = new List<PlaytableFile>();
                    try
                    {
                        var filesGroupedVm = JsonConvert.DeserializeObject<Dictionary<string, List<ArquivoAplicativoVm>>>(result.Data.text);
                        foreach (var fileVm in filesGroupedVm.FirstOrDefault().Value)
                            files.Add(new PlaytableFile(fileVm));

                        files = files.Where(file => !file.Deleted).ToList();
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(files, string.Empty));
                    }
                    catch (Exception e)
                    {
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(null, e.ToString()));
                    }
                }
            );
        }
        /// <summary>
        /// Get all files belonging to the specified classroom and filtered by language
        /// </summary>
        /// <param name="classroomId">Id of the classroom that has the files</param>
        /// <param name="language">Files language</param>
        /// <param name="completed">Callback containing a list of all files or error</param>
        public void GetClassroomFiles(long classroomId, string language, AsyncCallback<List<PlaytableFile>> completed)
        {
            GetClassroomFiles(classroomId, 
                result =>
                {
                    if (result.HasError)
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(null, result.Error));
                    else
                    {
                        var files = result.Data.Where(file => file.Language.ToLower() == language.ToLower()).ToList();
                        completed?.Invoke(new AsyncResult<List<PlaytableFile>>(files, string.Empty));
                    }
                }
            );
        }

        public void GetFilesInTrash(AsyncCallback<List<PlaytableFile>> completed)
        {
            WebRequestWrapper.Instance.Get("/ArquivoAplicativos/GetAllInTrash", new Dictionary<string, string> { { "lixeira", "True" } },
                result => completed?.Invoke(ParseVmsJson(result)));
        }

        /// <summary>
        /// Get specific file
        /// </summary>
        /// <param name="id">Id of file</param>
        /// <param name="completed">Callback containing the requested file or error</param>
        public void Get(long id, AsyncCallback<PlaytableFile> completed)
        {
            WebRequestWrapper.Instance.Get("/ArquivoAplicativos/Get", new Dictionary<string, string> { { "id", id.ToString() } },
                result => completed?.Invoke(ParseVmJson(result)));
        }
        /// <summary>
        /// Get specific file
        /// </summary>
        /// <param name="GUID">GUID of file</param>
        /// <param name="completed">Callback containing the requested file or error</param>
        public void Get(string GUID, AsyncCallback<PlaytableFile> completed)
        {
            completed?.Invoke(new AsyncResult<PlaytableFile>(null, "Not implemented yet!"));
        }
        public void GetByPath(string path, AsyncCallback<PlaytableFile> completed)
        {
            WebRequestWrapper.Instance.Get("/ArquivoAplicativos/GetAll",
                (result) =>
                {
                    PlaytableFile fileByPath = null;
                    AsyncResult<List<PlaytableFile>> parsedResult = ParseVmsJson(result);
                    if (!parsedResult.HasError)
                        fileByPath = parsedResult.Data.FirstOrDefault(file => file.FullPath.Contains(path));
                    completed?.Invoke(new AsyncResult<PlaytableFile>(fileByPath, string.Empty));
                });
        }

        public void Add(Sprite sprite, PlaytableFileProperties properties, AsyncCallback<PlaytableFile> completed)
        {
            Add(sprite.texture, properties, completed);
        }
        public void Add(Texture2D texture, PlaytableFileProperties properties, AsyncCallback<PlaytableFile> completed)
        {
            if (string.IsNullOrEmpty(properties.Extension))
                properties.Extension = ".png";
            Add(texture.EncodeToPNG(), properties, completed);
        }
        public void Add(byte[] fileBytes, PlaytableFileProperties properties, AsyncCallback<PlaytableFile> completed)
        {
            if (properties.Size == 0)
                properties.Size = fileBytes.Length;
            WebRequestWrapper.Instance.Post("/ArquivoAplicativos/Add", new PlaytableFile(properties).GetVmJson(),
                result =>
                {
                    var resultCallback = ParseVmJson(result);
                    if (resultCallback.HasError)
                    {
                        completed?.Invoke(resultCallback);
                        return;
                    }

                    Storage.WriteFile(resultCallback.Data.FullPath, fileBytes, true,
                        resultFile =>
                        {
                            if (resultFile.HasError)
                                completed?.Invoke(new AsyncResult<PlaytableFile>(resultCallback.Data, resultFile.Error));
                            else
                                completed?.Invoke(resultCallback);
                        }
                    );
                }
            );
        }

        public void Add(PlaytableFileProperties properties, AsyncCallback<PlaytableFile> completed)
        {
            WebRequestWrapper.Instance.Post("/ArquivoAplicativos/Add", new PlaytableFile(properties).GetVmJson(),
                (result) => completed?.Invoke(ParseVmJson(result)));
        }

        /// <summary>
        /// Update file data in playtable
        /// </summary>
        /// <param name="file">File to be updated in playtable</param>
        /// <param name="completed">Callback containing the result of the operation or error</param>
        public void Update(PlaytableFile file, AsyncCallback<bool> completed)
        {
            WebRequestWrapper.Instance.Post("/ArquivoAplicativos/Update", file.GetVmJson(),
                result => completed?.Invoke(SimpleResult(result)));
        }

        /// <summary>
        /// Delete a file from playtable, this will make the file go to trash
        /// </summary>
        /// <param name="file">File to be deleted</param>
        /// <param name="completed">Callback containing the result of the operation or error</param>
        public void Delete(PlaytableFile file, AsyncCallback<bool> completed)
        {
            Delete(file.Id, completed);
        }
        /// <summary>
        /// Delete a file from playtable, this will make the file go to trash
        /// </summary>
        /// <param name="id">Id of file to be deleted</param>
        /// <param name="completed">Callback containing the result of the operation or error</param>
        public void Delete(long id, AsyncCallback<bool> completed)
        {
            WebRequestWrapper.Instance.Post("/ArquivoAplicativos/Delete", id.ToString(),
                result => completed?.Invoke(SimpleResult(result)));
        }
    }
}
