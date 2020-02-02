using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class IntroThenLoop : MonoBehaviour
{
    public AudioClip startClip;
    public AudioClip loopClip;

    private Coroutine soundCoroutine;
    void Start()
    {
        GetComponent<AudioSource>().loop = true;
        soundCoroutine = StartCoroutine(playSound());
    }

    IEnumerator playSound()
    {
        GetComponent<AudioSource>().clip = startClip;
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length);
        GetComponent<AudioSource>().clip = loopClip;
        GetComponent<AudioSource>().Play();
    }

    public void Restart()
    {
        if (soundCoroutine != null) {
            StopCoroutine(soundCoroutine);
        }
        GetComponent<AudioSource>().Stop();
        Start();
    }
}
