using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InactiveOnAwake : MonoBehaviour
{
    private void Awake()
    {
        foreach(Transform child in gameObject.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
