using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotorobotDeath : MonoBehaviour
{
    
    void Explode()
    {
        Debug.Log("Die");
        //////////////
        Destroy(gameObject);
    }
}
