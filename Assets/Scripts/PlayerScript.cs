﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int playerNum;
    public PlayerControllerBase playerControllerBase;

    public Inventory inventory;

    public int speed = 50;
    public Vector3 movement;
    private Quaternion qTo;

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

        Vector3 convertedX = Camera.main.transform.right * inputX;
        convertedX.y = 0;
        float oldXMagnitude = Mathf.Abs(inputX);
        float newXMagnitude = convertedX.magnitude;
        if (newXMagnitude == 0) {
            convertedX = Vector3.zero;
        } else {
            convertedX *= oldXMagnitude / newXMagnitude;
        }


        Vector3 convertedY = Camera.main.transform.up * inputY;
        convertedY.y = 0;
        float oldYMagnitude = Mathf.Abs(inputY);
        float newYMagnitude = convertedY.magnitude;
        if (newYMagnitude == 0) {
            convertedY = Vector3.zero;
        } else {
            convertedY *= oldYMagnitude / newYMagnitude;
        }

        movement = convertedX + convertedY;
        transform.position += speed * movement * Time.deltaTime;
        if (movement != Vector3.zero) {
            qTo = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, qTo, Time.deltaTime * 20);
        }
    }

    public void SetPlayerController(PlayerControllerBase p) {
        playerControllerBase = p;
    }
}
