using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : Singleton<Game>
{
    public VoyageManager voyage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            voyage.StartVoyage();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            voyage.ResetVoyage();
        }
    }
}
