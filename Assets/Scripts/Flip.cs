using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Flip : MonoBehaviour
{
    public Button flipBtn;
    public bool fliped;
    Quaternion targetRot = Quaternion.Euler(0, 0, 180f);
    public GameObject pA;
    public GameObject pB;
    public GameObject hpA;
    public GameObject hpB;
    Vector3 targetLeftPos = new Vector3(415f, 130f, 0f);
    Vector3 targetRightPos = new Vector3(-415f, 130f, 0f);
    public GameObject xA;
    public GameObject xB;
    public GameObject yA;
    public GameObject yB;
    float smooth = 5f;

    // Start is called before the first frame update
    void Start()
    {
        flipBtn.onClick.AddListener(StartFlipping);
    }

    // Update is called once per frame
    void Update()
    {
        if(fliped)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smooth);
            pA.transform.rotation = Quaternion.Slerp(pA.transform.rotation, targetRot, Time.deltaTime * smooth);
            pB.transform.rotation = Quaternion.Slerp(pB.transform.rotation, targetRot, Time.deltaTime * smooth);
            if (hpA != null && hpB != null)
            {
                hpA.transform.rotation = Quaternion.Slerp(hpA.transform.rotation, targetRot, Time.deltaTime * smooth);
                hpB.transform.rotation = Quaternion.Slerp(hpB.transform.rotation, targetRot, Time.deltaTime * smooth);
            }
            xA.transform.localPosition = Vector3.Slerp(xA.transform.localPosition, targetRightPos, Time.deltaTime * smooth);
            xB.transform.localPosition = Vector3.Slerp(xB.transform.localPosition, targetLeftPos, Time.deltaTime * smooth);
            if (yA != null && yB != null)
            {
                yA.transform.localPosition = Vector3.Slerp(yA.transform.localPosition, targetLeftPos, Time.deltaTime * smooth);
                yB.transform.localPosition = Vector3.Slerp(yB.transform.localPosition, targetRightPos, Time.deltaTime * smooth);
            }
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime * smooth);
            pA.transform.rotation = Quaternion.Slerp(pA.transform.rotation, Quaternion.identity, Time.deltaTime * smooth);
            pB.transform.rotation = Quaternion.Slerp(pB.transform.rotation, Quaternion.identity, Time.deltaTime * smooth);
            if (hpA != null && hpB != null)
            {
                hpA.transform.rotation = Quaternion.Slerp(hpA.transform.rotation, Quaternion.identity, Time.deltaTime * smooth);
                hpB.transform.rotation = Quaternion.Slerp(hpB.transform.rotation, Quaternion.identity, Time.deltaTime * smooth);
            }
            xA.transform.localPosition = Vector3.Slerp(xA.transform.localPosition, targetLeftPos, Time.deltaTime * smooth);
            xB.transform.localPosition = Vector3.Slerp(xB.transform.localPosition, targetRightPos, Time.deltaTime * smooth);
            if (yA != null && yB != null)
            {
                yA.transform.localPosition = Vector3.Slerp(yA.transform.localPosition, targetRightPos, Time.deltaTime * smooth);
                yB.transform.localPosition = Vector3.Slerp(yB.transform.localPosition, targetLeftPos, Time.deltaTime * smooth);
            }
        }
    }

    public void StartFlipping ()
    {
        fliped = !fliped;
    }
}
