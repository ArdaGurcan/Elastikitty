using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checks : MonoBehaviour
{

    GameObject[] check;
    float time = 10;
    // Start is called before the first frame update
    private void Awake()
    {
        check = GameObject.FindGameObjectsWithTag("Check");
        Debug.Log(check[0]);
    }

    void Update()
    {
        /*
        Debug.Log(time);
        if(time <= 0) {

        }
        else if(time > 9) {
            time -= Time.deltaTime;
        }
        else {
            if(Mathf.RoundToInt(time) % 2 == 0) {
                checks[i].SetActive(true);
                i++;
            }
            else {
                time -= Time.deltaTime;
            }
        }
        */
       
             check[0].SetActive(true);
        
    }


}
