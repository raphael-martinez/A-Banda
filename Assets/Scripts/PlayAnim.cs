using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnim : MonoBehaviour
{
    private Animator anim;
    public GameObject logoAnim;

    void Start ()
    {
        anim = logoAnim.GetComponent<Animator>();
    }

    public void PlayAnimation ()
    {
        anim.Play("Logo", 0, 0f);
    }
}
