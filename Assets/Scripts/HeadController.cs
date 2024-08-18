using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadController : MonoBehaviour
{
    
    private bool eat;
    public CatController cat;

    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void OnEat(InputAction.CallbackContext context) 
    {
        if (context.performed) 
        {
            eat = true;
            Invoke("EatDelay", 1.0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (eat) 
        {
            GameObject item = collision.gameObject;
            Debug.Log("Eat");
            if (item != null && item.CompareTag("Edible")) {
                cat.stomachContents.Add(item);
                item.tag = "Ate";
                item.SetActive(false);
                audioManager.PlaySFX(audioManager.eat);
            }
        }
    }

    void EatDelay()
    {
        eat = false;
    }
}
