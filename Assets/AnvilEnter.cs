using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnvilEnter : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Trigger")) {
            GameObject.FindGameObjectWithTag("Trigger").SetActive(false);
            GameObject.FindGameObjectWithTag("Trigger").SetActive(false);
        }
    }
}
