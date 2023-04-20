using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AudioGroup : ScriptableObject
{
    public AudioClip[] clips;
    public AudioClip GetClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }
}
