using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayParticles : MonoBehaviour
{

    public ParticleSystem[] ps;

    public void PlayPS()
    {
        ps[0].Play();
        ps[1].Play();
    }
}
