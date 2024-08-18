using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
   [SerializeField] AudioSource musicSource;
   [SerializeField] AudioSource SFXSource;

   public AudioClip background;
   public AudioClip eat;
   public AudioClip death;
   public AudioClip goal;


    private void Awake() {
       GameObject[] obj = GameObject.FindGameObjectsWithTag("Audiog");
       if (obj.Length > 1) {
        Destroy(this.gameObject);
       }
       else {
        DontDestroyOnLoad(this.gameObject);
       }
    }

   private void Start() 
   {
    musicSource.clip = background;
    musicSource.Play();
   }

   public void PlaySFX(AudioClip clip) 
   {
    SFXSource.PlayOneShot(clip);
   }
}
