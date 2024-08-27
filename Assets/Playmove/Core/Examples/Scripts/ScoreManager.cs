using Newtonsoft.Json;
using Playmove.Avatars.API;
using Playmove.Metrics.API;
using Playmove.Metrics.API.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Student
{
    public int Score;
    public string Name;
    public Student()
    {
        Name = "";
        Score = 0;
    }

    public Student(string name, int score)
    {
        Name = name;
        Score = score;
    }
}

namespace Playmove.Metrics.API.Models
{
    public partial class Score
    {
        public Dictionary<int, Student> ScoresByGameMode = new Dictionary<int, Student>();

        public bool ContainsScoresInGameMode(int gameMode)
        {
            return ScoresByGameMode.ContainsKey(gameMode);
        }

        public Student GetStudentScoreByGameMode(int gameMode)
        {
            if (!ScoresByGameMode.ContainsKey(gameMode))
            {
                return null;
            }
            else
            {
                return ScoresByGameMode[gameMode];
            }
        }

        public void SetScore(Student student, int gameMode)
        {
            if (!ScoresByGameMode.ContainsKey(gameMode))
            {
                ScoresByGameMode.Add(gameMode, student);
            }

            if (student.Score > ScoresByGameMode[gameMode].Score)
            {
                ScoresByGameMode[gameMode].Score = student.Score;
            }

            GlobalScore = ScoresByGameMode.Sum(item => item.Value.Score);
        }
    }

    // Utiliza a base da classe ranking e adiciona itens específicos
    public partial class Ranking
    {
        public List<Student> GetStudentsByGameMode(int gameMode)
        {
            List<Student> students = ScoreInfo.Where(item => item.Value.ContainsScoresInGameMode(gameMode))
            .Select(item => item.Value.GetStudentScoreByGameMode(gameMode)).ToList();
            students = students.OrderByDescending(item => item.Score).ToList();
            return students;
        }

        public Student GetStudentByGameMode(int gameMode, string playerGUID)
        {
            if (!ScoreInfo.ContainsKey(playerGUID) && ScoreInfo[playerGUID].ContainsScoresInGameMode(gameMode))
                return ScoreInfo[playerGUID].GetStudentScoreByGameMode(gameMode);
            else
                return null;
        }

        public void SetScore(string playerGUID, Student studen, int gameMode)
        {
            if (!ScoreInfo.ContainsKey(playerGUID))
                ScoreInfo.Add(playerGUID, new Score());
            // ---
            ScoreInfo[playerGUID].SetScore(studen, gameMode);
        }
    }
}

namespace Playmove.Core.Examples
{
    public class ScoreManager : MonoBehaviour
    {
        public static int MaxScoresNumber = 50;
        public static int DifficultyLevels = 3;
        public static Ranking ranking;
        private static List<Student>[] students;

        private static int _difficulty;
        public static int Difficulty
        {
            get
            {
                return _difficulty;
            }
            set
            {
                _difficulty = value;
            }
        }

        private static int _score;
        public static int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
            }
        }

        // Inicializa a classe
        public static void Initialize()
        {
            if (ranking == null)
                ranking = new Ranking();
            if (students != null)
                return;

            students = new List<Student>[DifficultyLevels];
            for (int i = 0; i < students.Length; i++)
            {
                students[i] = new List<Student>();
            }

            Difficulty = 0;
            Score = 0;
            Load(null);
        }

        // Carrega os dados, se esxistirem, do banco de dados;
        public static void Load(AsyncCallback<bool> completed)
        {
            MetricsAPI.Score.GetRanking((result) =>
            {
                ranking = result.Data;
                completed?.Invoke(new AsyncResult<bool>(true, string.Empty));
            });
        }

        public static void Save()
        {
            MetricsAPI.Score.SaveRanking(ranking, null);
        }

        public static void DeleteAll()
        {
            MetricsAPI.Score.DeleteRanking(ranking,
                (result) =>
                {
                    Debug.Log("Ranking deletado: " + result.Data);
                }
            );
        }

        public static void RegisterStudent(string playerGUID, Student student, int difficulty)
        {
            ranking.SetScore(playerGUID, student, difficulty);
        }
    }
}
