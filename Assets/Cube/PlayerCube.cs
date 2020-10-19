using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCube : MonoBehaviour
{
    public float speed = 1.0f;
    private float baseSpeed;
    // Start is called before the first frame update
    void Start()
    {
        baseSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        speed = baseSpeed * Time.deltaTime;

        if(Input.GetAxis("Horizontal") > 0.1)
        {
            transform.Translate(speed, 0, 0);
        }
        else if (Input.GetAxis("Horizontal") < -0.1)
        {
            transform.Translate(-speed, 0, 0);
        }

        if (Input.GetAxis("Vertical") > 0.1)
        {
            transform.Translate(0, speed, 0);
        }
        else if (Input.GetAxis("Vertical") < -0.1)
        {
            transform.Translate(0,-speed, 0);
        }
    }
}
