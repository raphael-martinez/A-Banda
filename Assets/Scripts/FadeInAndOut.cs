using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInAndOut : MonoBehaviour
{
    public float duration;
    public float t = 10f;
    public Material material;
    public AnimationCurve inOut;
    private float alpha;

    // Start is called before the first frame update
    void Start()
    {
        material.SetFloat("Vector1_12abe2f1b3de49cbb6fefeae038fcf68", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (t < duration)
        {
            t += Time.deltaTime;
            alpha = inOut.Evaluate(t / duration);
            material.SetFloat("Vector1_12abe2f1b3de49cbb6fefeae038fcf68", alpha);
        }
    }
}
