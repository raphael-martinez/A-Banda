using Playmove.Core.API.Models;
using Playmove.Metrics.API.Vms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Playmove.Metrics.API.Models
{
    /// <summary>
    /// Responsible to hold data for each event that the game match needs
    /// </summary>
    [Serializable]
    public class StageEvent : VmItem<EventoVm>, IDatabaseItem
    {
        public new int Id { get; set; }
        public List<long> Players { get; set; } = new List<long>();
        public string Question { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RightAnswer { get; set; }
        public string PlayerAnswer { get; set; }

        /// <summary>
        /// Indicates where this event is completed or not
        /// </summary>
        public bool IsComplete { get; private set; }

        /// <summary>
        /// Completes the event passing the rightAnswer and the playerAnswer
        /// </summary>
        /// <param name="rightAnswer">Right answer for this Event/Question</param>
        /// <param name="playerAnswer">Player answer for this Event/Question</param>
        public void End(string rightAnswer, string playerAnswer)
        {
            EndDate = DateTime.Now;
            RightAnswer = rightAnswer;
            PlayerAnswer = playerAnswer;
            IsComplete = true;
        }

        public StageEvent(){}

        public StageEvent(EventoVm vm)
        {
            SetDataFromVm(vm);
        }

        public override EventoVm GetVm()
        {
            return new EventoVm()
            {
                Question = Question,
                StartDate = StartDate,
                EndDate = EndDate,
                RightAnswer = RightAnswer,
                PlayerAnswer = PlayerAnswer,
                Players = Players
            };
        }

        public override void SetDataFromVm(EventoVm vm)
        {
            Question = vm.Question;
            StartDate = vm.StartDate;
            EndDate = vm.EndDate;
            RightAnswer = vm.RightAnswer;
            PlayerAnswer = vm.PlayerAnswer;
            Players = vm.Players;
        }
    }
}
