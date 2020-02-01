using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerControllerBase : MonoBehaviour
{
    //Not currently synched to net controller
    protected int playerIdx;

    public event System.Action<int> onItemUsed;
    public event System.Action<int> onItemDropped;

    protected void triggerItemUsed(int index) {
        onItemUsed?.Invoke(index);
    }
    protected void triggerItemDropped(int index) {
        onItemDropped?.Invoke(index);
    }

    public virtual void SetPlayerIndex(int idx)
    {
        playerIdx = idx;
    }

    //The client sets position from the UI here!
    public abstract void SetInput(Vector2 pos);

    //The host sets the inventory.  Should call this after picking up/using an item.
    //Could make 'add inventory item' and 'remove inventory item' as useful helpers
    //Has to be an array...List<> is unsupported out of the box
    public abstract void SetInventory(string[] inventory);

    public abstract Vector2 GetInput();

    public abstract string[] GetInventory();
}
