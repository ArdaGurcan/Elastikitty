using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))  // Ensure the player object has the "Player" tag
        {
           SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
