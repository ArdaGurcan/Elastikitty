using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Animator doorAnimator;  
    
    public void Start() {
        doorAnimator = GetComponent<Animator>();
    }

    public void OpenDoor()
    {
        doorAnimator.SetTrigger("Open");
    }

    public void CloseDoor()
    {
         doorAnimator.SetTrigger("Close");
    }
}
