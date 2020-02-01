using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Inventory inventory;
    public int velocity = 50;
    public Vector2 movement;
    public int playerNum;
    public PlayerControllerBase playerControllerBase;

    private void Awake()
    {
        inventory = new Inventory();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = playerControllerBase.GetInput();
        float inputX = input.x;
        float inputY = input.y;

        Vector3 convertedX = Camera.main.transform.right*inputX;
        convertedX.y = 0;
        float oldXMagnitude = Mathf.Abs(inputX);
        float newXMagnitude = convertedX.magnitude;
        if (newXMagnitude == 0)
            convertedX = Vector3.zero;
        else
            convertedX *= oldXMagnitude/newXMagnitude;


        Vector3 convertedY = Camera.main.transform.up*inputY;
        convertedY.y = 0;
        float oldYMagnitude = Mathf.Abs(inputY);
        float newYMagnitude = convertedY.magnitude;
        if (newYMagnitude == 0)
            convertedY = Vector3.zero;
        else
            convertedY *= oldYMagnitude/newYMagnitude;

        transform.position += velocity * (convertedX + convertedY) * Time.deltaTime;
    }

    public void SetPlayerController(PlayerControllerBase p) {
        playerControllerBase = p;
    }
}
