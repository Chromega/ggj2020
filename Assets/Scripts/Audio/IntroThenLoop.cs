using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class IntroThenLoop : MonoBehaviour
{
    public AudioClip startClip;
    public AudioClip loopClip;
    public AudioSource audioSource;

    private Coroutine soundCoroutine;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        soundCoroutine = StartCoroutine(playSound());
    }

    IEnumerator playSound()
    {
        audioSource.clip = startClip;
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
        audioSource.clip = loopClip;
        audioSource.Play();
    }

    public void Restart()
    {
        if (soundCoroutine != null) {
            StopCoroutine(soundCoroutine);
        }
        audioSource.Stop();
        Start();
    }
}
