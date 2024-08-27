using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayPiano : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public AudioClip note;
    private AudioSource source;
    private Button btn;
    private bool mouse_over = false;
    private bool mouse_down = false;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        btn = GetComponent<Button>();
    }

    void Update()
    {
        if (mouse_over)
        {
            if (Input.GetMouseButtonDown(0))
            {
                source.PlayOneShot(note);
            }
            if(mouse_down)
            {
                btn.Select();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            mouse_down = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mouse_down = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false;
        if (mouse_down)
        {
            source.PlayOneShot(note);
        }
    }
}