using UnityEngine.UI;
using UnityEngine;
using System;

public class Leaderboards : MonoBehaviour
{
    [SerializeField]
    private RectTransform _select = null;
    [SerializeField]
    private Vector2[] _positions = null;

    [Space]

    [SerializeField]
    private Text[] _firstNames = null;
    [SerializeField]
    private Text[] _secondNames = null;
    [SerializeField]
    private Text[] _thirdNames = null;
    [SerializeField]
    private Text[] _lastNames = null;

    [SerializeField]
    private Text[] _ranks = null;
    
    private void Start()
    {
        SetTeamResult();
    }

    private void SetTeamResult()
    {
        SavedRank[] ranks = Leaderboard.Instance.GetRanks();

        Text[] names;
        for(int i = 0; i < ranks.Length; i++)
        {
            names = GetListByIndex(i);

            Debug.Log(i);
            Debug.Log(ranks[i]);
            _ranks[i].text = ranks[i].Score.ToString();

            for(int j = 0; j < names.Length; j++)
            {
                names[j].text = ranks[i].Alunos[j];
            }
        }
    }

    private Text[] GetListByIndex(int index)
    {
        return index switch
        {
            0 => _firstNames,
            1 => _secondNames,
            2 => _thirdNames,
            3 => _lastNames,
            _ => null,
        };
    }
}
