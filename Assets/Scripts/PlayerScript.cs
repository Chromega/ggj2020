using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Inventory inventory;
    public Vector2 velocity = new Vector2(50, 50);
    public Vector2 movement;
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
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        transform.position += new Vector3(velocity.x * inputX, 0, velocity.y * inputY) * Time.deltaTime;
    }
}
