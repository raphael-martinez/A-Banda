using System;
using System.Collections;
using System.Collections.Generic;
using Tayx.Graphy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfilerControler : MonoBehaviour
{
    private GraphyManager graphy;
    private bool profilerActive = false;
    private int mode = 3;
    public bool enableOnStartup;
    public GameObject profilerButton;
    public GameObject modesButton;
    
    void Start()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        //RemoveProfiler();
        GraphySetup();
#else
        RemoveProfiler();
#endif
    }

    public void ProfilerToggle()
    {
        graphy.ToggleActive();

        if (!profilerActive)
        {
            profilerActive = true;
            modesButton.gameObject.SetActive(true);
        }else if (profilerActive)
        {
            profilerActive = false;
            modesButton.gameObject.SetActive(false);
        }
    }

    public void ProfilerToggleModes()
    {
        //quero mimir, vai esse negocio feio mesmo
        if (mode == 3)
        {
            mode = 1;
            graphy.PresetChange(mode);
        }else if (mode == 1)
        {
            mode = 2;
            graphy.PresetChange(mode);
        }else if (mode == 2)
        {
            mode = 3;
            graphy.PresetChange(mode);
        }
    }

    private void GraphySetup()
    {
        graphy = GetComponentInChildren<GraphyManager>();

        if (enableOnStartup)
        {
            graphy.EnableOnStartup = true;
            graphy.ToggleActive();
            profilerActive = true;
        }

        if (profilerActive)
        {
            modesButton.gameObject.SetActive(true);
        }
        else
        {
            modesButton.gameObject.SetActive(false);
        }
    }

    private void RemoveProfiler()
    {
        GameObject.Destroy(profilerButton);
        GameObject.Destroy(this);
        GameObject.Destroy(graphy.gameObject);
    }
}