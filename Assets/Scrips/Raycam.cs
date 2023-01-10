using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycam : MonoBehaviour
{


    float discancia = 5f;
    float escala =0.1f;

    Camera cam;
    public LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        GameObject puntero;
    }

    // Update is called once per frame
    void Update() 
    {
        GameObject puntero;
        
        puntero = GameObject.Find("final");
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = discancia;
        puntero.transform.localScale = new Vector3(escala, escala, escala);
        mousePos = cam.ScreenToWorldPoint(mousePos);
        puntero.transform.position = mousePos;
    }
}
