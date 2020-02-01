using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerController : PlayerControllerBase
{
    public string[] inventory;


    public override string[] GetInventory()
    {
        return inventory;
    }

    public override Vector2 GetInput()
    {
        //could switch off of playerIdx property
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public override void SetInput(Vector2 pos)
    {
        throw new System.NotImplementedException();
    }

    public override void SetInventory(string[] inventory)
    {
        this.inventory = inventory;
    }
}
