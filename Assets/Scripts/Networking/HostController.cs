using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostController : MonoBehaviour
{
    // Start is called before the first frame update
    public static HostController Instance { get; private set; }


    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
