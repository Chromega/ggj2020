using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    // The default movement speed of the ship, per second
    public float baseMovementSpeed = 0.1f;

    // Movement penalty per engine
    public float movementSpeedPenalty = 0.2f;

    // Each speed object on the ship
    public List<Repairable> speedComponents;

    // Each steering object on the ship
    public List<Repairable> steeringComponents;

    // Each hull object on the ship
    public List<Repairable> hullComponents;
    // Each battle object on the ship
    public List<Repairable> battleComponents;

    public List<List<Repairable>> allRepairables;
    public bool sailing = false;

    // Start is called before the first frame update
    void Start()
    {
        allRepairables = new List<List<Repairable>>();
        // Right now just manually add each of the repairable categories
        allRepairables.Add(speedComponents);
        allRepairables.Add(steeringComponents);
        allRepairables.Add(hullComponents);
        allRepairables.Add(battleComponents);
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
        foreach (Repairable engine in speedComponents)
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

    // Break down something random!
    public void BreakDownRandomly()
    {
        int system = Random.Range(0, allRepairables.Count);
        allRepairables[system][Random.Range(0, allRepairables[system].Count)].Break();
    }

    public void Reset()
    {
        transform.position = new Vector3(0f, 0f, 0f);
    }
}
