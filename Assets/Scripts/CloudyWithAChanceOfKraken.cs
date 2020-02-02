using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudyWithAChanceOfKraken : MonoBehaviour
{
    public Animator animator;

    private float timeToKraken = 50f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        timeToKraken -= Time.deltaTime;
        if (timeToKraken < 0 && Random.value < 0.01)
        {
            animator.SetTrigger("kraken");
            timeToKraken = Random.Range(10, 15);
        }
    }

}