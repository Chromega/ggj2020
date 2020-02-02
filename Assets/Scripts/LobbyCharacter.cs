using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCharacter : MonoBehaviour
{
    Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        gameObject.SetActive(false);
    }

    public void Appear()
    {
        gameObject.SetActive(true);
        StartCoroutine(Slide(new Vector3(startPos.x - 10, startPos.y, startPos.z), startPos, .5f));
    }

    public void Exit()
    {
        if (gameObject.activeSelf)
            StartCoroutine(Slide(startPos, new Vector3(startPos.x + 10, startPos.y, startPos.z), .5f));
    }
    
    IEnumerator Slide(Vector3 start, Vector3 end, float time)
    {
        float t = 0;
        while (t < time)
        {
            float lerp = t / time;
            transform.position = Vector3.Lerp(start, end, lerp);
            yield return null;
            t += Time.deltaTime;
        }
        transform.position = end;
    }
}
