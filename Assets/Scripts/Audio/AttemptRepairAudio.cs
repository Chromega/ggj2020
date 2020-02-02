using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AttemptRepairAudio : MonoBehaviour
{
    public AudioClip wrongRepairClip;
    public AudioClip rightRepairClip;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playWrongRepair()
	{
        GetComponent<AudioSource>().clip = wrongRepairClip;
        GetComponent<AudioSource>().Play();
    }

    public void playRightRepairClip()
	{
        GetComponent<AudioSource>().clip = rightRepairClip;
        GetComponent<AudioSource>().Play();
    }

    public static AttemptRepairAudio Instance { get; private set; }
}
