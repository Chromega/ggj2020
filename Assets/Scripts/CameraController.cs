using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Ship ship;

    // Start is called before the first frame update
    void Start()
    {
        ship = Game.Instance.ship;
    }

    // Update is called once per frame
    void Update()
    {
        // Follow the ship
        transform.position = new Vector3(ship.transform.position.x + 8, 6, 6.5f);
    }
}
