using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayFlute : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioClip note;
    private AudioSource source;
    private Button btn;
    private bool mouse_over = false;
    private bool mouse_down = false;

    public static float t;
    //private float min = 0f;
    //private float max = 1f;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        btn = GetComponent<Button>();
    }

    void Update()
    {
        source.volume = Mathf.Lerp(1, 0, t);

        if (mouse_over)
        {
            if (Input.GetMouseButtonDown(0) && source.volume > 0.2f)
            {
                source.PlayOneShot(note);
            }
            if (mouse_down)
            {
                btn.Select();
            }
        }

        if (mouse_down)
        {
            t += 0.3f * Time.deltaTime;
        }else if (t < 0)
        {
            t -= 0.4f * Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0))
        {
            mouse_down = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mouse_down = false;
            t = 0f;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
        if (mouse_down && source.volume > 0.2f)
        {
            source.PlayOneShot(note);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false;
    }
}