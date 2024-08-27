using System.Collections.Generic;
using Playmove.Metrics.API;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Team : MonoBehaviour
{
    public Button startBtn;
    public Button cooldownBtn;
    public GameObject[] hide;
    public GameObject[] show;
    private AudioSource source;
    public AudioClip failSFX;

    [Header("Player A")]
    public Button[] pAbtn;
    public AudioClip[] xylo;
    [Header("Player B")]
    public Button[] pBbtn;
    public AudioClip[] flute;
    [Header("Player C")]
    public Button[] pCbtn;
    public AudioClip[] harp;
    [Header("Player D")]
    public Button[] pDbtn;
    public AudioClip[] trump;
    [Header("Activated Players")]
    public GameObject[] activePlayers;
    private int actP;
    public Canvas[] layerOrder;

    [Header("Game Data")]
    // Keep track of player turn (first turn for random)
    public int playerTurn;
    // List of sequence from game
    public List<int> coopGame;
    // Add new random note
    public int newNote;
    // Keep track of current sequence
    public int currentSequence = 0;
    // Tempo between notes
    public float tempo;
    public int sequence;
    public Text sequenceTxt;
    public Composer access;

    [Header("Points")]
    // Points
    public GameObject pointsPanel;
    public GameObject pointer;
    private int pts;
    public Text pointsTxt;
    private Quaternion targetRot;
    public float smooth;
    public GameObject[] hp;
    private int wrong;
    private Vector3 mousePos;
    public GameObject pointPrefab;
    public GameObject lb;
    private Vector3 offset;

    private void Start()
    {
        MetricsAPI.StartMatch("team", "normal", 99);
        Leaderboard.Instance.SetGameMode(GameMode.Team);

        source = GetComponent<AudioSource>();

        startBtn.onClick.AddListener (StartCD);

        // Player A four keys
        pAbtn[0].onClick.AddListener(delegate { PlayerA(1); });
        pAbtn[1].onClick.AddListener(delegate { PlayerA(2); });
        pAbtn[2].onClick.AddListener(delegate { PlayerA(3); });
        pAbtn[3].onClick.AddListener(delegate { PlayerA(4); });

        // Player B four keys
        pBbtn[0].onClick.AddListener(delegate { PlayerB(1); });
        pBbtn[1].onClick.AddListener(delegate { PlayerB(2); });
        pBbtn[2].onClick.AddListener(delegate { PlayerB(3); });
        pBbtn[3].onClick.AddListener(delegate { PlayerB(4); });

        // Player C four keys
        pCbtn[0].onClick.AddListener(delegate { PlayerC(1); });
        pCbtn[1].onClick.AddListener(delegate { PlayerC(2); });
        pCbtn[2].onClick.AddListener(delegate { PlayerC(3); });
        pCbtn[3].onClick.AddListener(delegate { PlayerC(4); });

        // Player D four keys
        pDbtn[0].onClick.AddListener(delegate { PlayerD(1); });
        pDbtn[1].onClick.AddListener(delegate { PlayerD(2); });
        pDbtn[2].onClick.AddListener(delegate { PlayerD(3); });
        pDbtn[3].onClick.AddListener(delegate { PlayerD(4); });

        offset = new Vector3(Screen.width, Screen.height, 0.0f);
    }

    void Update ()
    {
        // Enable Play Btn
        if(activePlayers[0].activeSelf || activePlayers[1].activeSelf || activePlayers[2].activeSelf || activePlayers[3].activeSelf)
        {
            startBtn.interactable = true;
        }
        else
        {
            startBtn.interactable = false;
        }

        //Spin Pointer
        pointer.transform.rotation = Quaternion.Slerp(pointer.transform.rotation, targetRot, Time.deltaTime * smooth);
    }

    private void StartCD ()
    {
        Invoke(nameof(StartGame), 3.0f);
    }

    void StartGame ()
    {
        if(cooldownBtn.gameObject.activeSelf)
        {
            print("Go");
            foreach(GameObject x in hide) // Turn off setup UI
            {
                x.SetActive(false);
            }
            cooldownBtn.gameObject.SetActive(false);
            // How many players
            if(activePlayers[0].activeSelf)
            {
                actP++;
                show[0].SetActive(true);
            }
            if (activePlayers[1].activeSelf)
            {
                actP++;
                show[1].SetActive(true);
            }
            if (activePlayers[2].activeSelf)
            {
                actP++;
                show[2].SetActive(true);
            }
            if (activePlayers[3].activeSelf)
            {
                actP++;
                show[3].SetActive(true);
            }
            // Start new round
            NewRound();
        }
    }

    void PlayerA(int i)
    {
        if (playerTurn == 1 || coopGame.Count == 0)
        {
            // Player turn or Selection screen = Audio Enabled
            source.PlayOneShot(xylo[i-1]);
            if (currentSequence < coopGame.Count)
            {
                if (coopGame[currentSequence] == i)
                {
                    //print("A right" + currentSequence);
                    mousePos = Input.mousePosition;
                    mousePos -= offset/2;
                    SpawnPoints();
                    currentSequence++;
                    if (currentSequence == coopGame.Count)
                    {
                        PassTurn();
                    }
                }
                else
                {
                    //print("A wrong" + currentSequence);
                    source.PlayOneShot(failSFX);
                    hp[0].SetActive(false);
                    wrong++;
                    PassTurn();
                }
            }
        }
    }

    void PlayerB(int i)
    {
        if (playerTurn == 2 || coopGame.Count == 0)
        {
            source.PlayOneShot(flute[i-1]);
            if (currentSequence < coopGame.Count)
            {
                if (coopGame[currentSequence] == i)
                {
                    //print("B right" + currentSequence);
                    mousePos = Input.mousePosition;
                    mousePos -= offset/2;
                    SpawnPoints();
                    currentSequence++;
                    if (currentSequence == coopGame.Count)
                    {
                        PassTurn();
                    }
                }
                else
                {
                    //print("B wrong" + currentSequence);
                    source.PlayOneShot(failSFX);
                    hp[1].SetActive(false);
                    wrong++;
                    PassTurn();
                }
            }
        }
    }

    void PlayerC(int i)
    {
        if (playerTurn == 3 || coopGame.Count == 0)
        {
            source.PlayOneShot(harp[i-1]);
            if (currentSequence < coopGame.Count)
            {
                if (coopGame[currentSequence] == i)
                {
                    //print("C right" + currentSequence);
                    mousePos = Input.mousePosition;
                    mousePos -= offset/2;
                    SpawnPoints();
                    currentSequence++;
                    if (currentSequence == coopGame.Count)
                    {
                        PassTurn();
                    }
                }
                else
                {
                    //print("C wrong" + currentSequence);
                    source.PlayOneShot(failSFX);
                    hp[2].SetActive(false);
                    wrong++;
                    PassTurn();
                }
            }
        }
    }

    void PlayerD(int i)
    {
        if (playerTurn == 4 || coopGame.Count == 0)
        {
            source.PlayOneShot(trump[i-1]);
            if (currentSequence < coopGame.Count)
            {
                if (coopGame[currentSequence] == i)
                {
                    //print("D right" + currentSequence);
                    mousePos = Input.mousePosition;
                    mousePos -= offset/2;
                    SpawnPoints();
                    currentSequence++;
                    if (currentSequence == coopGame.Count)
                    {
                        PassTurn();
                    }
                }
                else
                {
                    //print("D wrong" + currentSequence);
                    source.PlayOneShot(failSFX);
                    hp[3].SetActive(false);
                    wrong++;
                    PassTurn();
                }
            }
        }
    }

    void SpawnPoints ()
    {
        for(int i = 0; i < currentSequence+1; i++)
        {
            GameObject pt = Instantiate(pointPrefab, mousePos, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)), GameObject.FindGameObjectWithTag("Canvas").transform);
            pt.GetComponent<Rigidbody2D>().velocity = pt.transform.up * 300f;
            if(coopGame[currentSequence] == 1) // Red
            {
                pt.GetComponent<Image>().color = new Color32(232, 68, 55, 100);
            }
            if (coopGame[currentSequence] == 2) // Yellow
            {
                pt.GetComponent<Image>().color = new Color32(236, 203, 26, 100);
            }
            if (coopGame[currentSequence] == 3) // Green
            {
                pt.GetComponent<Image>().color = new Color32(40, 175, 29, 100);
            }
            if (coopGame[currentSequence] == 4) // Blue
            {
                pt.GetComponent<Image>().color = new Color32(23, 135, 240, 100);
            }
        }
    }

    public void AddPoints ()
    {
        pts++;
        pointsTxt.text = pts.ToString();
    }

    void PassTurn ()
    {
        // Send all to back
        for (int i = 0; i < 4; i++)
        {
            layerOrder[i].transform.position = Vector3.forward;
        }
        /*
        if (playerTurn>0)
        {
            layerOrder[playerTurn-1].transform.localPosition = backLayer; //Send last player back
        }*/
        playerTurn++;
        if(playerTurn == 1 && !activePlayers[0].activeSelf) //Check if there is player A (Xylo)
        {
            playerTurn++;
        }
        if (playerTurn == 2 && !activePlayers[1].activeSelf) //Check if there is player B (Flute)
        {
            playerTurn++;
        }
        if (playerTurn == 3 && !activePlayers[2].activeSelf) //Check if there is player C (Harp)
        {
            playerTurn++;
        }
        if (playerTurn == 4 && !activePlayers[3].activeSelf) //Check if there is player D (Trumpet)
        {
            playerTurn++;
        }
        currentSequence = 0;
        if(playerTurn > 4)
        {
            Invoke(nameof(NewRound), 2.0f);
        }
        if (playerTurn > 0 && playerTurn < 5)
        {
            layerOrder[playerTurn - 1].transform.position = Vector3.back; //Send current player front
            targetRot = Quaternion.Euler(0, 0, -90f * (playerTurn-1));
        }
    }

    void NewRound ()
    {
        // If n of active players equal n of wrong notes = GAME OVER
        if(actP == wrong)
        {
            print("GAME OVER");

            Leaderboard.Instance.SetTeamScore(pts);
            Leaderboard.Instance.OnSetAllWinner();

            pointsPanel.SetActive(false);
            lb.SetActive(true);
            //Send to Scoreboard Scene
        }
        else
        {
            wrong = 0;
            for (int i = 0; i < 4; i++)
            {
                hp[i].SetActive(true);
            }
            playerTurn = 0;
            pointsPanel.SetActive(false);
            // New random note
            newNote = Random.Range(1, 5);
            coopGame.Add(newNote);
            //print(newNote);
            StartCoroutine (TempoPlay(coopGame.Count));
        }
    }

    IEnumerator TempoPlay (int size)
    {
        while (size > 0)
        {
            // Call function from Composer
            access.PlayMusic(coopGame[sequence]);
            sequence++;
            sequenceTxt.text = sequence.ToString();
            size--;
            yield return new WaitForSeconds(tempo);
        }
        access.PlayMusic(0);
        sequence = 0;
        PassTurn();
        pointsPanel.SetActive(true);
    }
}
