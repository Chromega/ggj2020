using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerController : PlayerControllerBase
{
    public string[] inventory;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            triggerItemUsed(0);
        }
    }

    public override string[] GetInventory()
    {
        return inventory;
    }

    public override Vector2 GetInput()
    {
        //could switch off of playerIdx property
        float inputX = 0;
        float inputY = 0;
        if (playerIdx == 0) {
            inputX = Input.GetAxis("P1_Horizontal");
            inputY = Input.GetAxis("P1_Vertical");
        } else if (playerIdx == 1) {
            inputX = Input.GetAxis("P2_Horizontal");
            inputY = Input.GetAxis("P2_Vertical");
        }
        return new Vector2(inputX, inputY);
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
