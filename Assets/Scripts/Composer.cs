using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Composer : MonoBehaviour
{
    public GameObject[] newNotes;
    public AudioClip[] piano;
    private AudioSource source;
    public GameObject[] red;
    public GameObject[] yellow;
    public GameObject[] green;
    public GameObject[] blue;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayMusic (int note)
    {
        if (note == 0)
        {
            newNotes[0].SetActive(false);
        }
        else
        {
            newNotes[0].SetActive(true);

            newNotes[1].SetActive(false);
            newNotes[2].SetActive(false);
            newNotes[3].SetActive(false);
            newNotes[4].SetActive(false);

            source.PlayOneShot(piano[note-1]);
            newNotes[note].SetActive(true);
        }

        //Red
        if (note == 1)
        {
            red[0].SetActive(true);
            red[1].SetActive(true);
            red[2].SetActive(true);
            red[3].SetActive(true);

            Invoke("ClearColors", 1.0f);
        }

        //Yellow
        if (note == 2)
        {
            yellow[0].SetActive(true);
            yellow[1].SetActive(true);
            yellow[2].SetActive(true);
            yellow[3].SetActive(true);

            Invoke("ClearColors", 1.0f);
        }

        //Green
        if (note == 3)
        {
            green[0].SetActive(true);
            green[1].SetActive(true);
            green[2].SetActive(true);
            green[3].SetActive(true);

            Invoke("ClearColors", 1.0f);
        }

        //Blue
        if (note == 4)
        {
            blue[0].SetActive(true);
            blue[1].SetActive(true);
            blue[2].SetActive(true);
            blue[3].SetActive(true);

            Invoke("ClearColors", 1.0f);
        }
    }

    public void ClearColors ()
    {
        red[0].SetActive(false);
        red[1].SetActive(false);
        red[2].SetActive(false);
        red[3].SetActive(false);

        yellow[0].SetActive(false);
        yellow[1].SetActive(false);
        yellow[2].SetActive(false);
        yellow[3].SetActive(false);

        green[0].SetActive(false);
        green[1].SetActive(false);
        green[2].SetActive(false);
        green[3].SetActive(false);

        blue[0].SetActive(false);
        blue[1].SetActive(false);
        blue[2].SetActive(false);
        blue[3].SetActive(false);
    }
}
