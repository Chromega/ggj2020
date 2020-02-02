using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    // The default movement speed of the ship, per second
    public float baseMovementSpeed = 0.1f;

    // Movement penalty per engine
    public float movementSpeedPenalty = 0.05f;

    // Each speed object on the ship
    public List<Repairable> speedComponents;

    // Each steering object on the ship
    public List<Repairable> steeringComponents;

    // Each hull object on the ship
    public List<Repairable> hullComponents;
    // Each battle object on the ship
    public List<Repairable> battleComponents;

    public List<Repairable> allRepairables;
    public bool sailing = false;

    // Start is called before the first frame update
    void Start()
    {
        allRepairables = new List<Repairable>();
        // Right now just manually add each of the repairable categories
        foreach (Repairable c in speedComponents)
        {
            allRepairables.Add(c);
        }
        foreach (Repairable c in steeringComponents)
        {
            allRepairables.Add(c);
        }
        foreach (Repairable c in hullComponents)
        {
            allRepairables.Add(c);
        }
        foreach (Repairable c in battleComponents)
        {
            allRepairables.Add(c);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float GetMovementSpeed()
    {
        float movementSpeed = baseMovementSpeed;
        // Each broken component reduces speed by 5% multiplicatively
        foreach (Repairable component in allRepairables)
        {
            if (component.broken)
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
        int offset = Random.Range(0, allRepairables.Count);
        for (var i = offset; i <= offset + allRepairables.Count; i++)
        {
            if (!allRepairables[i % allRepairables.Count].broken)
            {
                allRepairables[i % allRepairables.Count].Break();
                break;
            }
        }
    }

    public void Restart()
    {
        StopSailing();
        foreach (Repairable component in allRepairables)
        {
            component.Repair();
        }
    }

    public float HullPercentage()
    {
        int broken = 0;
        for (int i = 0; i < allRepairables.Count; i++)
        {
            if (allRepairables[i].broken)
            {
                broken++;
            }
        }
        return Mathf.Clamp(1f - (broken / 10f), 0f, 1f);
    }
}
