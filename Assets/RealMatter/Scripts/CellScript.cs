using UnityEngine;
using System.Collections;

public class CellScript : MonoBehaviour {

    public RMCell cell = null;

    public void Initialize(RMCell c)
    {
        cell = c;
        transform.position = c.x;
        transform.rotation = c.R.toQuat();
    }
	/*
    public void FixedUpdate()
    {
        if (cell != null)
        {
            //Assuming object initialized, add a check if this is a problem
            transform.position = cell.x;
            transform.rotation = cell.R.toQuat();
        }
	}*/
}
