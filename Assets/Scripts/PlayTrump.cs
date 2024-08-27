using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayTrump : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioClip note;
    private AudioSource source;
    private bool mouse_over = false;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (mouse_over)
        {
            if (Input.GetMouseButtonDown(0))
            {
                source.PlayOneShot(note);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false;
    }
}