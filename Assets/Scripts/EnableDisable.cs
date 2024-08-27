using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisable : MonoBehaviour
{
    public GameObject[] show;
    public GameObject[] hide;

    public void ShowHide ()
    {
        foreach(GameObject s in show)
        {
            s.SetActive (true);
        }
        foreach (GameObject h in hide)
        {
            h.SetActive(false);
        }
        //show.SetActive(true);
        //hide.SetActive (false);
    }
}
