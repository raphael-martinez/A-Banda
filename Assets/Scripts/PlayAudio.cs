using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    public AudioClip note;
    private AudioSource source;


    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayNote()
    {
        //note.Play();
        source.PlayOneShot(note);
    }
}
