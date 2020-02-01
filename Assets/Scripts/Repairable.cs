using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repairable : MonoBehaviour
{
    // What is the state of the current object
    public bool _broken = false;
    public bool broken 
    {
        get 
        {
            return _broken;
        }
    }

    public MeshRenderer mesh;

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (broken)
        {
            mesh.material.color = Color.red;
        }
        else 
        {
            mesh.material.color = Color.white;
        }
    }

    // Set the state of the object to broken
    public void Break()
    {
        _broken = true;
    }

    // Set the state of the object to fixed
    public void Repair()
    {
        _broken = false;
    }
}
