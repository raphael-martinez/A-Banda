using System;

namespace Playmove.Metrics.API.Models
{
    /// <summary>
    /// Responsible to hold data for the player progress
    /// </summary>
    [Serializable]
    public class Progress
    {
        public float Level { get; set; }
        public float Score { get; set; }
    }
}
