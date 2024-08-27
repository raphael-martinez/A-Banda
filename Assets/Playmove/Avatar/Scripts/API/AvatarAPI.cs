using Playmove.Avatars.API.Interfaces;
using Playmove.Avatars.API.Models;
using Playmove.Avatars.API.Services;
using Playmove.Avatars.API.Vms;
using Playmove.Core;
using Playmove.Core.API.Models;
using Playmove.Core.API.Services;
using Playmove.Core.Audios;
using Playmove.Core.BasicEvents;
using Playmove.Core.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using AsyncCallback = Playmove.Core.AsyncCallback;

namespace Playmove.Avatars.API
{
    /// <summary>
    /// Responsible to access APIs for Avatar
    /// </summary>
    public static class AvatarAPI
    {
        private static string _rootAvatarPath = string.Empty;
        public static string RootAvatarPath
        {
            get
            {
                if (string.IsNullOrEmpty(_rootAvatarPath))
                    _rootAvatarPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PlayTable"));

                return _rootAvatarPath;
            }
        }

        // Posicionamento
        private static string _avatarLockFilePath = string.Empty;
        public static string AvatarLockFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_avatarLockFilePath))
                    _avatarLockFilePath = $"{RootAvatarPath}/avatar.lock";
                return _avatarLockFilePath;
            }
        }

        public static bool IsAvatarLock
        {
            get { return File.Exists(AvatarLockFilePath); }
        }
        // ---

        // League
        private static string _leagueLockFilePath = string.Empty;
        public static string LeagueLockFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_leagueLockFilePath))
                    _leagueLockFilePath = $"{RootAvatarPath}/league.lock";
                return _leagueLockFilePath;
            }
        }

        public static bool IsLeagueLock
        {
            get { return File.Exists(LeagueLockFilePath); }
        }
        // End League

        private static List<Slot> _slotsCache = new List<Slot>();
        public static List<Slot> CurrentSlots
        {
            get
            {
                return _slotsCache;
            }
            set
            {
                _slotsCache = value;
            }
        }

        public static List<Slot> SlotsPlaying
        {
            get
            {
                return _slotsCache.Where(x => x.Players.Count > 0).ToList();
            }
        }

        public static List<Player> PlayersFromSlot(int slotPos)
        {
            return _slotsCache.Where(x => x.Pos == slotPos).SelectMany(x => x.Players).ToList();
        }

        public static List<long> PlayersIdsFromSlot(int slotPos)
        {
            return _slotsCache.Where(x => x.Pos == slotPos).SelectMany(x => x.PlayersId).ToList();
        }

        private static List<long> _allPlayersId = new List<long>();
        public static List<long> AllPlayersId
        {
            get {
                if (_allPlayersId.Count == 0)
                    _allPlayersId = CurrentSlots.SelectMany(x => x.Players).Select(x => x.Id).ToList();

                return _allPlayersId;
            }
            set
            {
                _allPlayersId = value;
            }
        }

        private static List<Player> _allPlayers = new List<Player>();
        public static List<Player> AllPlayers
        {
            get
            {
                if (_allPlayers.Count == 0)
                    _allPlayers = CurrentSlots.SelectMany(x => x.Players).ToList();

                return _allPlayers;
            }
            set
            {
                _allPlayers = value;
            }
        }

        /// <summary>
        /// Verification if there's the Minimum Necessary Players
        /// </summary>
        public static bool HasMinimumNecessaryPlayers
        {
            get
            {
                int slotsWithPlayer = _slotsCache
                    .Where(slot => slot.Players.Find(player => !player.IsVillain) != null)
                    .Count();
                return slotsWithPlayer >= GameSettings.MinSlots;
            }
        }

        /// <summary>
        /// Event dispatched after slots changed
        /// </summary>
        public static PlaytableEvent<AsyncResult<List<Slot>>> OnSlotsMayChanged = new PlaytableEvent<AsyncResult<List<Slot>>>();

        /// <summary>
        /// Service to access Classroom
        /// </summary>
        private static ClassroomService _classroom;
        public static ClassroomService Classroom
        {
            get
            {
                if (_classroom == null)
                    _classroom = new ClassroomService();
                return _classroom;
            }
        }

        /// <summary>
        /// Service to access Avatar
        /// </summary>
        private static AvatarService _avatar;
        public static AvatarService Avatar
        {
            get
            {
                if (_avatar == null)
                    _avatar = new AvatarService();
                return _avatar;
            }
        }

        private static SlotsServices _slots;
        /// <summary>
        /// Service to access Slots
        /// </summary>
        public static SlotsServices Slots
        {
            get
            {
                if (_slots == null)
                    _slots = new SlotsServices();
                return _slots;
            }
        }

        /// <summary>
        /// Service to access Elements Categories
        /// </summary>
        private static CategoryService _categories;
        public static CategoryService Categories
        {
            get
            {
                if (_categories == null)
                    _categories = new CategoryService();
                return _categories;
            }
        }

        /// <summary>
        /// Get all players from specified classroom containing the avatar icon
        /// </summary>
        /// <param name="classGUID">Classroom from where you want the players</param>
        /// <param name="completed">Callback containing all players from the classroom or error</param>
        public static void GetPlayersThumbnail(long classroomId, AsyncCallback<List<Player>> completed)
        {
            Avatar.GetPlayersThumbnail(classroomId, completed);
        }

        /// <summary>
        /// Get a player from its id
        /// </summary>
        /// <param name="id">Id of the player</param>
        /// <param name="completed">Callback containing the player requested or error</param>
        public static void GetPlayer(long? id, AsyncCallback<Player> completed)
        {
            Avatar.GetPlayer(id, completed);
        }

        /// <summary>
        /// Get player from its GUID
        /// </summary>
        /// <param name="GUID">GUID of the player you want</param>
        /// <param name="completed">Callback containing the player or error</param>
        public static void GetPlayer(string GUID, AsyncCallback<Player> completed)
        {
            Avatar.GetPlayer(GUID, completed);
        }

        /// <summary>
        /// Get all avatar elements for the specified player
        /// </summary>
        /// <param name="playerGUID">Player that you want the elements of</param>
        /// <param name="completed">Callback containing all elements that the player has or error</param>
        public static void GetElements(string playerGUID, AsyncCallback<List<Category>> completed)
        {
            Categories.LoadElements(completed);
        }

        /// <summary>
        /// Unlock specific element for the specified slot
        /// </summary>
        /// <param name="elementGUID">GUID of the element you want to unlock</param>
        /// <param name="slot">Slot containing the players that will have the element unlocked</param>
        /// <param name="completed">Callback containing the result of the operation or error</param>
        public static void UnlockElement(string elementGUID, Slot slot, AsyncCallback<bool> completed)
        {
            Avatar.UnlockElement(elementGUID, slot, completed);
        }
        /// <summary>
        /// Unlock specific element for the specified players
        /// </summary>
        /// <param name="elementGUID">GUID of the element you want to unlock</param>
        /// <param name="players">Players that will have the element unlocked</param>
        /// <param name="completed">Callback containing the result of the operation or error</param>
        public static void UnlockElement(string elementGUID, List<Player> players, AsyncCallback<bool> completed)
        {
            Avatar.UnlockElement(elementGUID, players, completed);
        }

        /// <summary>
        /// Open Avatar app to configure slots or create new characters
        /// </summary>
        /// <param name="completed">Callback when the Avatar app closes</param>
        public static void Open(AsyncCallback<List<Slot>> completed)
        {
            Open(GameSettings.SlotsConfig, completed);
        }

        /// <summary>
        /// Open Liga dos Heróis
        /// </summary>
        public static void OpenLeague()
        {
            AudioManager.StopGameSounds(1f);
            GameObject wait = GameObject.Find("ControlCenter").GetComponent<ControlCenter>().WaitAvatarToCloseObj;
            wait.SetActive(true);
            // ---
            Avatar.OpenLeague(() =>
            {
                AudioManager.ResumeGameSounds(1f);
                wait.SetActive(false);
            });
        }

        /// <summary>
        /// Open Avatar app to configure slots or create new characters
        /// </summary>
        /// <param name="config">Different slot of config than GameSetting</param>
        /// <param name="completed">Callback when the Avatar app closes</param>
        public static void Open(SlotsConfig config, AsyncCallback<List<Slot>> completed)
        {
            GameObject wait = GameObject.Find("ControlCenter").GetComponent<ControlCenter>().WaitAvatarToCloseObj;
            // ---
            wait.SetActive(true);
            AudioManager.StopGameSounds(1f);
            // ---
            Avatar.Open(config, () =>
            {
                GetSlots(config, true, result =>
                {
                    AudioManager.ResumeGameSounds(1f);
                    wait.SetActive(false);
                    completed?.Invoke(result);
                    OnSlotsMayChanged.Invoke(result);
                });
            });
        }

        /// <summary>
        /// Get all slots that are active in Playtable right now
        /// </summary>
        /// <param name="completed">Callback containing all slots or error</param>
        public static void GetSlots(AsyncCallback<List<Slot>> completed)
        {
            GetSlots(GameSettings.SlotsConfig, false, completed);
        }

        /// <summary>
        /// Get all slots that are active in Playtable right now
        /// </summary>
        /// <param name="force">Used to force a new set of slots</param>
        /// <param name="completed">Callback containing all slots or error</param>
        public static void GetSlots(bool force, AsyncCallback<List<Slot>> completed)
        {
            GetSlots(GameSettings.SlotsConfig, force, completed);
        }

        /// <summary>
        /// Get all slots that are active in Playtable right now
        /// </summary>
        /// <param name="config">Different slot of config than GameSetting</param>
        /// <param name="force">Used to force a new set of slots</param>
        /// <param name="completed">Callback containing all slots or error</param>
        public static void GetSlots(SlotsConfig config, bool force, AsyncCallback<List<Slot>> completed)
        {
            if (_slotsCache != null && _slotsCache.Count > 0 && force == false)
            {
                completed?.Invoke(new AsyncResult<List<Slot>>(_slotsCache, string.Empty));
                return;
            }

            _slotsCache.Clear();

            Slots.GetSlots(config, result =>
            {
                _slotsCache = result.Data;
                if (_slotsCache != null)
                {
                    if (_slotsCache.Where(x => x.Players.Count > 0).Count() > 0 || GameSettings.GUID == Playtable.LeagueGUID)
                        completed?.Invoke(result);
                    else
                    Open((confirmed) =>
                    {
                        completed?.Invoke(result);
                    });
                }
                else
                    completed?.Invoke(result);
            });
        }

        /// <summary>
        /// Get specific slot from specified slot position
        /// </summary>
        /// <param name="slotPosition">Position that the slot are</param>
        /// <param name="completed">Callback containing slot or error</param>
        public static void GetSlot(int slotPosition, AsyncCallback<Slot> completed)
        {
            if (_slotsCache != null && slotPosition < _slotsCache.Count)
            {
                completed?.Invoke(new AsyncResult<Slot>(_slotsCache[slotPosition], string.Empty));
                return;
            }

            Slots.GetSlot(slotPosition, completed);
        }

        /// <summary>
        /// Check if Avatar lock file is created
        /// </summary>
        /// <param name="completed">Callback containing slot or error</param>
        public static IEnumerator CheckAvatarAppClosed(AsyncCallback completed)
        {
            yield return new WaitForSeconds(3f);
            while (IsAvatarLock)
                yield return new WaitForSeconds(0.25f);
            completed?.Invoke();
        }

        /// <summary>
        /// Check if the focus is in the currentGame
        /// </summary>
        /// <param name="completed">Callback containing slot or error</param>
        public static IEnumerator CheckLeagueAppClosed(AsyncCallback completed)
        {
            yield return new WaitForSeconds(3f);
            while (IsLeagueLock)
                yield return new WaitForSeconds(0.25f);
            completed?.Invoke();
        }
    }
}