using Newtonsoft.Json;
using Playmove.Core.API.Services;
using Playmove.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Playmove.Core.API
{
    /// <summary>
    /// Core Playtable APIs
    /// </summary>
    public static class PlaytableAPI
    {
        private static ApplicationService _application;
        /// <summary>
        /// Service to work with application
        /// </summary>
        public static ApplicationService Application
        {
            get
            {
                if (_application == null)
                    _application = new ApplicationService();
                return _application;
            }
        }

        private static ThemeService _theme;
        /// <summary>
        /// Service to work with themes
        /// </summary>
        public static ThemeService Theme
        {
            get
            {
                if (_theme == null)
                    _theme = new ThemeService();
                return _theme;
            }
        }

        private static PlaytableFileService _file;
        /// <summary>
        /// Service to work with files
        /// </summary>
        public static PlaytableFileService File
        {
            get
            {
                if (_file == null)
                    _file = new PlaytableFileService();
                return _file;
            }
        }

        private static PrefsService _prefs;
        /// <summary>
        /// Service to access Playtable Setups
        /// </summary>
        public static PrefsService Prefs
        {
            get
            {
                if (_prefs == null)
                    _prefs = new PrefsService();
                return _prefs;
            }
        }

        /// <summary>
        /// Authenticate game in SOP
        /// </summary>
        /// <param name="executableName">Game executable name</param>
        /// <param name="completed">Callback with the result of authentication</param>
        public static void Authenticate(string executableName, AsyncCallback<string> completed)
        {
            WebRequestWrapper.Instance.Get("/v1", "/TicketProvider/Authenticate", 
                new Dictionary<string, string> { { "gameTicket", executableName } }, 
                result => completed?.Invoke(new AsyncResult<string>(result.Data.text, result.Error)));
        }

        /// <summary>
        /// Get playtable inactivity time
        /// </summary>
        public static void GetInactiveTime(AsyncCallback<int> completed = null)
        {
            WebRequestWrapper.Instance.Get("/Configuracoes/GetInactiveTime", 
                result =>
                {
                    int time = 0;
                    if (!result.HasError)
                    {
                        if (!int.TryParse(result.Data.text, out time))
                            result.Error = $"Could not parse string ({result.Data.text}) to float!";
                    }

                    completed?.Invoke(new AsyncResult<int>(time, result.Error));
                }
            );
        }

        /// <summary>
        /// Set playtable screen orientation
        /// </summary>
        public static void RotateScreen(AsyncCallback<bool> completed)
        {
            WebRequestWrapper.Instance.Get("/Configuracoes/RotateScreen",
                result =>
                {
                    completed?.Invoke(new AsyncResult<bool>(true, string.Empty));
                }
            );
        }

        /// <summary>
        /// Get playtable volume
        /// </summary>
        /// <param name="completed">Callback with the current volume</param>
        public static void GetVolume(AsyncCallback<float> completed)
        {
            WebRequestWrapper.Instance.Get("/Configuracoes/GetVolume", result =>
            {
                float volume = 0;
                if (!result.HasError)
                {
                    if (!float.TryParse(result.Data.text, out volume))
                        result.Error = $"Could not parse string ({result.Data.text}) to float!";
                }

                completed?.Invoke(new AsyncResult<float>(volume / 100, result.Error));
            });
        }
        /// <summary>
        /// Set playtable volume
        /// </summary>
        /// <param name="volume">Volume to be set</param>
        /// <param name="completed">Callback with result</param>
        public static void SetVolume(float volume, AsyncCallback<bool> completed = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "volume", (volume * 100).ToString() }
            };
            WebRequestWrapper.Instance.Get("/Configuracoes/SetVolume", parameters, 
                result =>
                {
                    bool data = true;
                    if (result.HasError)
                        data = false;

                    completed?.Invoke(new AsyncResult<bool>(data, result.Error));
                }
            );
        }

        /// <summary>
        /// Get playtable mute
        /// </summary>
        /// <param name="completed">Callback with the current mute state</param>
        public static void GetMute(AsyncCallback<bool> completed)
        {
            WebRequestWrapper.Instance.Get("/Configuracoes/IsInMute", 
                result =>
                {
                    bool mute = false;
                    try
                    {
                        if (!result.HasError)
                            mute = Convert.ToBoolean(result.Data.text);
                    }
                    catch
                    {
                        result.Error = $"Could not convert value ({result.Data.text}) to type bool!";
                    }

                    completed?.Invoke(new AsyncResult<bool>(mute, result.Error));
                }
            );
        }
        /// <summary>
        /// Set playtable mute
        /// </summary>
        /// <param name="mute">Mute to be set</param>
        /// <param name="completed">Callback with result</param>
        public static void SetMute(bool mute, AsyncCallback<string> completed = null)
        {
            WebRequestWrapper.Instance.Get(mute ? "/Configuracoes/Mute" : "/Configuracoes/UnMute", 
                result =>
                {
                    string data = string.Empty;
                    if (!result.HasError)
                        data = "Success";

                    completed?.Invoke(new AsyncResult<string>(data, result.Error));
                }
            );
        }

        public static void GetMicrophoneSensitivity(AsyncCallback<float> completed = null)
        {
            WebRequestWrapper.Instance.Get("/Configuracoes/GetMicrophoneSensitivity", result =>
            {
                float volume = 0;
                if (!result.HasError)
                {
                    if (!float.TryParse(result.Data.text, out volume))
                        result.Error = $"Could not parse string ({result.Data.text}) to float!";
                }

                completed?.Invoke(new AsyncResult<float>(volume / 100, result.Error));
            });
        }

        public static void SetMicrophoneSensitivity(float sensitivity, AsyncCallback<bool> completed = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "volume", (sensitivity * 100).ToString() }
            };
            WebRequestWrapper.Instance.Get("/Configuracoes/SetMicrophoneSensitivity", parameters,
                result =>
                {
                    var data = true;
                    if (result.HasError)
                        data = false;

                    completed?.Invoke(new AsyncResult<bool>(data, result.Error));
                }
            );
        }

        public static void GetBannedWords(AsyncCallback<BannedWordsData> completed)
        {
            WebRequestWrapper.Instance.Get("/v1", "/Settings/GetRecordes",
                new Dictionary<string, string> {
                    { "gameId", "BannedWords" },
                    { "gameDifficult", "0" }
                },
                result =>
                {
                    if (result.HasError)
                        completed?.Invoke(new AsyncResult<BannedWordsData>(null, result.Error));
                    else
                    {
                        try
                        {
                            BannedWordsData bannedWords = JsonConvert.DeserializeObject<BannedWordsData>(result.Data.text);
                            completed?.Invoke(new AsyncResult<BannedWordsData>(bannedWords, string.Empty));
                        }
                        catch
                        {
                            completed?.Invoke(new AsyncResult<BannedWordsData>(null, result.Error));
                        }
                    }
                });
        }
        public static void SetBannedWords(BannedWordsData bannedWords, AsyncCallback<bool> completed = null)
        {
            WebRequestWrapper.Instance.Get("/v1", "/Settings/SetRecordes",
                new Dictionary<string, string> {
                    { "gameId", "BannedWords" },
                    { "gameDifficult", "0" },
                    { "gameScores", JsonConvert.SerializeObject(bannedWords) }
                },
                result =>
                {
                    if (result.HasError)
                        completed?.Invoke(new AsyncResult<bool>(false, result.Error));
                    else
                        completed?.Invoke(new AsyncResult<bool>(true, string.Empty));
                });
        }
    }
}
