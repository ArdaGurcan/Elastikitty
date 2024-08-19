using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonDoor : MonoBehaviour
{
    // Start is called before the first frame update
    Door door;
    private void Awake() {
        door = GameObject.FindGameObjectWithTag("Door").GetComponent<Door>();
    }
    private float timer;

    private void Update() {
        if (timer > 0) {
            timer -= Time.deltaTime;
        }
        if(timer <= 0f) {
                door.CloseDoor();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player")) {
            door.OpenDoor();
        }
    }

    private void OnTriggerStay2D(Collider2D collider) {
        if (collider.CompareTag("Player")) {
            timer = 1f;
        }
    }
}
