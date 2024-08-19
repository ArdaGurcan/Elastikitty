using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ButtonDoor : MonoBehaviour
{
    
    [SerializeField]
    Sprite buttonUp;
    [SerializeField]
    Sprite buttonDown;
    static bool doorOpen = false;
    bool colliding = false;
    bool wasColliding = false;
    static int collisions = 0;
    Door door;
    private void Awake() {
        door = GameObject.FindGameObjectWithTag("Door").GetComponent<Door>();
    }
    // private float timer;

    // private void Update() {
    //     if (timer > 0) {
    //         timer -= Time.deltaTime;
    //     }
    //     if(timer <= 0f) {
    //             door.CloseDoor();
    //     }
    // }

    void OnDrawGizmos() {
        Vector2 colliderOffset = new Vector2(transform.position.x - 0.01079774f, transform.position.y - 0.08182621f);
        Vector2 colliderSize = new Vector2(0.5700831f, 0.3116035f);
        Gizmos.color = colliding ? Color.green : Color.red;
        Gizmos.DrawWireCube(colliderOffset, colliderSize);
    }

    void FixedUpdate() {
        Vector2 colliderOffset = new Vector2(transform.position.x - 0.01079774f, transform.position.y - 0.08182621f);
        Vector2 colliderSize = new Vector2(0.5700831f, 0.3116035f);
        colliding = Physics2D.OverlapArea(colliderOffset - colliderSize / 2f, colliderOffset + colliderSize / 2f, 
            LayerMask.GetMask("head") | LayerMask.GetMask("body") | LayerMask.GetMask("butt")) != null;

        if (colliding && !wasColliding) {
            collisions++;
        } else if (!colliding && wasColliding) {
            collisions--;
        }
        wasColliding = colliding;

        if ((collisions > 0) && !doorOpen) {
            doorOpen = true;
            door.OpenDoor();
        } else if (collisions == 0 && doorOpen) {
            doorOpen = false;
            door.CloseDoor();
        }
    }

    void Update() {
        if (colliding) {
            GetComponent<SpriteRenderer>().sprite = buttonDown;
        } else {
            GetComponent<SpriteRenderer>().sprite = buttonUp;
        }
    }

    // private void OnTriggerEnter2D(Collider2D collider) {
    //     if (collider.CompareTag("Player")) {
    //         door.OpenDoor();
    //     }
    // }

    // private void OnTriggerStay2D(Collider2D collider) {
    //     if (collider.CompareTag("Player")) {
    //         timer = 1f;
    //     }
    // }
}
