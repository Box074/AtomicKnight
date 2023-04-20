using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertRangeCheck : MonoBehaviour
{
    [NonSerialized]
    public List<GameObject> gameObjects = new List<GameObject>();
    public bool AnyGameObject => gameObjects.Count > 0;
    private void Awake()
    {
        gameObjects = new List<GameObject>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var go = collision.gameObject;
        if(!gameObjects.Contains(go))
        {
            gameObjects.Add(go);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        gameObjects.Remove(collision.gameObject);
    }
}
