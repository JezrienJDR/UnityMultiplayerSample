using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBase : MonoBehaviour
{
    public Material mat;
    public string id;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;

        if(mat == null)
        {
            Debug.Log("Mat is null at start.");
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void ColourChange(float r, float g, float b)
    {
        //if (mat == null)
        //{
        //    Debug.Log("Mat is null at ColourChangeCall.");
        //}

        //mat = GetComponent<Renderer>().material;


        GetComponent<Renderer>().material.SetColor("_Color", new Color(r/255.0f, g/255.0f, b/255.0f));

        //if (mat == null)
        //{
        //    Debug.Log("Mat is null after second assign.");
        //}

        //mat.color = new Color(r, g, b);
    }
}
