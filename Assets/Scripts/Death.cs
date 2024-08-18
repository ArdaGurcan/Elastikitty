using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    public GameObject deathScreen;  // Assign your death screen canvas here in the Inspector
    public GameObject controls;

    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))  // Ensure the player object has the "Player" tag
        {
            // Activate the death screen
            audioManager.PlaySFX(audioManager.death);
            deathScreen.SetActive(true);
            controls.SetActive(false);
            // Optionally, you can stop the game here
            // Time.timeScale = 0;  // Freezes the game
        }
    }
}
