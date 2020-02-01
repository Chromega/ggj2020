using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Inventory inventory;
    public int velocity = 50;
    public Vector2 movement;
    public int playerNum;
    private Collider myCollider;

    private void Awake()
    {
        inventory = new Inventory();
    }
    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = 0;
        float inputY = 0;
        if (playerNum == 0) {
            inputX = Input.GetAxis("P1_Horizontal");
            inputY = Input.GetAxis("P1_Vertical");
        } else if (playerNum == 1) {
            inputX = Input.GetAxis("P2_Horizontal");
            inputY = Input.GetAxis("P2_Vertical");
        }

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
}
