using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnfocusedFPS : MonoBehaviour
{
    //max fps, 0 = unlimited (will pin the gpu at 100% all the time and may cause screen tearing on games that go over 60 fps)
    public int focusedFps = 30;
    
    // unfocused fps
    public int unfocusedFps = 1;
    private void Awake()
    {
        QualitySettings.vSyncCount = 0; // atificially limiting the fps target requires vsync to be off.
    }
    
    private void OnApplicationFocus(bool hasFocus) // gets triggered when the applications gains/loses focus.
    {
#if !UNITY_EDITOR
        if (hasFocus)
        {
            OnFocus();
        }
        else if (!hasFocus)
        {
            OnUnfocus();
        }
#endif
    }

    private void OnUnfocus()
    {
        Application.targetFrameRate = unfocusedFps; // sets the targetfps (the limit).
    }

    private void OnFocus()
    {
        Application.targetFrameRate = focusedFps; // sets the targetfps (the limit).
    }
}