using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RollControl : MonoBehaviour
{
    // Variables to store the input values
    private float buttMove;
    private float headMove;
    private bool buttJump;
    private bool headJump;


    public void OnButtMove(InputAction.CallbackContext context)
    {
        // Gets the value of the input as float since it can be decimal in the case of a joystick
        // This value is either -1 or 1 depending on direction pressed on keyboard
        // Reads into input variable to immediately receive input and then uses it when drawing the next frame
        buttMove = context.ReadValue<float>();
    }

    public void OnHeadMove(InputAction.CallbackContext context)
    {
        headMove = context.ReadValue<float>();
    }

    public void OnButtJump(InputAction.CallbackContext context)
    {
        // If the button is pressed
        if (context.performed)
        {
            buttJump = true;
        }
    }

    public void OnHeadJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            headJump = true;
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        // // If the button is pressed
        // if (buttJump || headJump)
        // {
        //     // Print the message
        //     print("Butt Jump");
        //     GetComponent<Rigidbody2D>().AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        //     // Reset the button press
        //     buttJump = false;
        //     headJump = false;
        // }

        // If the button is pressed
        if (Mathf.Abs(buttMove) > 0.01)
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(buttMove, 0) * 2);
        }
        if (Mathf.Abs(headMove) > 0.01)
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(headMove, 0) * 2);
        }
    }
}
