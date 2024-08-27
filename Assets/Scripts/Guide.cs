using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guide : MonoBehaviour
{
    private int order = 3;
    public Text[] txt;

    private string[] numb = { "123", "4", "3", "2", "1" };
    private string[] lett = { "ABC", "D", "C", "B", "A" };
    private string[] notes = { "CDE", "c" + "\n" + "dó", "G" + "\n" + "Sol", "E" + "\n" + "Mi", "C" + "\n" + "Dó" };
    private string[] empty = { " ", " ", " ", " ", " " };

    public void Next()
    {
        order++;
        if(order>3)
        {
            order = 0;
        }
        switch (order)
        {
            case 0:
                for (int i = 0; i < txt.Length; i++)
                {
                    txt[i].text = numb[i];
                }
                break;
            case 1:
                for (int i = 0; i < txt.Length; i++)
                {
                    txt[i].text = lett[i];
                }
                break;
            case 2:
                for (int i = 0; i < txt.Length; i++)
                {
                    txt[i].text = notes[i];
                }
                break;
            case 3:
                for (int i = 0; i < txt.Length; i++)
                {
                    txt[i].text = empty[i];
                }
                break;
        }
    }
}
