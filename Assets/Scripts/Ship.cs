using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    // The default movement speed of the ship, per second
    public float baseMovementSpeed = 0.1f;

    // Movement penalty per engine
    public float movementSpeedPenalty = 0.2f;

    // Each engine object on the ship
    public List<Repairable> engines;

    public bool sailing = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sailing)
        {
            transform.position += new Vector3(GetMovementSpeed(), 0f, 0f); 
        }
    }

    public float GetMovementSpeed()
    {
        float movementSpeed = baseMovementSpeed;
        // Each broken engine reduces speed by 20% multiplicatively
        foreach (Repairable engine in engines)
        {
            if (engine.broken)
            {
                movementSpeed = movementSpeed * movementSpeedPenalty;
            }
        }
        return baseMovementSpeed;
    }

    // Start the voyage!
    public void Sail()
    {
        sailing = true;
    }

    // Stop sailing
    public void StopSailing()
    {
        sailing = false;
    }

    public void Reset()
    {
        transform.position = new Vector3(0f, 0f, 0f);
    }
}
