using Playmove.Avatars.API.Models;
using System.Collections.Generic;
using Playmove.Core.Examples;
using Playmove.Avatars.API;
using Playmove.Metrics.API;
using Playmove.Core;
using UnityEngine;

public enum GameMode
{
    Versus = 0,
    Team = 1,
    Free = 2,
    None
}

public class Rank
{
    public int Score { get; private set; } = 0;

    public readonly Player[] Alunos = new Player[4]{ null, null, null, null };

    public void SetAluno(int pos, Player player) => Alunos[pos] = player;
    public void SetScore(int score) => Score = score;
}

public class SavedRank
{
    public int Score = 0;
    public readonly string[] Alunos = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };

    public SavedRank() {}
    public SavedRank (Rank rank)
    {
        Score = rank.Score;
        for(int i = 0; i < rank.Alunos.Length; i++)
        {
            Alunos[i] = rank.Alunos[i].Name;
        }
    }

    public override bool Equals(object obj)
    {
        Rank rank = obj as Rank;

        if(rank.Score != Score)
        {
            return false;
        }

        for(int i = 0; i < Alunos.Length; i++)
        {
            if (Alunos[i] != rank.Alunos[i].Name)
            {
                return false;
            }
        }

        return true;
    }
}

[DisallowMultipleComponent]
public class Leaderboard : MonoBehaviour
{
    private readonly SavedRank[] _ranks = new SavedRank[4] { new(), new(), new(), new() };
    private readonly Rank _rank = new();

    public static Leaderboard Instance { get; private set; } = null;
    public GameMode GameMode { get; private set; } = GameMode.Free;

    private void Awake()
    {
        if(Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        ScoreManager.Initialize();
        DontDestroyOnLoad(gameObject);

        LoadRanks();

        // OnSlotsMayChanged is the event call after the player sets the players inn slots
        AvatarAPI.OnSlotsMayChanged.AddListener(OnGetSlots);

        // If theres no players
        if (AvatarAPI.CurrentSlots.Count == 0)
        {
            // Get Slots from DB
            AvatarAPI.GetSlots(true, (result) =>
            {
                // if there's error Liga dos Heróis will be called
                if (result.HasError)
                    AvatarAPI.Open(null);
            });
            return;
        }

        // Forces OnSlotsMayChanged call
        AvatarAPI.OnSlotsMayChanged.Invoke(new AsyncResult<List<Slot>>(AvatarAPI.CurrentSlots, string.Empty));
    }

    private void OnGetSlots(AsyncResult<List<Slot>> result)
    {
        if (result.HasError)
        {
            Debug.LogError("Erro ao pegar usuários, verifique se o SOP Virtual está rodando");
            return;
        }

        foreach (Slot item in AvatarAPI.CurrentSlots)
        {
            _rank.SetAluno(item.Pos, item.Players[0]);
        }
    }

    public void OnSetWinner(int slotPos, int score)
    {
        Player aluno = _rank.Alunos[slotPos];
        ScoreManager.RegisterStudent(aluno.GUID, new Student(aluno.Name, score), (int)GameMode);
        MetricsAPI.Score.SaveRanking(ScoreManager.ranking, null);

        MetricsAPI.EndMatch(EndReasons.Victory, AvatarAPI.PlayersIdsFromSlot(slotPos), null);
    }

    public void OnSetAllWinner()
    {
        SaveRanking();
        MetricsAPI.EndMatch(EndReasons.Victory, null);
    }

    private void SaveRanking()
    {
        foreach (Player item in _rank.Alunos)
        {
            ScoreManager.RegisterStudent(item.GUID, new Student(item.Name, _rank.Score), (int)GameMode);
        }

        // ---
        MetricsAPI.Score.SaveRanking(ScoreManager.ranking, null);
    }

    private void LoadRanks()
    {
        string data;
        for(int i = 0; i < _ranks.Length; i++)
        {
            data = PlayerPrefs.GetString(string.Concat("rank_", i.ToString()), string.Empty);

            if (!string.IsNullOrEmpty(data))
            {
                _ranks[i] = JsonUtility.FromJson<SavedRank>(data);
            }
        }
    }

    private void SaveRanks()
    {
        string data;
        for (int i = 0; i < _ranks.Length; i++)
        {
            data = JsonUtility.ToJson(_ranks[i]);
            PlayerPrefs.SetString(string.Concat("rank_", i.ToString()), data);
        }
    }

    private void UpdateRank()
    {
        int index;
        for(int i = 0; i < _ranks.Length; i++)
        {
            if(_rank.Score >= _ranks[i].Score)
            {
                for(int j = 3; j >= i; j--)
                {
                    index = j+1;
                    if (index >= _ranks.Length)
                    {
                        continue;
                    }

                    _ranks[index] = _ranks[i];
                }

                _ranks[i] = new SavedRank(_rank);
            }
        }

        SaveRanks();
    }

    public void SetTeamScore(int score)
    {
        _rank.SetScore(score);
        UpdateRank();
    }

    public void SetGameMode(GameMode mode) => GameMode = mode;
    public SavedRank[] GetRanks() => _ranks;
    public int GetRankPos()
    {
        for (int i = 0; i < _ranks.Length; i++)
        {
            if (_ranks[i].Equals(_rank))
            {
                return i;
            }
        }

        return -1;
    }
}
