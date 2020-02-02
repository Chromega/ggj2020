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
    private ItemSpawner itemSpawner;

    private float timeToNextBreakage;

    const float MIN_SECONDS_BETWEEN_BREAKS = 3f;
    const float MAX_SECONDS_BETWEEN_BREAKS = 5f;

    // Start is called before the first frame update
    void Start()
    {
        itemSpawner = GetComponent<ItemSpawner>();
        ship = Game.Instance.ship;
    }

    // Update is called once per frame
    void Update()
    {
        if (underway)
        {
            float speed = ship.GetMovementSpeed();
            distanceProgress += speed * Time.deltaTime;
            timeToNextBreakage = Mathf.Max(timeToNextBreakage - Time.deltaTime, 0f);
            if (timeToNextBreakage == 0)
            {
                itemSpawner.SpawnItemCount(1);
                ship.BreakDownRandomly();
                SetRandomTimeToBreakage();
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            itemSpawner.SpawnItemCount(1);
            ship.BreakDownRandomly();
        }
    }

    public void SetRandomTimeToBreakage()
    {
        timeToNextBreakage = Random.Range(MIN_SECONDS_BETWEEN_BREAKS, MAX_SECONDS_BETWEEN_BREAKS);
        Debug.Log($"Next breakage in {timeToNextBreakage} seconds...");
    }

    public float GetCurrentProgress()
    {
        return Mathf.Clamp(distanceProgress / goalDistance, 0f, 1f);
    }

    public void ResetVoyage()
    {
        distanceProgress = 0f;
        underway = false;
        ship.Restart();
    }

    public void StartVoyage()
    {
        underway = true;
        ship.Sail();
    }
}
