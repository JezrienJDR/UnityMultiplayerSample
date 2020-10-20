using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBase : MonoBehaviour
{
    public Material mat;
    public string id;

    public float timeSinceLastUpdate = 0;
    public float deadTime;

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
        timeSinceLastUpdate += Time.deltaTime;
        if(timeSinceLastUpdate > 3.0f)
        {
            gameObject.SetActive(false);
            //DestroyImmediate(gameObject);
        }
    }

    public void ColourChange(float r, float g, float b)
    {
        timeSinceLastUpdate = 0.0f;


        GetComponent<Renderer>().material.SetColor("_Color", new Color(r/255.0f, g/255.0f, b/255.0f));

        //if (mat == null)
        //{
        //    Debug.Log("Mat is null after second assign.");
        //}

        //mat.color = new Color(r, g, b);
    }
}
