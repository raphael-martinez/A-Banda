using System.Collections.Generic;
using Random=UnityEngine.Random;
using Playmove.Metrics.API;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class Versus : MonoBehaviour
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
    public List<int> compGame;
    // Add new random note
    public int newNote;
    // Keep track of current sequence
    public int currentSequence = 0;
    // Tempo between notes
    public float tempo;
    public int sequence;
    public Text sequenceTxt;
    public Composer access;

    [Header("Health")]
    // HP
    public GameObject[] hpA;
    public GameObject[] hpB;
    public GameObject[] hpC;
    public GameObject[] hpD;
    private int a;
    private int b;
    private int c;
    private int d;
    public int ptsA;
    public int ptsB;
    public int ptsC;
    public int ptsD;
    public int[] victoryOrder = new int[4];
    public GameObject[] medals;

    [Header("Spin")]
    // Points
    public GameObject spinPanel;
    public GameObject pointer;
    private Quaternion targetRot;
    public float smooth;
    private int wrong;

    void Start()
    {
        MetricsAPI.StartMatch("versus", "normal", 99);
        Leaderboard.Instance.SetGameMode(GameMode.Versus);

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

    void StartCD ()
    {
        Invoke("StartGame", 3.0f);
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
                a = 3;
                show[0].SetActive(true);
            }
            if (activePlayers[1].activeSelf)
            {
                actP++;
                b = 3;
                show[1].SetActive(true);
            }
            if (activePlayers[2].activeSelf)
            {
                actP++;
                c = 3;
                show[2].SetActive(true);
            }
            if (activePlayers[3].activeSelf)
            {
                actP++;
                d = 3;
                show[3].SetActive(true);
            }
            // Start new round
            NewRound();
        }
    }

    void PlayerA(int i)
    {
        if (playerTurn == 1 || compGame.Count == 0)
        {
            // Player turn or Selection screen = Audio Enabled
            source.PlayOneShot(xylo[i - 1]);
            if (currentSequence < compGame.Count)
            {
                if (compGame[currentSequence] == i)
                {
                    //print("A right" + currentSequence);
                    ptsA += currentSequence+1;
                    currentSequence++;
                    if (currentSequence == compGame.Count)
                    {
                        PassTurn();
                    }
                }
                else
                {
                    //print("A wrong" + currentSequence);
                    //hp[0].SetActive(false);
                    //wrong++;
                    source.PlayOneShot(failSFX);
                    a--;
                    switch (a)
                    {
                        case 2:
                            hpA[2].SetActive(false);
                            break;
                        case 1:
                            hpA[1].SetActive(false);
                            break;
                        case 0:
                            hpA[0].SetActive(false);
                            break;
                    }
                    wrong++;
                    PassTurn();
                }
            }
        }
    }

    void PlayerB(int i)
    {
        if (playerTurn == 2 || compGame.Count == 0)
        {
            // Player turn or Selection screen = Audio Enabled
            source.PlayOneShot(flute[i - 1]);
            if (currentSequence < compGame.Count)
            {
                if (compGame[currentSequence] == i)
                {
                    //print("B right" + currentSequence);
                    ptsB += currentSequence + 1;
                    currentSequence++;
                    if (currentSequence == compGame.Count)
                    {
                        PassTurn();
                    }
                }
                else
                {
                    //print("B wrong" + currentSequence);
                    //hp[1].SetActive(false);
                    //wrong++;
                    source.PlayOneShot(failSFX);
                    b--;
                    switch (b)
                    {
                        case 2:
                            hpB[2].SetActive(false);
                            break;
                        case 1:
                            hpB[1].SetActive(false);
                            break;
                        case 0:
                            hpB[0].SetActive(false);
                            break;
                    }
                    wrong++;
                    PassTurn();
                }
            }
        }
    }

    void PlayerC(int i)
    {
        if (playerTurn == 3 || compGame.Count == 0)
        {
            // Player turn or Selection screen = Audio Enabled
            source.PlayOneShot(harp[i - 1]);
            if (currentSequence < compGame.Count)
            {
                if (compGame[currentSequence] == i)
                {
                    //print("C right" + currentSequence);
                    ptsC += currentSequence + 1;
                    currentSequence++;
                    if (currentSequence == compGame.Count)
                    {
                        PassTurn();
                    }
                }
                else
                {
                    //print("C wrong" + currentSequence);
                    //hp[2].SetActive(false);
                    //wrong++;
                    source.PlayOneShot(failSFX);
                    c--;
                    switch (c)
                    {
                        case 2:
                            hpC[2].SetActive(false);
                            break;
                        case 1:
                            hpC[1].SetActive(false);
                            break;
                        case 0:
                            hpC[0].SetActive(false);
                            break;
                    }
                    wrong++;
                    PassTurn();
                }
            }
        }
    }

    void PlayerD(int i)
    {
        if (playerTurn == 4 || compGame.Count == 0)
        {
            // Player turn or Selection screen = Audio Enabled
            source.PlayOneShot(trump[i - 1]);
            if (currentSequence < compGame.Count)
            {
                if (compGame[currentSequence] == i)
                {
                    //print("D right" + currentSequence);
                    ptsD += currentSequence + 1;
                    currentSequence++;
                    if (currentSequence == compGame.Count)
                    {
                        PassTurn();
                    }
                }
                else
                {
                    //print("D wrong" + currentSequence);
                    //hp[3].SetActive(false);
                    //wrong++;
                    source.PlayOneShot(failSFX);
                    d--;
                    switch (d)
                    {
                        case 2:
                            hpD[2].SetActive(false);
                            break;
                        case 1:
                            hpD[1].SetActive(false);
                            break;
                        case 0:
                            hpD[0].SetActive(false);
                            break;
                    }
                    wrong++;
                    PassTurn();
                }
            }
        }
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
        if(playerTurn == 1 && a == 0) //Check if there is player A (Xylo)
        {
            playerTurn++;
        }
        if (playerTurn == 2 && b == 0) //Check if there is player B (Flute)
        {
            playerTurn++;
        }
        if (playerTurn == 3 && c == 0) //Check if there is player C (Harp)
        {
            playerTurn++;
        }
        if (playerTurn == 4 && d == 0) //Check if there is player D (Trumpet)
        {
            playerTurn++;
        }
        currentSequence = 0;
        if(playerTurn > 4)
        {
            Invoke("NewRound", 2.0f);
        }
        if (playerTurn > 0 && playerTurn < 5)
        {
            layerOrder[playerTurn - 1].transform.position = Vector3.back; //Send current player front
            targetRot = Quaternion.Euler(0, 0, -90f * (playerTurn - 1));
        }
    }

    void NewRound ()
    {
        // If n of active players equal n of wrong notes = GAME OVER
        if((a+b+c+d) == 0)
        {
            spinPanel.SetActive(false);
            //print("GAME OVER");
            victoryOrder[0] = ptsA;
            victoryOrder[1] = ptsB;
            victoryOrder[2] = ptsC;
            victoryOrder[3] = ptsD;
            
            Array.Sort(victoryOrder);

            int post;
            int score = 0;
            for(int i = 0; i < victoryOrder.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        score = ptsA;
                        break;

                    case 1:
                        score = ptsB;
                        break;

                    case 2:
                        score = ptsC;
                        break;

                    case 3:
                        score = ptsD;
                        break;
                }

                post = RankByIndex(i);
                Leaderboard.Instance.OnSetWinner(post, score);
            }

            StartCoroutine(Award(3));
        }
        else
        {
            playerTurn = 0;
            spinPanel.SetActive(false);
            if (actP > wrong)   // Dont add new note if everyone got wrong
            {
                // New random note
                newNote = Random.Range(1, 5);
                compGame.Add(newNote);
            }
            wrong = 0;
            //print(newNote);
            StartCoroutine (TempoPlay(compGame.Count));
        }
    }

    private int RankByIndex(int index)
    {
        int valor = 0;

        switch(index)
        {
            case 0:
                valor = ptsA;
                break;

            case 1:
                valor = ptsB;
                break;

            case 2:
                valor = ptsC;
                break;

            case 3:
                valor = ptsD;
                break;
        }

        for (int i = 0; i < victoryOrder.Length; i++)
        {
            if (victoryOrder[i] == valor)
            {
                return i;
            }
        }

        return -1;
    }

    IEnumerator Award (int n)
    {
        int score;
        bool breakIt;
        while (n > 0)
        {
            breakIt = true;
            score = victoryOrder[4 - n];

            if (score > 0)
            {
                if (score == ptsA)
                {
                    breakIt = false;
                    medals[n - 1].SetActive(true);
                }
                else if (score == ptsB)
                {
                    breakIt = false;
                    medals[n + 2].SetActive(true);
                }
                else if (score == ptsC)
                {
                    breakIt = false;
                    medals[n + 5].SetActive(true);
                }
                else if (score == ptsD)
                {
                    breakIt = false;
                    medals[n + 8].SetActive(true);
                }
            }

            if(breakIt)
            {
                break;
            }

            n--;
            yield return new WaitForSeconds(tempo);
        }

        // Enable exit button
        show[4].SetActive(true);
    }

    IEnumerator TempoPlay (int size)
    {
        while (size > 0)
        {
            // Call function from Composer
            access.PlayMusic(compGame[sequence]);
            sequence++;
            sequenceTxt.text = sequence.ToString();
            size--;
            yield return new WaitForSeconds(tempo);
        }
        access.PlayMusic(0);
        sequence = 0;
        PassTurn();
        spinPanel.SetActive(true);
    }
}
