using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using Playmove.Core.Bundles;

namespace Playmove.Core.Editor
{
    public static class DownloadLocalization
    {
        private static Action<string> _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="completed"></param>
        public static void UpdateLocalization(Action completed, Action<string> logEvent)
        {
            _log = logEvent;
            Log("Start localization update");
            DownloadRegisteredLanguages((languages) =>
            {
                if (languages.Count > 0)
                {
                    Log("Downloading localization files for all languages");
                    DownloadLocalizationFilesForLanguages(languages, (filesContent) =>
                    {
                        Log("Saving files for all languages");
                        for (int i = 0; i < filesContent.Count; i++)
                        {
                            try
                            {
                                var bundleAssetPath = string.Format("{0}/{1}", 
                                    PlaytableBundlesPath.GetGlobalBundlePath(BundleType.Localization), 
                                    languages[i].ToLower());
                                Log("Writing file for language " + languages[i] + " at " + bundleAssetPath);

                                if (string.IsNullOrEmpty(filesContent[i]))
                                    continue;

                                if (!Directory.Exists(bundleAssetPath))
                                    Directory.CreateDirectory(bundleAssetPath);

                                File.WriteAllText(bundleAssetPath + "/strings.json", filesContent[i]);
                                Log("Success: Saved file correctly!");
                            }
                            catch (Exception e)
                            {
                                Log("Error: Could not save file | " + e.ToString());
                            }
                        }

                        AssetDatabase.Refresh();
                        AssetDatabase.SaveAssets();

                        Log("Update completed!");
                        completed?.Invoke();
                    });
                }
                else
                {
                    Log("Update completed!");
                    completed?.Invoke();
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="completed"></param>
        private static void DownloadRegisteredLanguages(Action<List<string>> completed)
        {
            List<string> languages = new List<string>();
            string url = string.Format(@"https://api.playmove.com.br/api/v2/LocalizacaoApps/ListarLocalizacaoApp?executavel={0}.exe",
                GameSettings.ExecutableName);

            Log("Requesting Playmove service to get languages for this game");
            Request(url, (jsonContent) =>
                {
                    if (string.IsNullOrEmpty(jsonContent))
                    {
                        Log("Error: Did not found any language registered for this game. Contact Playmove Support.");
                        completed?.Invoke(languages);
                        return;
                    }

                    var dummyType = new[] { new { Localizacao = "" } };
                    var gameLanguages = JsonConvert.DeserializeAnonymousType(jsonContent, dummyType);
                    foreach (var language in gameLanguages)
                        languages.Add(language.Localizacao);

                    Log("Success: Found the following languages: " + string.Join("; ", languages));
                    completed?.Invoke(languages);
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="languages">List of registered languages on to Playmove Intranet</param>
        /// <param name="completed">Complete callback containing the file content for each language</param>
        private static void DownloadLocalizationFilesForLanguages(List<string> languages, Action<List<string>> completed, 
            int currentLanguage = 0, List<string> filesContent = null)
        {
            if (filesContent == null)
                filesContent = new List<string>();

            if (currentLanguage > languages.Count - 1)
            {
                completed?.Invoke(filesContent);
                return;
            }

            DownloadLocalizationFile(languages[currentLanguage], (fileContent) =>
            {
                if (string.IsNullOrEmpty(fileContent))
                {
                    filesContent.Add(string.Empty);
                    DownloadLocalizationFilesForLanguages(languages, completed, ++currentLanguage, filesContent);
                }
                else
                {
                    filesContent.Add(fileContent);
                    DownloadLocalizationFilesForLanguages(languages, completed, ++currentLanguage, filesContent);
                }
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="language">Language registered on to Playmove Intranet</param>
        /// <param name="completed">Complete callback container the file content for the specified language</param>
        private static void DownloadLocalizationFile(string language, Action<string> completed)
        {
            string url = string.Format("https://api.playmove.com.br/api/v2/LocalizacaoApps/ListarTodos?Filtro.Executavel={0}.exe&Filtro.Localizacao={1}",
                GameSettings.ExecutableName, language);

            Log("Requesting localization file for language " + language);
            Request(url, (jsonContent) =>
            {
                if (string.IsNullOrEmpty(jsonContent))
                {
                    Log("Warning: Did not found localization file for the language " + language);
                    completed?.Invoke(string.Empty);
                    return;
                }

                var dummyType = new[] { new { TagNome = "", Texto = "" } };
                var localizedTags = JsonConvert.DeserializeAnonymousType(jsonContent, dummyType);

                Log("Success: Downloaded localization file for language " + language);
                completed?.Invoke(JsonConvert.SerializeObject(localizedTags));
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="completed"></param>
        private static void Request(string url, Action<string> completed)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization-Id", "gerenciarapps@playmove.com.br");
            request.SetRequestHeader("Authorization-Token", "8505956C87EE4DED9FE34201A1BCF2FA");
            request.SetRequestHeader("Authorization-Role", "30");

            request.SendWebRequest().completed += _ =>
            {
                if (string.IsNullOrEmpty(request.error))
                    completed(request.downloadHandler.text);
                else
                    Log("Error: Requesting error " + request.error);
            };
        }

        private static void Log(string log)
        {
            _log?.Invoke(log);
        }
    }
}
