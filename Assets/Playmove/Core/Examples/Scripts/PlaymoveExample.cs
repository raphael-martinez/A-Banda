using Newtonsoft.Json;
using Playmove.Avatars.API;
using Playmove.Avatars.API.Models;
using Playmove.Core.Audios;
using Playmove.Core.Bundles;
using Playmove.Core.Controls;
using Playmove.Framework;
using Playmove.Framework.Popups;
using Playmove.Metrics.API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Playmove.Core.Examples
{

    public class PlaymoveExample : MonoBehaviour
    {
        public Button startMatchButton;
        public Button leaveMatchButton;
        public Button retryMatchButton;
        public Button AllWinMatchButton;

        public List<Text> slotPlayers;
        public List<Button> startEventButtons;
        public List<Button> endEventButtons;
        public List<Button> cancelEventButtons;
        public List<Button> endMatchButtons;

        public AudioClip sound;

        void Awake()
        {
            ButtonsBehavior(false, false, false, false, false);

            // OnSlotsMayChanged is the event call after the player sets the players inn slots
            AvatarAPI.OnSlotsMayChanged.AddListener(OnGetSlots);
            
            // Initialize ScoreManager
            ScoreManager.Initialize();

            // If theres no players
            if(AvatarAPI.CurrentSlots.Count == 0)
            {
                // Get Slots from DB
                AvatarAPI.GetSlots(true, (result) =>
                {
                    // if there's error Liga dos Heróis will be called
                    if(result.HasError)
                        AvatarAPI.Open(null);
                });
                return;
            }
            
            // Forces OnSlotsMayChanged call
            AvatarAPI.OnSlotsMayChanged.Invoke(new AsyncResult<List<Slot>>(AvatarAPI.CurrentSlots, string.Empty));
        }

        // OnGetSlots is called when OnSlotsMayChanged is Invoked
        private void OnGetSlots(AsyncResult<List<Slot>> result)
        {
            if (result.HasError)
            {
                Debug.LogError("Erro ao pegar usuários, verifique se o SOP Virtual está rodando");
                return;
            }
            // ---
            // Start elements interaction
            // ---
            // Disable all screen elements
            for(var i = 0; i < slotPlayers.Count; i++)
            {
                slotPlayers[i].text = "";
                GameObject.Find("Slot " + i).GetComponent<CanvasGroup>().interactable = false;
            }
            // ---
            // Check slots and add names on each correct slot
            foreach (Slot item in AvatarAPI.CurrentSlots)
            {
                slotPlayers[item.Pos].text = item.Id.ToString()+"\n\n";
                if (item.Players.Count > 0)
                {
                    foreach (var player in item.Players)
                        slotPlayers[item.Pos].text += player.Name + "\n";
                    // ---
                    GameObject.Find("Slot " + item.Pos).GetComponent<CanvasGroup>().interactable = true;
                }
            }
            // ---
            ButtonsBehavior(true, false, false, false, false);
            // ---

            //Debug.Log(AvatarAPI.CurrentSlots.Count);
            //Debug.Log(AvatarAPI.SlotsPlaying.Count);
        }

        /// <summary>
        /// Example function used to Open the Avatar
        /// with a different slot settings than 
        /// GameSettings
        /// </summary>
        public void OnOpenWithNewConfig()
        {
            AvatarAPI.Open(new SlotsConfig(4, 5, 2, true, true), null);
        }

        /// <summary>
        /// Example function used to start a Match. We defined a Match as each time 
        /// a player ou group start a new gameplay that will have a Congratulations 
        /// or Defeat message at the end
        /// </summary>
        public void OnStartMatch()
        {
            MetricsAPI.StartMatch("normal", "fácil", 1);
            // ---
            ButtonsBehavior(false, true, true, false, false);
            // ---
        }

        /// <summary>
        /// Example function used to end a Match when the player
        /// or group leave the current match
        /// </summary>
        public void OnLeaveMatch()
        {
            MetricsAPI.EndMatch(EndReasons.Leave, null);
            // ---
            ButtonsBehavior(true, false, false, false, false);
            // ---
        }

        /// <summary>
        /// Example function used to end and restart a Match when the player
        /// or group retry the current match
        /// </summary>
        public void OnRetryMatch()
        {
            MetricsAPI.EndMatch(EndReasons.Retry, (result) =>
            {
                OnStartMatch();
            });
            // ---
            ButtonsBehavior(true, false, false, false, false);
            // ---
        }

        /// <summary>
        /// Example function used to end a Match when all the players
        /// win the match
        /// </summary>
        public void OnSetAllWinner()
        {
            // ---
            SaveRanking(AvatarAPI.AllPlayers);
            MetricsAPI.EndMatch(EndReasons.Victory, null);
            // ---
            ButtonsBehavior(true, false, false, false, false);
            // ---
        }

        /// <summary>
        /// Example function used to end a Match when one slot
        /// win the match
        /// </summary>
        public void OnSetWinner(int slotPos)
        {
            // ---
            SaveRanking(AvatarAPI.PlayersFromSlot(slotPos));
            MetricsAPI.EndMatch(EndReasons.Victory, AvatarAPI.PlayersIdsFromSlot(slotPos), null);
            // ---
            ButtonsBehavior(true, false, false, false, false);
            // ---
        }

        /// <summary>
        /// Example function used to start an Event to the selected slot
        /// </summary>
        public void OnStartEvent(int slotPos)
        {
            // Inicia um evento
            MetricsAPI.StartEvent(AvatarAPI.PlayersIdsFromSlot(slotPos), "teste");
            // ---
            ButtonsBehavior(false, false, false, true, true, slotPos);
            // ---
        }

        /// <summary>
        /// Example function used to end an Event to the selected Slot
        /// </summary>
        public void OnEndEvent(int slotPos)
        {
            // Finaliza um evento
            MetricsAPI.EndEvent(AvatarAPI.PlayersIdsFromSlot(slotPos), "a"+slotPos, "b" + slotPos);
            // ---
            ButtonsBehavior(false, true, true, false, false, slotPos);
            // ---
        }

        /// <summary>
        /// Example function used to cancel an Event to the selected Slot
        /// </summary>
        public void OnCancelEvent(int slotPos)
        {
            // Cancela um evento
            MetricsAPI.CancelEvent(AvatarAPI.PlayersIdsFromSlot(slotPos));
            // ---
            ButtonsBehavior(false, true, true, false, false, slotPos);
            // ---
        }

        // -------------------
        /// <summary>
        /// Example function used to get the Ranking saved
        /// </summary>
        public void GetRanking()
        {
            ScoreManager.Load(result =>
            {
                Debug.Log(JsonConvert.SerializeObject(ScoreManager.ranking));
            });
        }

        /// <summary>
        /// Example function used to save the Ranking
        /// </summary>
        public void SaveRanking(List<Player> players)
        {
            foreach (var item in players)
                ScoreManager.RegisterStudent(item.GUID, new Student(item.Name, Random.Range(50, 200)), Random.Range(0, ScoreManager.DifficultyLevels));
            // ---
            MetricsAPI.Score.SaveRanking(ScoreManager.ranking, null);
        }

        /// <summary>
        /// Example function used to delete the Ranking saved
        /// We used an Pop-up example as well
        /// </summary>
        public void DeleteRanking()
        {
            ControlBoxSubPopup.CloseIfAny();
            Fader.FadeTo(0.75f, 0.5f);
            Popup.Open(Localization.GetAsset<string>(AssetsCatalog.string_Attention), Localization.GetAsset<string>(AssetsCatalog.string_DesejaMesmoApagarPlacar),
                new PopupButton(Localization.GetAsset<string>(AssetsCatalog.string_Cancel)),
                new PopupButton(Localization.GetAsset<string>(AssetsCatalog.string_Continue)) { Action = _ =>
                    ScoreManager.DeleteAll()
                },
                _ => Fader.FadeTo(0, 0.5f));
        }

        // -------------------
        // just a visual controller
        private void ButtonsBehavior(bool startMatch, bool endMatch, bool startEvent, bool cancelEvent, bool endEvent, int slotPos = -1)
        {
            // ---
            startMatchButton.interactable = startMatch;
            leaveMatchButton.interactable = endMatch;
            retryMatchButton.interactable = endMatch;
            AllWinMatchButton.interactable = endMatch;
            // ---
            if(slotPos == -1)
            {
                foreach (var item in startEventButtons)
                    item.interactable = startEvent;
                // ---
                foreach (var item in endEventButtons)
                    item.interactable = endEvent;
                // ---
                foreach (var item in cancelEventButtons)
                    item.interactable = cancelEvent;
                // ---
                foreach (var item in endMatchButtons)
                    item.interactable = endMatch;
            } else
            {
                var slot = GameObject.Find("Slot " + slotPos);
                slot.transform.Find("Start Event").GetComponent<Button>().interactable = startEvent;
                slot.transform.Find("End Event").GetComponent<Button>().interactable = endEvent;
                slot.transform.Find("Cancel Event").GetComponent<Button>().interactable = cancelEvent;
                slot.transform.Find("Winner").GetComponent<Button>().interactable = startEvent;
            }
        }
    }
}
