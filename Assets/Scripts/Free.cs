using Playmove.Metrics.API;
using UnityEngine;

public class Free : MonoBehaviour
{
    private void Start()
    {
        MetricsAPI.StartMatch("free", "normal", 99);
        Leaderboard.Instance.SetGameMode(GameMode.Free);
    }
}
