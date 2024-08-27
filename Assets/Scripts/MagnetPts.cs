using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetPts : MonoBehaviour
{
    private Team team;
    private float speed;
    private Vector3 center = new Vector3 (0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        team = gameObject.GetComponentInParent(typeof(Team)) as Team;
    }

    // Update is called once per frame
    void Update()
    {
        float dis = Vector3.Distance(transform.position, center);
        if (dis > 50)
        {
            Invoke("Magnet", 0.3f);
            transform.position = Vector3.MoveTowards(transform.position, center, speed);
        }
        else
        {
            team.AddPoints();
            Destroy(this.gameObject);
        }
    }

    void Magnet ()
    {
        speed = 800;
        speed = speed * Time.deltaTime;
    }
}
