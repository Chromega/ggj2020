using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneUI : MonoBehaviour
{
    protected Joystick joystick;

    // Start is called before the first frame update
    void Start()
    {
      joystick = FindObjectOfType<Joystick>();
    }

    // Update is called once per frame
    void Update()
    {
      /*
      var rigidbody = GetComponent<RigidBody>();
      rigidbody.velocity = new Vector3(joystick.Horizontal * 100f, rigidbody.velocity.y, joystick.Vertical * 100f);
      */
      Vector2 joystickDir = new Vector2(joystick.Horizontal * 100f, joystick.Vertical * 100f);
      Debug.Log(joystickDir);
    }
}
