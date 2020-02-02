using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class IntroThenLoop : MonoBehaviour
{
    public AudioClip startClip;
    public AudioClip loopClip;
    void Start()
    {
        GetComponent<AudioSource>().loop = true;
        StartCoroutine(playSound());
    }

    IEnumerator playSound()
    {
        GetComponent<AudioSource>().clip = startClip;
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length);
        GetComponent<AudioSource>().clip = loopClip;
        GetComponent<AudioSource>().Play();
    }
}
