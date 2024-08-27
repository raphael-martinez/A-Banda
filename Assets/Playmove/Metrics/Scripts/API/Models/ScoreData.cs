using System;

namespace Playmove.Metrics.API.Models
{
    /// <summary>
    /// Responsible to save all Score properties for a Player
    /// </summary>
    [Serializable]
    public partial class Score
    {
        public float GlobalScore { get; set; }

        public Score() { }
    }
}
