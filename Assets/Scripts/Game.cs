using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : Singleton<Game>
{
    public VoyageManager voyage;
    public Ship ship;

    public Inventory player1Inventory;
    private void Awake()
    {
        ship = GameObject.Find("Ship").GetComponent<Ship>();
        voyage = GetComponent<VoyageManager>();
        UIManager.Instance.player1Inventory.AssignInventory(player1Inventory);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            voyage.StartVoyage();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            voyage.ResetVoyage();
        }
    }
}
