using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoyageManager : MonoBehaviour
{
    public float distanceProgress = 0f;

    // Length of the voyage
    public float goalDistance = 10.0f;

    // Is the voyage going right now
    public bool underway = false;

    // The ship object with ship state
    private Ship ship;

    // Start is called before the first frame update
    void Start()
    {
        ship = Game.Instance.ship;
    }

    // Update is called once per frame
    void Update()
    {
        if (underway)
        {
            float speed = ship.GetMovementSpeed();
            distanceProgress += speed * Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ship.BreakDownRandomly();
        }
    }

    public float GetCurrentProgress()
    {
        return distanceProgress / goalDistance;
    }

    public void ResetVoyage()
    {
        distanceProgress = 0f;
        underway = false;
        ship.StopSailing();
        ship.Reset();
    }

    public void StartVoyage()
    {
        underway = true;
        ship.Sail();
    }
}
